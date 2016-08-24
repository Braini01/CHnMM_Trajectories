using GestureRecognitionLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using GestureRecognitionLib.Dollar;
using GestureRecognitionLib.CHnMM;
using GestureRecognitionLib.DTW;


namespace LfS.GestureRecognitionTests
{

    public static class VerificationResults
    {
        public class BasicResult
        {
            public double FAR { get; }
            public double FRR { get; }

            public BasicResult(double far, double frr)
            {
                FAR = far;
                FRR = frr;
            }
        }

    
        //public class SingleResult
        //{
        //    public string ExecutedGesture { get; private set; }
        //    public string ClassifiedGesture { get; private set; }
        //    public int RecognitionTime { get; private set; }
        //    public long ExecutedTraceID { get; private set; }

        //    public SingleResult(string executed, string classified, int recogTime, long traceId, GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail fail)
        //    {
        //        ExecutedGesture = executed;
        //        ClassifiedGesture = classified;
        //        RecognitionTime = recogTime;
        //        ExecutedTraceID = traceId;
        //    }

        //    public static string getCSVHead()
        //    {
        //        return "Executed;Classified;ExecutedTraceID;FailReason;RecognitionTime(ms)";
        //    }

        //    public string CSVData()
        //    {
        //        var fail = Enum.GetName(typeof(GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail), FailReason);

        //        var sb = new StringBuilder(ExecutedGesture);
        //        sb.Append(";");
        //        sb.Append(ClassifiedGesture);
        //        sb.Append(";");
        //        sb.Append(ExecutedTraceID);
        //        sb.Append(";");
        //        sb.Append(fail == "UNDEFINED" ? "" : fail);
        //        sb.Append(";");
        //        sb.Append(RecognitionTime);
        //        return sb.ToString();
        //    }
        //}
    }

        //class Experiment
        //{
        //    private const string outputPath = "..\\..\\ExperimentResults\\";

        //    private Configuration[] parameterSets;
        //    private string gestureSetName;
        //    private int nSubsets;
        //    private int nCSVResultLines;

        //    private SetOfGestureTraces gesturesToTest;

        //    public Experiment(IEnumerable<Configuration> paramSets, string setName, int nSubSets)
        //    {
        //        parameterSets = paramSets.ToArray();
        //        gestureSetName = setName;
        //        nSubsets = nSubSets;
        //        gesturesToTest = new SetOfGestureTraces( DataSets.getGestureSet(gestureSetName) );

        //        var nParamSets = parameterSets.Length;
        //        var nExamplesPerGesture = gesturesToTest.ExampleCountPerGesture;
        //        var subsetSize = nExamplesPerGesture / nSubsets;
        //        nCSVResultLines = nParamSets * subsetSize * nSubsets * gesturesToTest.GestureCount;

        //        if (nCSVResultLines >= 1048576) throw new ArgumentException("This experiment creates too many lines for an excel sheet");
        //    }

        //    public void execute()
        //    {
        //        Stopwatch swSubset = new Stopwatch();
        //        Stopwatch swCrossval = new Stopwatch();
        //        Stopwatch swExperiment = new Stopwatch();

        //        var results = new GestureRecognitionResults.CrossValidationResult[parameterSets.Length];

        //        swExperiment.Start();
        //        gesturesToTest.createSubsets(nSubsets, true);

        //        int i = 0;
        //        foreach (var parameterSet in parameterSets)
        //        {
        //            var recSystem = new CHnMMRecognitionSystem(parameterSet);

        //            var subsetResults = new GestureRecognitionResults.SubsetResult[nSubsets];
        //            swCrossval.Restart();
        //            for (int curTestSetIndex = 0; curTestSetIndex < nSubsets; curTestSetIndex++)
        //            {
        //                swSubset.Restart();
        //                gesturesToTest.trainRecognitionSystem(recSystem, curTestSetIndex);
        //                var trainTime = (int)swSubset.ElapsedMilliseconds;

        //                swSubset.Restart();
        //                var singleResults = gesturesToTest.testRecognitionSystem(recSystem, curTestSetIndex);
        //                subsetResults[curTestSetIndex] = new GestureRecognitionResults.SubsetResult(singleResults.ToArray(), (int)swSubset.ElapsedMilliseconds, trainTime);
        //            }

        //            results[i++] = new GestureRecognitionResults.CrossValidationResult(parameterSet, subsetResults, (int)swCrossval.ElapsedMilliseconds);
        //        }

        //        var neededTime = swExperiment.ElapsedMilliseconds;

        //        string fileName = "GestureRecognition_" + gestureSetName + "_nSubsets=" + nSubsets + "_neededTime=" + neededTime + ".csv";
        //        GestureRecognitionResults.saveResultsToFile(outputPath + fileName, results);
        //    }
        //}



    public class RecognitionExperiment
    {
        private IRecognitionSystem recognizer;
        private ISubsetCreator subsetCreator;
        private ITraceDataProcessor traceProcessor;

        private IEnumerable<GestureDataSet> trainingSubSets;
        private IEnumerable<GestureDataSet> testSubSets;

        private string gestureSetName;

        public RecognitionExperiment(IRecognitionSystem recognizer, ISubsetCreator subsetCreator, ITraceDataProcessor traceProcessor, string setName)
        {
            gestureSetName = setName;
            this.recognizer = recognizer;
            this.subsetCreator = subsetCreator;
            this.traceProcessor = traceProcessor;

            GestureDataSet allGestures = DataSets.getTrajectoryDataSet(setName);

            if (traceProcessor != null)
                allGestures = traceProcessor.processAllGestures(allGestures);

            subsetCreator.createSubsets(allGestures);
            trainingSubSets = subsetCreator.getTrainingSubsets();
            testSubSets = subsetCreator.getTestSubsets();
        }

        public LinkedList<GestureRecognitionResults.SubsetResult> execute()
        {
            Stopwatch swSingle = new Stopwatch();
            Stopwatch swSubset = new Stopwatch();
            Stopwatch swTrain = new Stopwatch();

            var result = new LinkedList<GestureRecognitionResults.SubsetResult>();
            var subsets = trainingSubSets.Zip(testSubSets, (train, test) => new { TrainingSubset = train, TestSubset = test });
            foreach(var subset in subsets)
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


    public class VerificationExperiment
    {
        private IVerificationSystem verifier;
        //private IVerificationSubsetCreator subsetCreator;
        //private ITraceDataProcessor traceProcessor;

        private GestureDataSet trainingSet;
        private GestureDataSet genuineSet;
        private GestureDataSet forgerySet;

        private string gestureSetName;

        private VerificationResults.BasicResult DoVerification(GestureDataSet trainingSet, GestureDataSet genuineSet, GestureDataSet forgerySet)
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

        public VerificationExperiment(IVerificationSystem verifier/*, IVerificationSubsetCreator subsetCreator, ITraceDataProcessor traceProcessor*/, string setName)
        {
            gestureSetName = setName;
            this.verifier = verifier;
            //this.subsetCreator = subsetCreator;
            //this.traceProcessor = traceProcessor;

            var allSets = DataSets.getVerificationDataSet(gestureSetName);

            trainingSet = allSets.First();
            genuineSet = allSets.Skip(1).First();
            forgerySet = allSets.Skip(2).First();
        }

        public VerificationResults.BasicResult execute()
        {
            Stopwatch swSingle = new Stopwatch();
            Stopwatch swSubset = new Stopwatch();
            Stopwatch swTrain = new Stopwatch();

            return DoVerification(trainingSet, genuineSet, forgerySet);
        }
    }

    public class DTWExperiment
    {
        private bool includeTime;
        private string dataSourceName;
        private bool crossValidate;
        private int nTrainingOrSubsets;
        private double pointDistance;

        public DTWExperiment(bool includeTime, string setName, bool crossValidate, int nTrainingOrSubsets, double pointDistance = 0)
        {
            this.includeTime = includeTime;
            this.dataSourceName = setName;
            this.nTrainingOrSubsets = nTrainingOrSubsets;
            this.crossValidate = crossValidate;
            this.pointDistance = pointDistance;
        }

        public void execute()
        {
            var dtwRec = new DTWRecognitionSystem(includeTime);
            ISubsetCreator subsetCreator = crossValidate ?
                                    new CrossvalidationSubsetCreator(nTrainingOrSubsets) :
                                    (ISubsetCreator)new SimpleSplitSubsetCreator(nTrainingOrSubsets);

            var tracePreprocessor = (pointDistance > 0) ? new DynamicAreaPointSampling(pointDistance) : null;
            var recognitionExperiment = new RecognitionExperiment(dtwRec, subsetCreator, tracePreprocessor, dataSourceName);

            var results = recognitionExperiment.execute();

            var txtTime = (includeTime) ? "withTime" : "withoutTime";
            string fileName = $"GestureRecognition_DTW{txtTime}_{dataSourceName}_5trains.csv";
            GestureRecognitionResults.saveResultsToFile("..\\..\\ExperimentResults\\" + fileName, results);
        }
    }

    public class CHnMMRecognitionExperiment
    {
        private string dataSourceName;
        private bool crossValidate;
        private int nTrainingOrSubsets;
        private IEnumerable<Configuration> configs;

        public CHnMMRecognitionExperiment(string setName, bool crossValidate, int nTrainingOrSubsets, Configuration config) 
            : this(setName, crossValidate, nTrainingOrSubsets, new Configuration[] { config }) { }

        public CHnMMRecognitionExperiment(string setName, bool crossValidate, int nTrainingOrSubsets, IEnumerable<Configuration> configs)
        {
            this.dataSourceName = setName;
            this.nTrainingOrSubsets = nTrainingOrSubsets;
            this.crossValidate = crossValidate;
            this.configs = configs;
        }

        public void execute()
        {
            var results = new LinkedList<GestureRecognitionResults.CrossValidationResult>();
            foreach (var config in configs)
            {
                var chnmmRec = new CHnMMRecognitionSystem(config);
                ISubsetCreator subsetCreator = crossValidate ?
                                        new CrossvalidationSubsetCreator(nTrainingOrSubsets) :
                                        (ISubsetCreator)new SimpleSplitSubsetCreator(nTrainingOrSubsets);

                var recognitionExperiment = new RecognitionExperiment(chnmmRec, subsetCreator, null, dataSourceName);

                results.AddLast(new GestureRecognitionResults.CrossValidationResult(config, recognitionExperiment.execute().ToArray(), -1));
            }
            var txt = (crossValidate) ? $"{nTrainingOrSubsets}subsets" : $"{nTrainingOrSubsets}trainingtraces";
            string fileName = $"GestureRecognition_CHnMM_{dataSourceName}_{txt}.csv";
            GestureRecognitionResults.saveResultsToFile("..\\..\\ExperimentResults\\" + fileName, results.ToArray());
        }
    }


    //public class DTWExperiment_old
    //{
    //    private const string outputPath = "..\\..\\ExperimentResults\\";

    //    private string gestureSetName;
    //    private int nSubsets;
    //    private int nCSVResultLines;

    //    private SetOfGestureTraces gesturesToTest;

    //    public DTWExperiment_old(string setName, int nSubSets)
    //    {
    //        gestureSetName = setName;
    //        nSubsets = nSubSets;
    //        gesturesToTest = new SetOfGestureTraces(DataSets.getGestureSet(gestureSetName));

    //        var nExamplesPerGesture = gesturesToTest.ExampleCountPerGesture;
    //        var subsetSize = nExamplesPerGesture / nSubsets;
    //        nCSVResultLines = subsetSize * nSubsets * gesturesToTest.GestureCount;

    //        if (nCSVResultLines >= 1048576) throw new ArgumentException("This experiment creates too many lines for an excel sheet");
    //    }

    //    public void execute()
    //    {
    //        Stopwatch swSubset = new Stopwatch();
    //        Stopwatch swCrossval = new Stopwatch();

    //        gesturesToTest.createSubsets(nSubsets, false);
    //        var recSystem = new DTWRecognitionSystem(true);

    //        var subsetResults = new GestureRecognitionResults.SubsetResult[nSubsets];
    //        swCrossval.Restart();
    //        for (int curTestSetIndex = 0; curTestSetIndex < nSubsets; curTestSetIndex++)
    //        {
    //            recSystem.clearGestures();
    //            swSubset.Restart();
    //            gesturesToTest.trainRecognitionSystem(recSystem, curTestSetIndex);
    //            var trainTime = (int)swSubset.ElapsedMilliseconds;

    //            swSubset.Restart();
    //            var singleResults = gesturesToTest.testRecognitionSystem(recSystem, curTestSetIndex);
    //            subsetResults[curTestSetIndex] = new GestureRecognitionResults.SubsetResult(singleResults.ToArray(), (int)swSubset.ElapsedMilliseconds, trainTime);
    //        }

    //        int neededTime = (int)swCrossval.ElapsedMilliseconds;
    //        var result = new GestureRecognitionResults.CrossValidationResultParameterless(subsetResults, neededTime);

    //        string fileName = "DTWGestureRecognition_" + gestureSetName + "_nSubsets=" + nSubsets + "_neededTime=" + neededTime + ".csv";
    //        GestureRecognitionResults.saveResultsToFile(outputPath + fileName, result);
    //    }
    //}
}
