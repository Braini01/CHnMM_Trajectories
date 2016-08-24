using GestureRecognitionLib;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GestureRecognitionLib.CHnMM;
using System.IO;

namespace LfS.GestureRecognitionTests.Experiments
{
    using System;
    using GestureDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public static class VerificationResults
    {
        public class BasicResult
        {
            public double FAR { get; }
            public double FRR { get; }
            public Configuration Config { get; }

            public BasicResult(double far, double frr)
            {
                FAR = far;
                FRR = frr;
            }

            public static string getCSVHead()
            {
                return $"FAR;FRR;";
            }

            public string getCSVData()
            {
                return $"{FAR:F2}; {FRR:F2}";
            }
        }

        public static void saveResultsToFile(string file, IEnumerable<Configuration> configs, IEnumerable<BasicResult> results)
        {
            Debug.Assert(configs.Count() == results.Count());
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine(Configuration.getCSVHeaders() + ";" + BasicResult.getCSVHead());

            foreach (var result in configs.Zip(results, (c,r) => new {Config = c, Result = r }))
            {
                sw.WriteLine(result.Config.getCSVValues() + ";" + result.Result.getCSVData());
            }
            sw.Close();
        }
    }

    public static class VerificationExperiment
    {
        public static VerificationResults.BasicResult DoVerification(IVerificationSystem verifier, GestureDataSet trainingSet, GestureDataSet genuineSet, GestureDataSet forgerySet)
        {
            Stopwatch swTrain = new Stopwatch();
            //train recognition system
            verifier.clearGestures();
            swTrain.Restart();
            foreach (var e in trainingSet)
            {
                verifier.trainGesture(e.Key, e.Value);
            }
            swTrain.Stop();

            int nFalseAccepts = 0;
            int nFalseRejects = 0;

            int nGenuineAttempts = 0;
            //test genuine
            foreach (var e in genuineSet)
            {
                foreach (var trace in e.Value)
                {
                    var userVerified = verifier.verifyGesture(e.Key, trace);
                    if (!userVerified) nFalseRejects++;
                    nGenuineAttempts++;
                }
            }

            int nForgeryAttempts = 0;
            //test forgeries
            foreach (var e in forgerySet)
            {
                foreach (var trace in e.Value)
                {
                    var userVerified = verifier.verifyGesture(e.Key, trace);
                    if (userVerified) nFalseAccepts++;
                    nForgeryAttempts++;
                }
            }

            double FAR = (double)nFalseAccepts / nForgeryAttempts;
            double FRR = (double)nFalseRejects / nGenuineAttempts;

            return new VerificationResults.BasicResult(FAR, FRR);
        }
    }

    public class CHnMMVerificationExperiment
    {
        private string dataSourceName;
        private int nTraining;
        private bool session2;
        private Configuration[] configs;

        private GestureDataSet trainingSet;
        private GestureDataSet genuineSet;
        private GestureDataSet forgerySet;

        public CHnMMVerificationExperiment(string setName, int nTraining, bool session2, Configuration config)
            : this(setName, nTraining, session2, new Configuration[] { config }) { }

        public CHnMMVerificationExperiment(string setName, int nTraining, bool session2, Configuration[] configs)
        {
            this.dataSourceName = setName;
            this.nTraining = nTraining;
            this.configs = configs;
            this.session2 = session2;

            var allSets = DataSets.getVerificationDataSet(setName, nTraining, session2);

            this.trainingSet = allSets.First();
            this.genuineSet = allSets.Skip(1).First();
            this.forgerySet = allSets.Skip(2).First();
        }

        public void execute()
        {
            var results = new LinkedList<VerificationResults.BasicResult>();
            foreach (var config in configs)
            {
                var chnmmRec = new CHnMMRecognitionSystem(config);
                var res = VerificationExperiment.DoVerification(chnmmRec, trainingSet, genuineSet, forgerySet);
                results.AddLast(res);
            }

            var txtSession = session2 ? "2" : "1";
            string fileName = $"Verification_CHnMM_{dataSourceName}_{nTraining}trainingTraces_Session{txtSession}_{DateTime.Now.ToFileTime()}.csv";
            VerificationResults.saveResultsToFile("..\\..\\ExperimentResults\\" + fileName, configs, results);
        }
    }
}