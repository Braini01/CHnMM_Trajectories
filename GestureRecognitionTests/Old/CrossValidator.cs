using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LfS.ModelLib;
using LfS.GestureDatabase;

namespace LfS.GestureRecognitionTests
{
    public class CrossValidator
    {
        public struct ResultRow
        {
            public string gesture;
            public string user;
            public int subset;
            public int TP;
            public int FN;
            public int FP;
            public int TN;

            public double FRR
            {
                get { return (double)FN / (TP + FN); }
            }

            public double FAR
            {
                get { return (double)FP / (FP + TN); }
            }
        }

        public LinkedList<ResultRow> validate(Dictionary<string, ICollection<ICollection<Touch>>> dataSets, string gesture, int nSubsets)
        {
            var results = new LinkedList<ResultRow>();
            foreach (var entry in dataSets)
            {
                var trueUser = entry.Key;
                var trueUserData = entry.Value;

                var subsetSize = trueUserData.Count / nSubsets;
                var subsets = new LinkedList<ICollection<Touch>>[nSubsets];

                var enu = trueUserData.GetEnumerator();

                //fill subsets
                for (int i = 0; i < nSubsets; i++)
                {
                    subsets[i] = new LinkedList<ICollection<Touch>>();
                    for (int j = 0; j < subsetSize; j++)
                    {
                        enu.MoveNext();
                        subsets[i].AddLast(enu.Current);
                    }
                }

                int curSubset = 1;
                foreach (var testSet in subsets)
                {
                    //build trainSet
                    var trainSet = new LinkedList<ICollection<Touch>>();
                    foreach (var subSet in subsets)
                        if (subSet != testSet)
                            foreach (var trace in subSet)
                                trainSet.AddLast(trace);

                    //use trainSet to train the model
                    var model = new GestureModel(trainSet);

                    //true positives, false negatives, false positives and true negatives
                    int TP = 0;
                    int FN = 0;
                    int FP = 0;
                    int TN = 0;

                    //calc estimated FRR
                    foreach (var testTrace in testSet)
                        if (model.evaluate(testTrace, true) == 0) FN++;
                        else TP++;

                    //double FRR = (double)nRejections / n;

                    //calc estimated FAR
                    foreach (var entry2 in dataSets)
                    {
                        var falseUser = entry2.Key;
                        var falseUserData = entry2.Value;

                        if (trueUser == falseUser) continue;

                        foreach (var falseUserTrace in falseUserData)
                            if (model.evaluate(falseUserTrace, true) == 0) TN++;
                            else FP++;
                    }

                    //save result
                    var result = new ResultRow();
                    result.gesture = gesture;
                    result.user = trueUser;
                    result.subset = curSubset++;
                    result.FN = FN;
                    result.FP = FP;
                    result.TP = TP;
                    result.TN = TN;
                    results.AddLast(result);
                }
            }

            return results;
        }

        public LinkedList<ResultRow> validate(ModelCreator modelcreator, Dictionary<string, ICollection<ICollection<Observation>>> dataSets, string gesture, int nSubsets)
        {
            var results = new LinkedList<ResultRow>();
            foreach (var entry in dataSets)
            {
                var trueUser = entry.Key;
                var trueUserData = entry.Value;

                var subsetSize = trueUserData.Count / nSubsets;
                var subsets = new LinkedList<ICollection<Observation>>[nSubsets];

                var enu = trueUserData.GetEnumerator();

                //fill subsets
                for (int i = 0; i < nSubsets; i++)
                {
                    subsets[i] = new LinkedList<ICollection<Observation>>();
                    for (int j = 0; j < subsetSize; j++)
                    {
                        enu.MoveNext();
                        subsets[i].AddLast(enu.Current);
                    }
                }

                int curSubset = 1;
                foreach (var testSet in subsets)
                {
                    //build trainSet
                    var trainSet = new LinkedList<ICollection<Observation>>();
                    foreach (var subSet in subsets)
                        if (subSet != testSet)
                            foreach (var trace in subSet)
                                trainSet.AddLast(trace);

                    //use trainSet to train the model
                    var model = modelcreator.createModel(trainSet);

                    //true positives, false negatives, false positives and true negatives
                    int TP = 0;
                    int FN = 0;
                    int FP = 0;
                    int TN = 0;

                    //calc estimated FRR
                    foreach (var testTrace in testSet)
                        if (model.evaluate(testTrace) == 0) FN++;
                        else TP++;
                    
                    //double FRR = (double)nRejections / n;

                    //calc estimated FAR
                    foreach (var entry2 in dataSets)
                    {
                        var falseUser = entry2.Key;
                        var falseUserData = entry2.Value;

                        if (trueUser == falseUser) continue;

                        foreach (var falseUserTrace in falseUserData)
                            if (model.evaluate(falseUserTrace) == 0) TN++;
                            else FP++;
                    }

                    //save result
                    var result = new ResultRow();
                    result.gesture = gesture;
                    result.user = trueUser;
                    result.subset = curSubset++;
                    result.FN = FN;
                    result.FP = FP;
                    result.TP = TP;
                    result.TN = TN;
                    results.AddLast(result);
                }
            }

            return results;
        }
    }
}
