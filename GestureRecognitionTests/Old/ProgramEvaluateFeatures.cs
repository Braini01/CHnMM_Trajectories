using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using LfS.ModelLib;
using LfS.GestureDatabase;
using MathNet.Numerics.IntegralTransforms;

namespace LfS.GestureRecognitionTests
{
    public class ProgramEvaluateFeatures
    {
        static void Main(string[] args)
        {
            /*
            System.Numerics.Complex[] values = new System.Numerics.Complex[32];

            for (int i = 0; i < 8; i++)
            {
                values[i] = new System.Numerics.Complex(Math.Cos(i * 4 * Math.PI / 32), 0);
                values[i] += new System.Numerics.Complex(Math.Cos(i * 2 * Math.PI / 32), 0);
            }

            for (int i = 8; i < 16; i++)
            {
                values[i] = new System.Numerics.Complex(Math.Cos(i * 4 * Math.PI / 32), 0);
                values[i] += new System.Numerics.Complex(Math.Cos(i * 2 * Math.PI / 32), 0);
            }

            for (int i = 16; i < 24; i++)
            {
                values[i] = new System.Numerics.Complex(Math.Cos(i * 4 * Math.PI / 32), 0);
                values[i] += new System.Numerics.Complex(Math.Cos(i * 2 * Math.PI / 32), 0);
            }

            for (int i = 24; i < 32; i++)
            {
                values[i] = new System.Numerics.Complex(Math.Cos(i * 4 * Math.PI / 32), 0);
                values[i] += new System.Numerics.Complex(Math.Cos(i * 2 * Math.PI / 32), 0);
            }

            var nsamples = 256;
            var samples2 = MathNet.Numerics.Signals.SignalGenerator.EquidistantInterval(dis => new System.Numerics.Complex(Math.Sin(dis), 0), 0, Math.PI*2, nsamples);

            var fft = new MathNet.Numerics.IntegralTransforms.Algorithms.DiscreteFourierTransform();
            fft.BluesteinForward(values,FourierOptions.Default);
            fft.BluesteinForward(samples2, FourierOptions.Default);

            Console.WriteLine(values);
            */

            
            var circleTraces = readUserSymbolTraces("Circle_1Finger");
            var squareTraces = readUserSymbolTraces("Square_1Finger");
            var ownFormTraces = readUserSymbolTraces("OwnForm_1Finger");

            var evaluator = new FeatureEvaluator();

            var resCircle = evaluator.evaluate(circleTraces);
            //var resSquare = evaluator.evaluate(squareTraces);
            //var resOwnForm = evaluator.evaluate(ownFormTraces);

            //var resAll = resCircle.Concat(resSquare).Concat(resOwnForm);
            saveResultsToFile(@"F:\Dropbox\LfS\Code\GestureRecognitionTests\featureEvaluation.csv", resCircle);
            
        }


        public static Dictionary<string, ICollection<LfS.GestureDatabase.Trace>> readUserSymbolTraces(string gesture)
        {
            var userSymbolTraces = new Dictionary<string, ICollection<LfS.GestureDatabase.Trace>>(20);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (dbEntities ctx = new dbEntities())
            {
                //ctx.Configuration.AutoDetectChangesEnabled = false;
                var gestures = ctx.Gestures.Include("TrainingTraces.Touches").Where(g => g.Name == gesture);
                foreach (var g in gestures)
                {
                    userSymbolTraces[g.User.Username] = g.TrainingTraces;
                    Console.WriteLine("Needed time: " + sw.Elapsed);
                }
            }

            return userSymbolTraces;
        }

        private static void saveResultsToFile(string file, IEnumerable<FeatureEvaluator.ResultRow> results)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine("Gesture;User;Feature;ValueRangeMin;ValueRangeMax;Similarity;SimilarityWithAverageTrace;SimilarityWithFraudData");
            foreach (var result in results)
            {
                foreach (var f in result.featureResults)
                {
                    foreach (var simAvg in f.similarityWithAvgTrace)
                    {
                        string line = result.gesture + ";" + result.user + ";" + f.featureName + ";" + f.valueRangeMin + ";" + f.valueRangeMax + ";" + f.similarity + ";" + simAvg + ";";
                        sw.WriteLine(line);
                    }

                    foreach (var simAvgFraud in f.similarityWithFraudData)
                    {
                        string line = result.gesture + ";" + result.user + ";" + f.featureName + ";" + f.valueRangeMin + ";" + f.valueRangeMax + ";" + f.similarity + ";;" + simAvgFraud;
                        sw.WriteLine(line);
                    }
                }
            }

            sw.Close();
        }
    }
}
