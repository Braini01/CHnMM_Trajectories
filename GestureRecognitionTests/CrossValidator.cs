using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using LfS.ModelLib;
using LfS.GestureDatabase;
using GestureRecognitionLib;
using MoreLinq;
using GestureRecognitionLib.CHnMM;

namespace LfS.GestureRecognitionTests
{
    public static class CrossValidator
    {
        public class AuthenticationResultRow
        {
            public string user;
            public int run;
            public int TP;
            public int FN;
            public int FP;
            public int TN;
            public int wrongByTimeTrueUser;
            public int wrongByFormTrueUser;
            public int wrongByTimeFalseUser;
            public int wrongByFormFalseUser;

            public double FRR
            {
                get { return (double)FN / (TP + FN); }
            }

            public double FAR
            {
                get { return (double)FP / (FP + TN); }
            }
        }

        public class RecognitionResultRow
        {
            public string executedGesture;
            public string classifiedGesture;
            public long recognitionTime;
            public int run;
        }

        private static GestureTrace[][] createSubsets(GestureTrace[] data, int nSubsets)
        {
            Debug.Assert(data.Length % nSubsets == 0);
            var subsetSize = data.Length / nSubsets;
            var subsets = new LinkedList<GestureTrace>[nSubsets];

           

            return data.Batch(subsetSize).Select(ss => ss.ToArray()).ToArray();
        }

        public static LinkedList<AuthenticationResultRow> validateAuthentication(CHnMMRecognitionSystem system, Dictionary<string, GestureTrace[]> dataSets, int nSubsets)
        {
            //if(nTrainSets + nTestSets != nSubsets) throw new ArgumentException("Anzahl der Train- und Testsets passt nicht zur angegebenen Anzahl an Subsets");

            var results = new LinkedList<AuthenticationResultRow>();
            foreach (var entry in dataSets)
            {
                var trueUser = entry.Key;
                var trueUserData = entry.Value;

                var subsets = createSubsets(trueUserData, nSubsets);

                //Leave one out cross validation: http://en.wikipedia.org/wiki/Cross-validation_%28statistics%29#Leave-one-out_cross-validation
                for (int curTestSetIndex = 0; curTestSetIndex < nSubsets; curTestSetIndex++)
                {
                    var trainSet = subsets.Where((ss, i) => i != curTestSetIndex).SelectMany(ss => ss).ToArray();
                    var testSet = subsets[curTestSetIndex];

                    //use trainSet to train the model
                    system.trainGesture(trueUser, trainSet.Select(gt => gt.LongestStroke).ToArray());

                    //true positives, false negatives, false positives and true negatives
                    int TP = 0;
                    int FN = 0;
                    int FP = 0;
                    int TN = 0;
                    int wbfTU = 0;
                    int wbtTU = 0;
                    int wbfFU = 0;
                    int wbtFU = 0;


                    //calc estimated FRR
                    foreach (var testTrace in testSet)
                    {
                        TrajectoryModel.ReasonForFail reason;
                        if (!system.authenticateUser(trueUser, testTrace, out reason)) FN++;
                        else TP++;

                        if (reason == TrajectoryModel.ReasonForFail.MissedArea) wbfTU++;
                        if (reason == TrajectoryModel.ReasonForFail.MissedTransition) wbtTU++;
                    }

                    //calc estimated FAR
                    foreach (var falseData in dataSets)
                    {
                        var falseUser = falseData.Key;
                        var falseUserData = falseData.Value;

                        if (trueUser == falseUser) continue;

                        TrajectoryModel.ReasonForFail reason = TrajectoryModel.ReasonForFail.UNDEFINED;
                        foreach (var falseUserTrace in falseUserData)
                            if (!system.authenticateUser(trueUser, falseUserTrace, out reason)) TN++;
                            else FP++;

                        if (reason == TrajectoryModel.ReasonForFail.MissedArea) wbfFU++;
                        if (reason == TrajectoryModel.ReasonForFail.MissedTransition) wbtFU++;
                    }

                    //save result
                    var result = new AuthenticationResultRow();
                    result.user = trueUser;
                    result.run = curTestSetIndex;
                    result.FN = FN;
                    result.FP = FP;
                    result.TP = TP;
                    result.TN = TN;
                    result.wrongByFormFalseUser = wbfFU;
                    result.wrongByFormTrueUser = wbfTU;
                    result.wrongByTimeFalseUser = wbtFU;
                    result.wrongByTimeTrueUser = wbtTU;
                    results.AddLast(result);
                }
            }

            return results;
        }

        //public static GestureRecognitionResults.CrossValidationResult validateRecognition(RecognitionSystem system, SetOfGestureTraces dataSets, int nSubsets)
        //{
        //    //create all subsets
        //    dataSets.createSubsets(nSubsets);

        //    var results = Enumerable.Empty<RecognitionResultRow>();
        //    for (int curTestSetIndex = 0; curTestSetIndex < nSubsets; curTestSetIndex++)
        //    {
        //        dataSets.trainRecognitionSystem(system, curTestSetIndex);
        //        results = results.Concat(dataSets.testRecognitionSystem(system, curTestSetIndex));                
        //    }

        //    return results.ToArray();
        //}



        public static void saveResultsToFile(string file, Dictionary<Configuration, IEnumerable<AuthenticationResultRow>> results)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine("User;Subset;TP;TN;FN;FP;FRR;FAR;wbtTU;wbtFU;wbfTU;wbfFU" + Configuration.getCSVHeaders());

            foreach (var result in results)
            {
                var config = result.Key.getCSVValues();
                foreach (var row in result.Value)
                {
                    string line = row.user + ";" + row.run + ";" + row.TP + ";" + row.TN + ";" + row.FN + ";" + row.FP + ";" + row.FRR + ";" + row.FAR + ";" + row.wrongByTimeTrueUser + ";" + row.wrongByTimeFalseUser + ";" + row.wrongByFormTrueUser + ";" + row.wrongByFormFalseUser + config;
                    sw.WriteLine(line);
                }
            }
            sw.Close();
        }

        public static void saveResultsToFile(string file, Dictionary<Configuration, IEnumerable<RecognitionResultRow>> results)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine("ExecutedGesture;ClassifiedGesture;Subset;RecognitionTime(ms)" + Configuration.getCSVHeaders());

            foreach (var result in results)
            {
                var config = result.Key.getCSVValues();
                foreach (var row in result.Value)
                {
                    string line = row.executedGesture + ";" + row.classifiedGesture + ";" + row.run + ";" + row.recognitionTime + config;
                    sw.WriteLine(line);
                }
            }
            sw.Close();
        }

    //    public LinkedList<ResultRow> validate(ModelCreator modelcreator, Dictionary<string, ICollection<ICollection<Observation>>> dataSets, string gesture, int nSubsets)
    //    {
    //        var results = new LinkedList<ResultRow>();
    //        foreach (var entry in dataSets)
    //        {
    //            var trueUser = entry.Key;
    //            var trueUserData = entry.Value;

    //            var subsetSize = trueUserData.Count / nSubsets;
    //            var subsets = new LinkedList<ICollection<Observation>>[nSubsets];

    //            var enu = trueUserData.GetEnumerator();

    //            //fill subsets
    //            for (int i = 0; i < nSubsets; i++)
    //            {
    //                subsets[i] = new LinkedList<ICollection<Observation>>();
    //                for (int j = 0; j < subsetSize; j++)
    //                {
    //                    enu.MoveNext();
    //                    subsets[i].AddLast(enu.Current);
    //                }
    //            }

    //            int curSubset = 1;
    //            foreach (var testSet in subsets)
    //            {
    //                //build trainSet
    //                var trainSet = new LinkedList<ICollection<Observation>>();
    //                foreach (var subSet in subsets)
    //                    if (subSet != testSet)
    //                        foreach (var trace in subSet)
    //                            trainSet.AddLast(trace);

    //                //use trainSet to train the model
    //                var model = modelcreator.createModel(trainSet);

    //                //true positives, false negatives, false positives and true negatives
    //                int TP = 0;
    //                int FN = 0;
    //                int FP = 0;
    //                int TN = 0;

    //                //calc estimated FRR
    //                foreach (var testTrace in testSet)
    //                    if (model.evaluate(testTrace) == 0) FN++;
    //                    else TP++;
                    
    //                //double FRR = (double)nRejections / n;

    //                //calc estimated FAR
    //                foreach (var entry2 in dataSets)
    //                {
    //                    var falseUser = entry2.Key;
    //                    var falseUserData = entry2.Value;

    //                    if (trueUser == falseUser) continue;

    //                    foreach (var falseUserTrace in falseUserData)
    //                        if (model.evaluate(falseUserTrace) == 0) TN++;
    //                        else FP++;
    //                }

    //                //save result
    //                var result = new ResultRow();
    //                result.gesture = gesture;
    //                result.user = trueUser;
    //                result.subset = curSubset++;
    //                result.FN = FN;
    //                result.FP = FP;
    //                result.TP = TP;
    //                result.TN = TN;
    //                results.AddLast(result);
    //            }
    //        }

    //        return results;
    //    }
    }
}
