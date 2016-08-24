using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using LfS.GestureDatabase;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Shapes;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.Windows.Forms.DataVisualization.Charting;
using LfS.ModelLib;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;

namespace LfS.GestureDatabaseViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Controls.Image bgimg = new System.Windows.Controls.Image();

        //private FeatureGenerator[] features = {
                                              //    new XFeatureGenerator(),
                                              //    new YFeatureGenerator(),
                                              //    new DistanceFeatureGenerator(),
                                              //    new StartDistanceFeatureGenerator(),
                                              //    new AngleFeatureGenerator(),
                                              //    new DirectionFeatureGenerator()
                                              //    //new VelocityFeatureGenerator(),
                                              //    //new AccelerationFeatureGenerator(),
                                              //    //new StartDistanceFeatureGenerator()
                                              //};

        public MainWindow()
        {
            InitializeComponent();
            treeView1.DataContext = new TraceTreeViewModel();
            cbFeature.DataContext = new[] 
            {
                new {Name = "X", Feature="X"},
                new {Name = "Y", Feature="Y"},
                new {Name = "X interpolated", Feature="X interpolated"},
                new {Name = "Y interpolated", Feature="Y interpolated"},
                /*
                new {Name = "Direction", Feature="Direction"},
                new {Name = "StartDistance", Feature="StartDistance"},
                new {Name = "StartDirection", Feature="StartDirection"},
                new {Name = "Curvature", Feature="Curvature"},*/
                new {Name = "Trace", Feature="Trace"},
                new {Name = "Trace interpolated", Feature="Trace interpolated"},
                new {Name = "Velocity X", Feature="Velocity X"},
                new {Name = "Velocity Y", Feature="Velocity Y"}
            };
            chartArea.AxisX.Minimum = 0;
            cbFeature.SelectedIndex = 0;
            cbGraphType.SelectedIndex = 0;
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //clear canvas
            if (!cbKeepDrawings.IsChecked.Value)
            {
                var circles = canvas1.Children.OfType<Ellipse>().ToList();
                circles.ForEach(c => canvas1.Children.Remove(c));
                //canvas1.Children.Clear();
                //canvas1.Children.Add(bgimg);
            }

            var selItem = treeView1.SelectedItem;

            if (!(selItem is TraceViewModel)) return;
            var traceID = ((TraceViewModel)selItem).ID;

            using (var ctx = new dbEntities())
            {
                var strokes = from t in ctx.Touches where t.Trace.Id == traceID orderby t.Time group t by t.FingerId into stroke select new { fingerID = stroke.Key, Touches = stroke.AsEnumerable() };

                var colors = new System.Windows.Media.Brush[] { System.Windows.Media.Brushes.Blue, System.Windows.Media.Brushes.Red, System.Windows.Media.Brushes.Green, System.Windows.Media.Brushes.Yellow, System.Windows.Media.Brushes.Violet, System.Windows.Media.Brushes.Brown, System.Windows.Media.Brushes.Pink };



                int i = 0;
                foreach (var stroke in strokes)
                {
                    
                    DrawTrace(stroke.Touches, 10, colors[i++], null);
                }
            }

            //var selItem = treeView1.SelectedItem;

            //if (!(selItem is TraceViewModel)) return;
            //var traceID = ((TraceViewModel)selItem).ID;

            //Bitmap img = new Bitmap(@"D:\Dropbox\LfS\Code\GestureCollectingInterface\canvasBG.png");
            //var g = Graphics.FromImage(img);
            //Pen pen = new Pen(Color.Black, 2);
            //System.Windows.Media.Brush FillBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(100,255,0,0));



           
            //using (var ctx = new dbEntities())
            //{
                //var points = from t in ctx.Touches where t.Trace.Id == traceID orderby t.Time select t;
                //DrawTrace(points, 10, FillBrush, null);
                //var series = CreateAccelerationSeries(points);
                
                //var chartArea = new ChartArea("Trace");
                //chartBox.ChartAreas.Add(chartArea);
                //chartBox.Series.Add(series[2]);
                //chartArea.AxisX.Maximum = 200.0;
                //chartArea.AxisX.Minimum = 0.0;

                /*
                var screenPoints = new PointCollection();
                foreach(var p in points)
                    screenPoints.Add(new System.Windows.Point(p.X*600,p.Y*600));

                Path path = new Path();
                var pathGeo = new PathGeometry();
                var pathFigure = new PathFigure();
                var segment = new PolyQuadraticBezierSegment(screenPoints, true);
                pathFigure.Segments.Add(segment);
                pathGeo.Figures.Add(pathFigure);
                path.Data = pathGeo;
                //path.Fill = b;
                path.Stroke = b;
                canvas1.Children.Add(path);
                //int size = 10;
                //foreach (var p in points)
                //{
                //    var point = new System.Windows.Point((double)p.X, (double)p.Y);

                //    var circle = new Ellipse();
                //    //var rect = new Rectangle((int)(p.X * 600 - size), (int)(p.Y * 600 - size), size * 2, size * 2);
                //    circle.Width = size * 2;
                //    circle.Height = size * 2;
                //    circle.Fill = b;
                //    circle.SetValue(Canvas.LeftProperty, (double)(p.X * 600 - size));
                //    circle.SetValue(Canvas.TopProperty, (double)(p.Y * 600 - size));

                //    canvas1.Children.Add(circle);

                //    //g.DrawEllipse(pen, rect);
                //    //g.FillEllipse(b, rect);
                 */
                //}
            //}




            //g.Dispose();
        }

        private void DrawTrace(IEnumerable<LfS.GestureDatabase.Touch> points, int size, System.Windows.Media.Brush baseFillBrush, System.Windows.Media.Brush StrokeBrush)
        {
            var n = points.Count();

            var opacityStep = 0.8 / n;

            var opacity = 1d;

            //var curFillBrush = baseFillBrush.Clone();

            foreach (var p in points)
            {
                //FillBrush.Opacity = opacity;
                var curFillBrush = baseFillBrush.Clone();
                curFillBrush.Opacity = opacity;
                opacity -= opacityStep;
                var circle = new Ellipse();
                circle.Width = size * 2;
                circle.Height = size * 2;
                circle.Fill = curFillBrush;
                //circle.SetValue(Ellipse.FillProperty, FillBrush);
                circle.Stroke = StrokeBrush;
                circle.SetValue(Canvas.LeftProperty, (double)(p.X * 600 - size));
                circle.SetValue(Canvas.TopProperty, (double)(p.Y * 600 - size));
                
                canvas1.Children.Add(circle);
            }
        }

        private Series CreateSeries(IEnumerable<LfS.GestureDatabase.Touch> points)
        {
            var series = new Series();

            foreach (var p in points)
                series.Points.AddXY(p.X, 1 - p.Y);

            series.ChartType = SeriesChartType.Point;

            return series;
        }

        //private Series featureToSeries(FeatureGenerator fg, IEnumerable<LfS.GestureDatabase.Touch> touches, int ableitung)
        //{
        //    var series = new Series();

        //    var values = fg.getFeature(touches, ableitung);
        //    //var times = fg.getTimes(touches);

        //    foreach (var entry in values)
        //        series.Points.AddXY(entry.X, entry.Y);

        //    return series;
        //}

        /*
        private Series CreateVelocitySeries(IEnumerable<LfS.GestureDatabase.Touch> points)
        {
            var series = new Series();

            int i=0;
            LfS.GestureDatabase.Touch prev = null;
            long startTime = 0;
            foreach (var p in points)
            {
                if (i++ == 0)
                {
                    series.Points.AddXY(0, 0);
                    prev = p;
                    startTime = p.Time;
                    continue;
                }
                else
                {
                    var difT = p.Time - prev.Time;
                    var difX = (double)(p.X - prev.X);
                    var difY = (double)(p.Y - prev.Y);
                    var v = Math.Sqrt(difX * difX + difY * difY);
                    //var dif = new Vector((double)(p.X - prev.X), (double)(p.Y - prev.Y));
                    //var v = dif.Length;
                    series.Points.AddXY(prev.Time + (difT / 2) - startTime, v);
                    prev = p;
                }
            }

            series.ChartType = SeriesChartType.Line;

            return series;
        }

        private Series CreateAccelerationSeries(IEnumerable<LfS.GestureDatabase.Touch> points)
        {
            var series = new Series();

            int i = 0;
            LfS.GestureDatabase.Touch prev = null;
            long startTime = 0;
            double prevV = 0;
            double prevTimeV = 0;

            foreach (var p in points)
            {
                if (i++ == 0)
                {
                    series.Points.AddXY(0, 0);
                    prev = p;
                    startTime = p.Time;
                    continue;
                }
                else
                {
                    var difT = p.Time - prev.Time;
                    var dif = new Vector((double)(p.X - prev.X), (double)(p.Y - prev.Y));
                    var v = dif.Length;
                    var timeV = prev.Time + (difT / 2) - startTime;
                    

                    if (i > 2)
                    {
                        double a = v - prevV;
                        var timeA = prevTimeV + (((p.Time-startTime) - prevTimeV) / 2);
                        series.Points.AddXY(timeA, a);
                    }

                    prev = p;
                    prevV = v;
                    prevTimeV = timeV;
                }
            }

            series.ChartType = SeriesChartType.Spline;
            
            return series;
        }

        private Series CreateCurvatureSeries(IEnumerable<LfS.GestureDatabase.Touch> points)
        {
            var series = new Series();

            int i = 0;
            LfS.GestureDatabase.Touch prev = null;
            long startTime = 0;
            Vector prevDif = new Vector(0, 0); ;

            foreach (var p in points)
            {
                if (i++ == 0)
                {
                    series.Points.AddXY(0, 0);
                    prev = p;
                    startTime = p.Time;
                    continue;
                }
                else
                {
                    var dif = new Vector((double)(p.X - prev.X), (double)(p.Y - prev.Y));
                    var v = dif.Length;

                    if (i > 2)
                    {
                        var angle = Vector.AngleBetween(prevDif, dif);
                        var curv = angle / 360; // /v;
                        series.Points.AddXY(prev.Time - startTime, curv);
                    }

                    prevDif = dif;
                    prev = p;
                }
            }

            series.ChartType = SeriesChartType.Spline;

            return series;
        }

        private Series CreateDistanceToStartSeries(IEnumerable<LfS.GestureDatabase.Touch> points)
        {
            var series = new Series();

            int i = 0;
            LfS.GestureDatabase.Touch start = null;
            long startTime = 0;
            foreach (var p in points)
            {
                if (i++ == 0)
                {
                    series.Points.AddXY(0, 0);
                    start = p;
                    startTime = p.Time;
                    continue;
                }
                else
                {
                    var difX = (double)(p.X - start.X);
                    var difY = (double)(p.Y - start.Y);
                    var dis = Math.Sqrt(difX * difX + difY * difY);

                    series.Points.AddXY(p.Time - startTime, dis);
                }
            }

            series.ChartType = SeriesChartType.Line;

            return series;
        }
        */
        public void normalizeTime(object sender, RoutedEventArgs e)
        {
            var normTime = chartBox.Series.Max(s => s.Points.Last().XValue);

            LinkedList<Series> sc = new LinkedList<Series>();
            foreach (var series in chartBox.Series)
            {
                var newSeries = new Series();
                newSeries.ChartType = series.ChartType;
                var seriesTime = series.Points.Last().XValue;
                foreach(var p in series.Points)
                {
                    var newX = p.XValue / seriesTime * normTime;
                    newSeries.Points.AddXY(newX, p.YValues[0]);
                }
                sc.AddLast(newSeries);
            }
       
            chartBox.Series.Clear();
            foreach( var s in sc)
                chartBox.Series.Add(s);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            chartBox.Series.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var selItem = treeView1.SelectedItem;
            var selFeature = cbFeature.SelectedValue as string;

            if (selItem is TraceViewModel)
            {
                var series = new Series();
                var features = (selItem as TraceViewModel).Features;
                features.fillSeries(series, selFeature, cbSmooth.IsChecked.Value);
                //addFeature(series, features);
                addSeries(series);
            }
            else if (selItem is GestureViewModel)
            {
                var gestureEntry = selItem as GestureViewModel;
                gestureEntry.LoadChildren();

                foreach (var trace in gestureEntry.Children.Cast<TraceViewModel>())
                {
                    var series = new Series();
                    var features = trace.Features;
                    features.fillSeries(series, selFeature, cbSmooth.IsChecked.Value);
                    addSeries(series);
                }
            }

            //var ableitung = int.Parse(tbAbleitung.Text);

            ////single trace?
            //if (selItem is TraceViewModel)
            //{
            //    var traceID = ((TraceViewModel)selItem).ID;
            //    Series series = null;
            //    using (var ctx = new dbEntities())
            //    {
            //        var points = from t in ctx.Touches where t.Trace.Id == traceID orderby t.Time select t;

            //        var fg = cbFeature.SelectedValue as FeatureGenerator;
            //        series = featureToSeries(fg, points, ableitung);
            //        /*
            //        switch (cbFeature.SelectedIndex)
            //        {
            //            case 0: series = CreateSeries(points); break;
            //            case 1: series = CreateVelocitySeries(points); break;
            //            case 2: series = CreateAccelerationSeries(points); break;
            //            case 3: series = CreateCurvatureSeries(points); break;
            //            case 4: series = CreateDistanceToStartSeries(points); break;
            //            case 5: series = featureToSeries(new StartDistanceFeatureGenerator(), points); break;
            //        }
            //        */
            //    }

            //    switch(cbGraphType.SelectedIndex)
            //    {
            //        case 0: series.ChartType = SeriesChartType.Point; break;
            //        case 1: series.ChartType = SeriesChartType.Line; break;
            //        case 2: series.ChartType = SeriesChartType.Spline; break;
            //    }

            //    chartBox.Series.Add(series);
            //}
            ////all traces of a gesture?
            //else if(selItem is GestureViewModel)
            //{
            //    var gestureID = ((GestureViewModel)selItem).ID;
                
            //    using (var ctx = new dbEntities())
            //    {
            //        var traces = from t in ctx.Traces where t.Gesture.Id == gestureID select t.Id;
            //        var fg = cbFeature.SelectedValue as FeatureGenerator;
            //        foreach (var traceID in traces)
            //        {
            //            var points = from t in ctx.Touches where t.Trace.Id == traceID orderby t.Time select t;

            //            Series series = null;
            //            series = featureToSeries(fg, points, ableitung);

            //            /*
            //            switch (cbFeature.SelectedIndex)
            //            {
            //                case 0: series = CreateSeries(points); break;
            //                case 1: series = CreateVelocitySeries(points); break;
            //                case 2: series = CreateAccelerationSeries(points); break;
            //                case 3: series = CreateCurvatureSeries(points); break;
            //                case 4: series = CreateDistanceToStartSeries(points); break;
            //                case 5: series = featureToSeries(new StartDistanceFeatureGenerator(), points); break;
            //            }
            //            */
            //            switch (cbGraphType.SelectedIndex)
            //            {
            //                case 0: series.ChartType = SeriesChartType.Point; break;
            //                case 1: series.ChartType = SeriesChartType.Line; break;
            //                case 2: series.ChartType = SeriesChartType.Spline; break;
            //            }

            //            chartBox.Series.Add(series);
            //        }
            //    }
            //}
        }

        public void addSeries(Series series)
        {
            switch (cbGraphType.SelectedIndex)
            {
                case 0: series.ChartType = SeriesChartType.Point; break;
                case 1: series.ChartType = SeriesChartType.Line; break;
                case 2: series.ChartType = SeriesChartType.Spline; break;
            }

            chartBox.Series.Add(series);
        }

        private void CreateSaveBitmap(Canvas canvas, string filename)
        {
            //Size size = new Size(window.Width, window.Height);
            //canvas.Measure(size);
            ////canvas.Arrange(new Rect(size));

            //var rtb = new RenderTargetBitmap(
            //    (int)window.Width, //width
            //    (int)window.Height, //height
            //    dpi, //dpi x
            //    dpi, //dpi y
            //    PixelFormats.Pbgra32 // pixelformat
            //    );
            //rtb.Render(canvas);

            //SaveRTBAsPNG(rtb, filename); 


            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
             (int)canvas.Width, (int)canvas.Height,
             96d, 96d, PixelFormats.Pbgra32);
            // needed otherwise the image output is black
            canvas.Measure(new System.Windows.Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new Rect(new System.Windows.Size((int)canvas.Width, (int)canvas.Height)));

            renderBitmap.Render(canvas);

            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }

        private void btnSaveCanvas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                CreateSaveBitmap(canvas1, saveFileDialog.FileName);
            }
        }

        /*
        private delegate void AddFeature(Series s, TraceFeatures tf);


        private void addXFeature(Series s, TraceFeatures tf)
        {
            for (int i = 0; i < tf.X.Length; i++)
                s.Points.AddXY(tf.Time[i],tf.X[i]);
        }

        private void addYFeature(Series s, TraceFeatures tf)
        {
            for (int i = 0; i < tf.Y.Length; i++)
                s.Points.AddXY(tf.Time[i], tf.Y[i]);
        }

        private void addStartDistanceFeature(Series s, TraceFeatures tf)
        {
            for (int i = 0; i < tf.StartDistance.Length; i++)
                s.Points.AddXY(tf.Time[i], tf.StartDistance[i]);
        }

        private void addDirectionFeature(Series s, TraceFeatures tf)
        {
            for (int i = 0; i < tf.Direction.Length; i++)
                s.Points.AddXY(tf.Time[i], tf.Direction[i]);
        }

        private void addXVelocityFeature(Series s, TraceFeatures tf)
        {
            for (int i = 1; i < tf.X.Length; i++)
                s.Points.AddXY(tf.Time[i], tf.X[i] - tf.X[i - 1]);
        }
        private void addYVelocityFeature(Series s, TraceFeatures tf)
        {
            for (int i = 1; i < tf.Y.Length; i++)
                s.Points.AddXY(tf.Time[i], tf.Y[i] - tf.Y[i - 1]);
        }
        private void addStartDistanceChangeFeature(Series s, TraceFeatures tf)
        {
            for (int i = 1; i < tf.StartDistance.Length; i++)
                s.Points.AddXY(tf.Time[i], tf.StartDistance[i] - tf.StartDistance[i - 1]);
        }
        private void addDirectionChangeFeature(Series s, TraceFeatures tf)
        {
            for (int i = 1; i < tf.Direction.Length; i++)
                s.Points.AddXY(tf.Time[i], tf.Direction[i] - tf.Direction[i - 1]);
        }
        private void addXSmoothedFeature(Series s, TraceFeatures tf)
        {
            for (int i = 0; i < tf.Xsmoothed1.Length; i++)
                s.Points.AddXY(tf.Time[i+4], tf.Xsmoothed1[i]);
        }
        private void addXSmoothedChangeFeature(Series s, TraceFeatures tf)
        {
            for (int i = 1; i < tf.Xsmoothed1.Length; i++)
                s.Points.AddXY(tf.Time[i+4], tf.Xsmoothed1[i] - tf.Xsmoothed1[i - 1]);
        }
         * */
    }
}
