using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using GestureRecognitionLib;
using LfS.GestureDatabase;

namespace LfS.GestureRecognitionTests
{
    class ProgramGestureRecognitionUserInvariant
    {
        static void Main(string[] args)
        {
            var baseConfig = new Configuration();

            baseConfig.nAreaForStrokeMap = 15;
            baseConfig.minRadiusArea = 0.03;
            baseConfig.toleranceFactorArea = 1.3;

            var paramVar1 = new DoubleParamVariation("nAreaForStrokeMap", 10, 2, 20);
            var paramVar2 = new DoubleParamVariation("minRadiusArea", 0.01, 0.02, 0.19);
            var paramVar3 = new DoubleParamVariation("toleranceFactorArea", 1.1, 0.1, 2.1);

            var configSet = baseConfig.getParameterVariations(paramVar1, paramVar2, paramVar3);


            Stream TestFileStream;
            BinaryFormatter serializer = new BinaryFormatter();
            Dictionary<string, GestureTrace[]> circleGestures, squareGestures, ownFormGestures;

            if (File.Exists("Circle_1Finger.gestures"))
            {
                TestFileStream = File.OpenRead("Circle_1Finger.gestures");
                circleGestures = (Dictionary<string, GestureTrace[]>)serializer.Deserialize(TestFileStream);
                TestFileStream.Close();
            }
            else
            {
                circleGestures = getGestureTraces("Circle_1Finger");

                TestFileStream = File.Create("Circle_1Finger.gestures");
                serializer.Serialize(TestFileStream, circleGestures);
                TestFileStream.Close();
            }

            if (File.Exists("Square_1Finger.gestures"))
            {
                TestFileStream = File.OpenRead("Square_1Finger.gestures");
                squareGestures = (Dictionary<string, GestureTrace[]>)serializer.Deserialize(TestFileStream);
                TestFileStream.Close();
            }
            else
            {
                squareGestures = getGestureTraces("Square_1Finger");

                TestFileStream = File.Create("Square_1Finger.gestures");
                serializer.Serialize(TestFileStream, squareGestures);
                TestFileStream.Close();
            }

            if (File.Exists("OwnForm_1Finger.gestures"))
            {
                TestFileStream = File.OpenRead("OwnForm_1Finger.gestures");
                ownFormGestures = (Dictionary<string, GestureTrace[]>)serializer.Deserialize(TestFileStream);
                TestFileStream.Close();
            }
            else
            {
                ownFormGestures = getGestureTraces("OwnForm_1Finger");

                TestFileStream = File.Create("OwnForm_1Finger.gestures");
                serializer.Serialize(TestFileStream, ownFormGestures);
                TestFileStream.Close();
            }
            
            var gesturesToTest = ownFormGestures;

            var nSubsets = 4;
            var results = new Dictionary<Configuration,IEnumerable<CrossValidator.RecognitionResultRow>>(1000);
            foreach (var config in configSet)
            {
                var recSystem = new RecognitionSystem(config);
                var rows = CrossValidator.validateRecognition(recSystem, gesturesToTest, nSubsets);
                results[config] = rows;
            }

            CrossValidator.saveResultsToFile(@"F:\Dropbox\LfS\Code\GestureRecognitionTests\GestureRecognition_OwnForm.csv", results);

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
