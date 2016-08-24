using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SWF = System.Windows.Forms;

using GestureRecognitionLib;
using LfS.GestureRecognitionTests;
using GestureRecognitionLib.CHnMM;

namespace Visualizer
{
    public struct AreaData
    {
        public double X;
        public double Y;
        public double Radius;
        public double ToleranceRadius;

        public AreaData(string csvLine)
        {
            var split = csvLine.Split(';');

            X = Convert.ToDouble(split[0]);
            Y = Convert.ToDouble(split[1]);
            Radius = Convert.ToDouble(split[2]);
            ToleranceRadius = Convert.ToDouble(split[3]);
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private AreaData[] loadedAreas;
        //private double offsetX;
        //private double offsetY;


        private CHnMMRecognitionSystem currentSystem;
        private Dictionary<string, SetOfGestureTraces> cachedDatasets = new Dictionary<string, SetOfGestureTraces>(50);
 
        public MainWindow()
        {
            InitializeComponent();
        }

        //private void btnLoadAreas_Click(object sender, RoutedEventArgs e)
        //{
        //    SWF.OpenFileDialog ofd = new SWF.OpenFileDialog();

        //    ofd.DefaultExt = ".csv";
        //    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        LoadAreas(ofd.FileName);
        //    }
        //}

        //private void LoadAreas(string csvFilePath)
        //{
        //    loadedAreas = File.ReadLines(csvFilePath).Skip(1).Select(line => new AreaData(line)).ToArray();

        //    //move translationinvariant gestures to canvas center
        //    var minX = loadedAreas.Min(a => a.X);
        //    var maxX = loadedAreas.Max(a => a.X);

        //    var minY = loadedAreas.Min(a => a.Y);
        //    var maxY = loadedAreas.Max(a => a.Y);

        //    double width = maxX - minX;
        //    double height = maxY - minY;

        //    double traceCenterX = minX + (width / 2);
        //    double traceCenterY = minY + (height / 2);

        //    offsetX = 0.5 - traceCenterX;
        //    offsetY = 0.5 - traceCenterY;

        //    DrawCanvas.Children.Clear();
        //    DrawLoadedAreas();
        //}

        //private void DrawLoadedAreas()
        //{
        //    var h = DrawCanvas.Height;
        //    var w = DrawCanvas.Width;

        //    //int i = 1;
        //    foreach (var area in loadedAreas)
        //    {
        //        var absX = (area.X+offsetX) * w;
        //        var absY = (area.Y+offsetY) * h;
        //        var absCircleW = area.Radius * w * 2;
        //        var absCircleH = area.Radius * h * 2;
        //        var absTolCircleW = area.ToleranceRadius * w * 2;
        //        var absTolCircleH = area.ToleranceRadius * h * 2;

        //        var circle = new Ellipse();
        //        circle.Width = absCircleW;
        //        circle.Height = absCircleH;
        //        circle.Fill = null;
        //        circle.Stroke = new SolidColorBrush(Color.FromRgb(0,0,0));
        //        circle.SetValue(Canvas.LeftProperty, absX - absCircleW / 2);
        //        circle.SetValue(Canvas.TopProperty, absY - absCircleH / 2);

        //        var tolCircle = new Ellipse();
        //        tolCircle.Width = absTolCircleW;
        //        tolCircle.Height = absTolCircleH;
        //        tolCircle.Fill = null;
        //        tolCircle.Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100));
        //        tolCircle.SetValue(Canvas.LeftProperty, absX - absTolCircleW / 2);
        //        tolCircle.SetValue(Canvas.TopProperty, absY - absTolCircleH / 2);

        //        //var curFillBrush = baseFillBrush.Clone();
        //        //curFillBrush.Opacity = opacity;

        //        //circle.Fill = curFillBrush;
        //        ////circle.SetValue(Ellipse.FillProperty, FillBrush);
        //        //circle.Stroke = StrokeBrush;
        //        //circle.SetValue(Canvas.LeftProperty, (double)(p.X * 600 - size));
        //        //circle.SetValue(Canvas.TopProperty, (double)(p.Y * 600 - size));

        //        DrawCanvas.Children.Add(circle);
        //        DrawCanvas.Children.Add(tolCircle);
        //    }
        //}

        public Configuration getConfiguration()
        {
            var conf = new Configuration();

            conf.areaPointDistance = double.Parse(tb_areaPointDistance.Text);
            conf.distEstName = cmb_distEst.Text;
            conf.hitProbability = double.Parse(tb_hitProbability.Text); 
            conf.isTranslationInvariant = cb_TranslationInvariant.IsChecked.Value;
            conf.minRadiusArea = double.Parse(tb_minRadius.Text); 
            conf.nAreaForStrokeMap = int.Parse(tb_nAreas.Text); 
            conf.toleranceFactorArea = double.Parse(tb_toleranceFactor.Text);
            conf.useAdaptiveTolerance = cb_AdaptiveTolerance.IsChecked.Value;
            conf.useFixAreaNumber = cb_useFixedAreaNumber.IsChecked.Value; 
            conf.useSmallestCircle = cb_SmallestCircle.IsChecked.Value;

            return conf;
        }

        public SetOfGestureTraces getSetGestureTraces(string name)
        {
            SetOfGestureTraces result;
            if (cachedDatasets.TryGetValue(name, out result))
            {
                return result;
            }
            else
            {
                result = new SetOfGestureTraces(DataSets.getGestureSet(name));
                cachedDatasets[name] = result;
                return result;
            }
        }

        public CHnMMRecognitionSystem createTrainedRecognitionSystem(Configuration conf, string gestureSetName, int nSubsets, int subsetIndex, bool shuffle)
        {
            var system = new CHnMMRecognitionSystem(conf);

            var gesturesData = getSetGestureTraces(gestureSetName);

            gesturesData.createSubsets(nSubsets, shuffle);

            gesturesData.trainRecognitionSystem(system, subsetIndex);

            return system;
        }

        public void fillGestureList()
        {
            lb_Gestures.ItemsSource = currentSystem.getKnownGesturenames().OrderBy(g => g);
        }

        public void fillTransitionDataGrid(string gestureName)
        {
            var gestureRep = currentSystem.getTrajectoryModel(gestureName);
            var chnmm = gestureRep.CHnMM;

            dg_Transitions.ItemsSource = chnmm.getTransitions().Select(t => new {Name = t.Dist.GetType().Name, Param1=t.Dist.Parameter1, Param2=t.Dist.Parameter2 });
        }

        public static void DrawStrokeMap(GestureRecognitionLib.CHnMM.StrokeMap strokeMap, Canvas DrawCanvas, Color color, bool isTranslationInvariant)
        {
            var h = DrawCanvas.ActualHeight;
            var w = DrawCanvas.ActualWidth;

            var offsetX = 0d;
            var offsetY = 0d;

            if (isTranslationInvariant)
            {
                //move translationinvariant gestures to canvas center
                var minX = strokeMap.Areas.Cast<Circle>().Min(a => a.X);
                var maxX = strokeMap.Areas.Cast<Circle>().Max(a => a.X);

                var minY = strokeMap.Areas.Cast<Circle>().Min(a => a.Y);
                var maxY = strokeMap.Areas.Cast<Circle>().Max(a => a.Y);

                double width = maxX - minX;
                double height = maxY - minY;

                double traceCenterX = minX + (width / 2);
                double traceCenterY = minY + (height / 2);

                offsetX = 0.5 - traceCenterX;
                offsetY = 0.5 - traceCenterY;
            }

            //int i = 1;
            foreach (var iarea in strokeMap.Areas)
            {
                var area = iarea as Circle;

                var absX = (area.X + offsetX) * w;
                var absY = (area.Y + offsetY) * h;
                var absCircleW = area.Radius * w * 2;
                var absCircleH = area.Radius * h * 2;
                var absTolCircleW = area.ToleranceRadius * w * 2;
                var absTolCircleH = area.ToleranceRadius * h * 2;

                var circle = new Ellipse();
                circle.Width = absCircleW;
                circle.Height = absCircleH;
                circle.Fill = null;
                circle.Stroke = new SolidColorBrush(color);
                circle.StrokeThickness = 1.5;
                circle.SetValue(Canvas.LeftProperty, absX - absCircleW / 2);
                circle.SetValue(Canvas.TopProperty, absY - absCircleH / 2);

                var tolCircle = new Ellipse();
                tolCircle.Width = absTolCircleW;
                tolCircle.Height = absTolCircleH;
                tolCircle.Fill = null;
                tolCircle.Stroke = new SolidColorBrush(Color.Multiply(color, 0.9f));
                tolCircle.StrokeDashArray = new DoubleCollection(new double[] {3});
                //tolCircle.StrokeDashOffset = 2;
                tolCircle.StrokeThickness = 1;
                tolCircle.SetValue(Canvas.LeftProperty, absX - absTolCircleW / 2);
                tolCircle.SetValue(Canvas.TopProperty, absY - absTolCircleH / 2);

                //var curFillBrush = baseFillBrush.Clone();
                //curFillBrush.Opacity = opacity;

                //circle.Fill = curFillBrush;
                ////circle.SetValue(Ellipse.FillProperty, FillBrush);
                //circle.Stroke = StrokeBrush;
                //circle.SetValue(Canvas.LeftProperty, (double)(p.X * 600 - size));
                //circle.SetValue(Canvas.TopProperty, (double)(p.Y * 600 - size));

                DrawCanvas.Children.Add(circle);
                DrawCanvas.Children.Add(tolCircle);
            }
        }

        private void btn_CreateSystem_Click(object sender, RoutedEventArgs e)
        {
            var conf = getConfiguration();
            var gestureSetName = cmb_gestureSet.Text;
            var nSubsets = int.Parse(tb_nSubsets.Text);
            var subsetIndex = int.Parse(tb_SubsetIndex.Text);
            var shuffle = cb_shuffled.IsChecked.Value;

            currentSystem = createTrainedRecognitionSystem(conf, gestureSetName, nSubsets, subsetIndex, shuffle);

            fillGestureList();
        }

        public void DrawSelectedStrokeMap()
        {
            var gestureName = lb_Gestures.SelectedItem.ToString();

            var rng = new Random();
            var color = Color.FromRgb((byte)rng.Next(256), (byte)rng.Next(256), (byte)rng.Next(256));

            var gestureRep = currentSystem.getTrajectoryModel(gestureName);
            var strokeMap = gestureRep.StrokeMap;

            DrawStrokeMap(strokeMap, DrawCanvas, color, cb_TranslationInvariant.IsChecked.Value);
            fillTransitionDataGrid(gestureName);
        }

        private void lb_Gestures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawSelectedStrokeMap();
        }

        private void btn_ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            DrawCanvas.Children.Clear();
        }

        private void lb_Gestures_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DrawSelectedStrokeMap();

            //var newWin = new StrokeMap();
            //newWin.Show();
        }
    }
}
