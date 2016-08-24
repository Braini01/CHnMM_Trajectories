using GestureRecognitionLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GestureRecognitionLib.CHnMM;
using GestureRecognitionLib.DTW;
using System.Diagnostics;

namespace LfS.GestureRecognitionTests.Experiments
{
    using GestureRecognitionLib.Dollar;
    using GestureDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public static class GestureRecognitionResults
    {
        public class SingleResult
        {
            public string ExecutedGesture { get; private set; }
            public string ClassifiedGesture { get; private set; }
            public int RecognitionTime { get; private set; }
            public long ExecutedTraceID { get; private set; }

            public GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail FailReason { get; private set; }

            public SingleResult(string executed, string classified, int recogTime, long traceId, GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail fail)
            {
                ExecutedGesture = executed;
                ClassifiedGesture = classified;
                RecognitionTime = recogTime;
                ExecutedTraceID = traceId;
                FailReason = fail;
            }

            public static string getCSVHead()
            {
                return "Executed;Classified;ExecutedTraceID;FailReason;RecognitionTime(ms)";
            }

            public string CSVData()
            {
                var fail = Enum.GetName(typeof(GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail), FailReason);

                var sb = new StringBuilder(ExecutedGesture);
                sb.Append(";");
                sb.Append(ClassifiedGesture);
                sb.Append(";");
                sb.Append(ExecutedTraceID);
                sb.Append(";");
                sb.Append(fail == "UNDEFINED" ? "" : fail);
                sb.Append(";");
                sb.Append(RecognitionTime);
                return sb.ToString();
            }
        }

        public class SubsetResult
        {
            public SingleResult[] Results { get; private set; }
            public int RecognitionTime { get; private set; }
            public int TrainTime { get; private set; }

            public double Precision { get; private set; }
            public double Recall { get; private set; }

            public SubsetResult(SingleResult[] results, int recogTime, int trainTime)
            {
                Results = results;
                RecognitionTime = recogTime;
                TrainTime = trainTime;
            }

            public static string getCSVHead()
            {
                return "Subset;TrainTime;SubsetRecognitionTime;" + SingleResult.getCSVHead();
            }

            public static string getShortCSVHead()
            {
                return "Subset;TrainTime;SubsetRecognitionTime;Precision;Recall";
            }

            public IEnumerable<string> getCSVData(int subsetIndex)
            {
                var sb = new StringBuilder();

                foreach (var res in Results)
                {
                    sb.Append(subsetIndex);
                    sb.Append(";");
                    sb.Append(TrainTime);
                    sb.Append(";");
                    sb.Append(RecognitionTime);
                    sb.Append(";");
                    sb.Append(res.CSVData());
                    yield return sb.ToString();
                    sb.Clear();
                }
            }
            public string getShortCSVData(int subsetIndex)
            {
                var sb = new StringBuilder();

                sb.Append(subsetIndex);
                sb.Append(";");
                sb.Append(TrainTime);
                sb.Append(";");
                sb.Append(RecognitionTime);
                sb.Append(";");
                sb.Append(Precision);
                sb.Append(";");
                sb.Append(Recall);
                return sb.ToString();
            }

        }

        public class CrossValidationResultParameterless
        {
            public SubsetResult[] Results { get; private set; }
            public int RecognitionTime { get; private set; }

            public CrossValidationResultParameterless(SubsetResult[] results, int recogTime)
            {
                Results = results;
                RecognitionTime = recogTime;
            }

            public static string getCSVHead()
            {
                return "CrossValidationRecognitionTime;" + SubsetResult.getCSVHead();
            }

            public IEnumerable<string> getCSVData()
            {
                var sb = new StringBuilder();

                int i = 0;
                foreach (var subset in Results)
                {
                    foreach (var res in subset.getCSVData(i++))
                    {
                        sb.Append(RecognitionTime);
                        sb.Append(";");
                        sb.Append(res);
                        yield return sb.ToString();
                        sb.Clear();
                    }
                }
            }
        }

        public class CrossValidationResult
        {
            public Configuration Parameter { get; private set; }
            public SubsetResult[] Results { get; private set; }
            public int RecognitionTime { get; private set; }

            public CrossValidationResult(Configuration parameter, SubsetResult[] results, int recogTime)
            {
                Results = results;
                RecognitionTime = recogTime;
                Parameter = parameter;
            }

            public static string getCSVHead()
            {
                return Configuration.getCSVHeaders() + ";CrossValidationRecognitionTime;" + SubsetResult.getCSVHead();
            }

            public IEnumerable<string> getCSVData()
            {
                var sb = new StringBuilder();

                int i = 0;
                foreach (var subset in Results)
                {
                    foreach (var res in subset.getCSVData(i++))
                    {
                        sb.Append(Parameter.getCSVValues());
                        sb.Append(";");
                        sb.Append(RecognitionTime);
                        sb.Append(";");
                        sb.Append(res);
                        yield return sb.ToString();
                        sb.Clear();
                    }
                }
            }
        }
        public static void saveResultsToFile(string file, IEnumerable<SubsetResult> results)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine(SubsetResult.getCSVHead());

            int i = 0;
            foreach (var result in results)
            {
                foreach (var row in result.getCSVData(i))
                {
                    sw.WriteLine(row);
                }
                i++;
            }
            sw.Close();
        }
        public static void saveResultsToFile(string file, CrossValidationResult[] results)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine(CrossValidationResult.getCSVHead());

            foreach (var result in results)
            {
                foreach (var row in result.getCSVData())
                {
                    sw.WriteLine(row);
                }
            }
            sw.Close();
        }
        public static void saveResultsToFile(string file, CrossValidationResultParameterless result)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine(CrossValidationResultParameterless.getCSVHead());

            foreach (var row in result.getCSVData())
            {
                sw.WriteLine(row);
            }

            sw.Close();
        }
    }

    public static class RecognitionExperiment
    {
        public static LinkedList<GestureRecognitionResults.SubsetResult> DoRecognition(IRecognitionSystem recognizer, IEnumerable<GestureDataSet> trainingSubSets, IEnumerable<GestureDataSet> testSubSets)
        {
            Stopwatch swSingle = new Stopwatch();
            Stopwatch swSubset = new Stopwatch();
            Stopwatch swTrain = new Stopwatch();

            var result = new LinkedList<GestureRecognitionResults.SubsetResult>();
            var subsets = trainingSubSets.Zip(testSubSets, (train, test) => new { TrainingSubset = train, TestSubset = test });
            foreach (var subset in subsets)
            {
                //train recognition system
                recognizer.clearGestures();
                swTrain.Restart();
                foreach (var e in subset.TrainingSubset)
                {
                    recognizer.trainGesture(e.Key, e.Value);
                }
                swTrain.Stop();

                var tmpResult = new LinkedList<GestureRecognitionResults.SingleResult>();
                //test recognition system
                swSubset.Restart();
                foreach (var e in subset.TestSubset)
                {
                    foreach (var testTrace in e.Value)
                    {
                        swSingle.Restart();
                        var classifiedGesture = recognizer.recognizeGesture(testTrace);
                        swSingle.Stop();

                        tmpResult.AddLast(new GestureRecognitionResults.SingleResult(e.Key, classifiedGesture, (int)swSingle.ElapsedMilliseconds, -1, GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail.UNDEFINED));
                    }
                }
                swSubset.Stop();

                result.AddLast(new GestureRecognitionResults.SubsetResult(tmpResult.ToArray(), (int)swSubset.ElapsedMilliseconds, (int)swTrain.ElapsedMilliseconds));
            }

            return result;
        }
    }

    public class DTWExperiment
    {
        private bool includeTime;
        private string dataSourceName;
        private bool crossValidate;
        private int nTrainingOrSubsets;
        private double pointDistance;

        private IEnumerable<GestureDataSet> trainingSubSets;
        private IEnumerable<GestureDataSet> testSubSets;

        public DTWExperiment(bool includeTime, string setName, bool crossValidate, int nTrainingOrSubsets, double pointDistance = 0)
        {
            this.includeTime = includeTime;
            this.dataSourceName = setName;
            this.nTrainingOrSubsets = nTrainingOrSubsets;
            this.crossValidate = crossValidate;
            this.pointDistance = pointDistance;

            ISubsetCreator subsetCreator = crossValidate ?
                                    new CrossvalidationSubsetCreator(nTrainingOrSubsets) :
                                    (ISubsetCreator)new SimpleSplitSubsetCreator(nTrainingOrSubsets);

            var traceProcessor = (pointDistance > 0) ? new DynamicAreaPointSampling(pointDistance) : null;

            GestureDataSet allGestures = DataSets.getTrajectoryDataSet(setName);

            if (traceProcessor != null)
                allGestures = traceProcessor.processAllGestures(allGestures);

            subsetCreator.createSubsets(allGestures);
            trainingSubSets = subsetCreator.getTrainingSubsets();
            testSubSets = subsetCreator.getTestSubsets();
        }

        public void execute()
        {
            var dtwRec = new DTWRecognitionSystem(includeTime);
            var results = RecognitionExperiment.DoRecognition(dtwRec, trainingSubSets, testSubSets);

            var txtTime = (includeTime) ? "withTime" : "withoutTime";
            var txtPD = (pointDistance > 0) ? "_pointDistance" + pointDistance : "";
            var txt = (crossValidate) ? $"{nTrainingOrSubsets}subsets" : $"{nTrainingOrSubsets}trainingtraces";
            string fileName = $"GestureRecognition_DTW{txtTime}_{dataSourceName}_{txt}{txtPD}.csv";
            GestureRecognitionResults.saveResultsToFile("..\\..\\ExperimentResults\\" + fileName, results);
        }
    }

    public class CHnMMRecognitionExperiment
    {
        private string dataSourceName;
        private bool crossValidate;
        private int nTrainingOrSubsets;
        private IEnumerable<Configuration> configs;

        private IEnumerable<GestureDataSet> trainingSubSets;
        private IEnumerable<GestureDataSet> testSubSets;

        public CHnMMRecognitionExperiment(string setName, bool crossValidate, int nTrainingOrSubsets, Configuration config)
            : this(setName, crossValidate, nTrainingOrSubsets, new Configuration[] { config }) { }

        public CHnMMRecognitionExperiment(string setName, bool crossValidate, int nTrainingOrSubsets, IEnumerable<Configuration> configs, ITraceDataProcessor traceProcessor = null)
        {
            this.dataSourceName = setName;
            this.nTrainingOrSubsets = nTrainingOrSubsets;
            this.crossValidate = crossValidate;
            this.configs = configs;

            ISubsetCreator subsetCreator = crossValidate ?
                                        new CrossvalidationSubsetCreator(nTrainingOrSubsets) :
                                        (ISubsetCreator)new SimpleSplitSubsetCreator(nTrainingOrSubsets);

            GestureDataSet allGestures = DataSets.getTrajectoryDataSet(setName);

            if (traceProcessor != null)
                allGestures = traceProcessor.processAllGestures(allGestures);

            subsetCreator.createSubsets(allGestures);
            trainingSubSets = subsetCreator.getTrainingSubsets();
            testSubSets = subsetCreator.getTestSubsets();
        }

        public void execute()
        {
            var results = new LinkedList<GestureRecognitionResults.CrossValidationResult>();
            foreach (var config in configs)
            {
                var chnmmRec = new CHnMMRecognitionSystem(config);
                var subsetResults = RecognitionExperiment.DoRecognition(chnmmRec, trainingSubSets, testSubSets);
                results.AddLast(new GestureRecognitionResults.CrossValidationResult(config, subsetResults.ToArray(), -1));
            }
            var txt = (crossValidate) ? $"{nTrainingOrSubsets}subsets" : $"{nTrainingOrSubsets}trainingtraces";
            string fileName = $"GestureRecognition_CHnMM_{dataSourceName}_{txt}.csv";
            GestureRecognitionResults.saveResultsToFile("..\\..\\ExperimentResults\\" + fileName, results.ToArray());
        }
    }

    public class DollarExperiment
    {
        private string dataSourceName;
        private bool crossValidate;
        private int nTrainingOrSubsets;

        private IEnumerable<GestureDataSet> trainingSubSets;
        private IEnumerable<GestureDataSet> testSubSets;

        public DollarExperiment(string setName, bool crossValidate, int nTrainingOrSubsets)
        {
            this.dataSourceName = setName;
            this.nTrainingOrSubsets = nTrainingOrSubsets;
            this.crossValidate = crossValidate;

            ISubsetCreator subsetCreator = crossValidate ?
                                    new CrossvalidationSubsetCreator(nTrainingOrSubsets) :
                                    (ISubsetCreator)new SimpleSplitSubsetCreator(nTrainingOrSubsets);


            GestureDataSet allGestures = DataSets.getTrajectoryDataSet(setName);
            subsetCreator.createSubsets(allGestures);
            trainingSubSets = subsetCreator.getTrainingSubsets();
            testSubSets = subsetCreator.getTestSubsets();
        }

        public void execute()
        {
            var dollarRec = new DollarRecognitionSystem();
            var results = RecognitionExperiment.DoRecognition(dollarRec, trainingSubSets, testSubSets);

            var txt = (crossValidate) ? $"{nTrainingOrSubsets}subsets" : $"{nTrainingOrSubsets}trainingtraces";
            string fileName = $"GestureRecognition_Dollar1_{dataSourceName}_{txt}.csv";
            GestureRecognitionResults.saveResultsToFile("..\\..\\ExperimentResults\\" + fileName, results);
        }
    }
}
