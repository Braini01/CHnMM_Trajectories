using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using GestureRecognitionLib;
using LfS.GestureDatabase;
using System.Diagnostics;
using GestureRecognitionLib.Estimators;

namespace LfS.GestureRecognitionTests
{
    class ProgramGestureRecognitionTim
    {
        static void Main(string[] args)
        {
            var baseConfig = new Configuration();

            baseConfig.nAreaForStrokeMap = 15;
            baseConfig.minRadiusArea = 0.03;
            baseConfig.toleranceFactorArea = 1.3;

            //var paramVar1 = new ParameterVariation("nAreaForStrokeMap", 10, 2, 20);
            var paramVar2 = new DoubleParamVariation("minRadiusArea", 0.005, 0.025, 0.18);
            var paramVar3 = new DoubleParamVariation("toleranceFactorArea", 1.05, 0.05, 1.5);
            var paramVar4 = new DoubleParamVariation("areaPointDistance", 0.05, 0.05, 0.2);
            



            var configSet = Configuration.getParameterVariations(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);

            Stream TestFileStream;
            BinaryFormatter serializer = new BinaryFormatter();
            Dictionary<string, GestureTrace[]> timGestures;

            if (File.Exists("Tim.gestures"))
            {
                TestFileStream = File.OpenRead("Tim.gestures");
                timGestures = (Dictionary<string, GestureTrace[]>)serializer.Deserialize(TestFileStream);
                TestFileStream.Close();
            }
            else
            {
                string user = "Tim";
                //string[] gestures = new string[] { "Triangle_Slow", "Triangle_Normal", "Triangle_Fast", "Circle_Slow", "Circle_Normal", "Circle_Fast", "D_Slow", "D_Normal", "D_Fast", "Circle_FastLeftSlowRight", "Circle_SlowLeftFastRight" };
                //string[] gestures = new string[] { "Triangle_Slow", "Triangle_Fast", "Circle_Slow", "Circle_Fast", "D_Slow", "D_Fast", "Circle_1Stop_Bottom", "Circle_2Stop_LeftRight" };
                string[] gestures = new string[] { "Triangle_Slow", "Triangle_Fast", "Circle_Slow", "Circle_Fast", "D_Slow", "D_Fast"};

                timGestures = gestures.Select(g => new { GestureName = g, Traces = getGestureTraces(user, g) }).ToDictionary(e => e.GestureName, e => e.Traces);

                TestFileStream = File.Create("Tim.gestures");
                serializer.Serialize(TestFileStream, timGestures);
                TestFileStream.Close();
            }

            var gesturesToTest = timGestures;

            var nSubsets = 5;
            var results = new Dictionary<Configuration,IEnumerable<CrossValidator.RecognitionResultRow>>(1000);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var config in configSet)
            {
                var recSystem = new RecognitionSystem(config);
                var rows = CrossValidator.validateRecognition(recSystem, gesturesToTest, nSubsets);
                results[config] = rows;
            }
            var neededTime = sw.ElapsedMilliseconds;
            Console.WriteLine(neededTime);
            CrossValidator.saveResultsToFile(@"D:\Dropbox\LfS\Code\GestureRecognitionTests\GestureRecognition_TimGestures_2Subsets_Test.csv", results);

            //do evaluation
            
            //Console.WriteLine(model.evaluate(squareTraces.First().Value.First()));


           

            //var crossValidator = new CrossValidator();

            //////saveSymbolTracesToDatabase();
            //var resCircle = crossValidator.validate(circleTraces, "Circle_1Finger", nSubsets);
            //var resSquare = crossValidator.validate(squareTraces, "Square_1Finger", nSubsets);
            //var resOwnForm = crossValidator.validate(ownFormTraces, "OwnForm_1Finger", nSubsets);

            //var resAll = resCircle.Concat(resSquare).Concat(resOwnForm);
            //saveResultsToFile(@"F:\Dropbox\LfS\Code\GestureRecognitionTests\crossValidation_Extrema.csv", resAll);//resAll);


            ////DBtoExcel(@"D:\Dropbox\LfS\Data\Multitouch_Userstudy\Traces.csv");
        }

        private static GestureTrace TouchesToGestureTrace(ICollection<Touch> touches)
        {
            var strokes = touches.GroupBy(t => t.FingerId, t => new TouchPoint((double)t.X, (double)t.Y, t.Time))
                                 .Select(grp => new Stroke(grp.OrderBy(t => t.Time).ToArray(), grp.Key));
            return new GestureTrace(strokes.ToArray());
        }

        public static GestureTrace[] getGestureTraces(string userName, string gestureName)
        {
            using (dbEntities ctx = new dbEntities())
            {
                var gesture = ctx.Gestures.Include("TrainingTraces.Touches").First(g => g.Name == gestureName && g.User.Username == userName);

                return gesture.TrainingTraces.Select(t => TouchesToGestureTrace(t.Touches)).ToArray();
            }
        }

        public static Dictionary<string, GestureTrace[]> getGestureTraces(string gestureName)
        {
            using (dbEntities ctx = new dbEntities())
            {
                var gestures = ctx.Gestures.Include("TrainingTraces.Touches").Where(g => g.Name == gestureName);

                return gestures.ToDictionary(g => g.User.Username + "_" + gestureName, g => g.TrainingTraces.Select(t => TouchesToGestureTrace(t.Touches)).ToArray());
            }
        }

        //private static void saveResultsToFile(string file, IEnumerable<CrossValidator.ResultRow> results)
        //{
        //    var stream = File.Open(file, FileMode.Create, FileAccess.Write);
        //    var sw = new StreamWriter(stream);

        //    sw.WriteLine("Gesture;User;Subset;TP;TN;FN;FP;FRR;FAR");
        //    foreach (var result in results)
        //    {
        //        string line = result.gesture + ";" + result.user + ";" + result.subset + ";" + result.TP + ";" + result.TN + ";" + result.FN + ";" + result.FP + ";" + result.FRR + ";" + result.FAR;
        //        sw.WriteLine(line);
        //    }

        //    sw.Close();
        //}
    }
}
