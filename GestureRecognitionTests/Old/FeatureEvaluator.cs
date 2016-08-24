using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using LfS.GestureDatabase;
using MathNet.Numerics.Interpolation.Algorithms;
using LfS.ModelLib;

namespace LfS.GestureRecognitionTests
{
    class FeatureEvaluator
    {
        public struct FeatureResult
        {
            public string featureName;
            public double valueRangeMin;
            public double valueRangeMax;
            public double similarity;
            public double[] similarityWithAvgTrace;
            public double[] similarityWithFraudData;
            public double similarityWithAllOrGeneralData;
        }

        public struct ResultRow
        {
            public string gesture;
            public string user;

            public FeatureResult[] featureResults;

            public double similarityPos;
            public double similarityPosWithAvgTrace;
        }

        public LinkedList<ResultRow> evaluate(Dictionary<string, ICollection<Trace>> dataSets)
        {
            var results = new LinkedList<ResultRow>();

            foreach (var entry in dataSets)
            {
                var curUser = entry.Key;
                var curUserTraces = entry.Value;
                var curUserStrokes = curUserTraces.Select(t => t.LongestStroke).ToArray();

                var otherUsersData = new Dictionary<string, ICollection<Trace>>(dataSets);
                otherUsersData.Remove(curUser);

                var otherUsers = otherUsersData.Select(ep=>ep.Value);

                var result = new ResultRow();
                result.gesture = curUserTraces.First().Gesture.Name;
                result.user = curUser;

                var featuresToCheck = new [] { 
                    new {Name="X", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureX), ValueRangeMin=0d, ValueRangeMax=1d},
                    new {Name="Y", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureY), ValueRangeMin=0d, ValueRangeMax=1d},
                    new {Name="VelocityX", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureVelocityX), ValueRangeMin=0d, ValueRangeMax=0.1d}, //stimmt range?
                    new {Name="VelocityY", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureVelocityY), ValueRangeMin=0d, ValueRangeMax=0.1d},
                    new {Name="Velocity", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureVelocity), ValueRangeMin=0d, ValueRangeMax=0.15d},
                    new {Name="Cosine direction", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureCosDirection), ValueRangeMin=-1d, ValueRangeMax=1d},
                    new {Name="Sine direction", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureSinDirection), ValueRangeMin=-1d, ValueRangeMax=1d},
                    new {Name="Distance", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureDistance), ValueRangeMin=0d, ValueRangeMax=10d}, //nach oben offen eigentlich
                    new {Name="Start Distance", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureStartDistance), ValueRangeMin=0d, ValueRangeMax=1.5d},
                    new {Name="Boundingbox diagonal length", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureBBDiagonalLength), ValueRangeMin=0d, ValueRangeMax=1.5d},
                    new {Name="Boundingbox diagonal angle", Feature=new Func<Trace.Stroke,double[]>(s=>s.FeatureBBDiagonalAngle), ValueRangeMin=0d, ValueRangeMax=Math.PI/2}
                };
                result.featureResults = new FeatureResult[featuresToCheck.Length];

                int i=0;
                foreach(var f in featuresToCheck)
                {
                    result.featureResults[i].featureName = f.Name;
                    result.featureResults[i].valueRangeMin = f.ValueRangeMin;
                    result.featureResults[i].valueRangeMax = f.ValueRangeMax;

                    var userFeatureTraces = curUserStrokes.Select( s => f.Feature(s).ToArray()).ToArray();
                    var otherUserFeatureTraces = otherUsers.Select(tc => tc.Select( t => f.Feature(t.LongestStroke).ToArray()));

                    result.featureResults[i].similarity = compareAllTraces(userFeatureTraces);
                    result.featureResults[i].similarityWithAvgTrace = compareTracesWithAverage(userFeatureTraces);
                    result.featureResults[i].similarityWithFraudData = similarityWithOtherTraces(userFeatureTraces, otherUserFeatureTraces);
                    //fres.similarityWithAllOrGeneralData = 
                    i++;
                }

                //result.test = freqYs.Average(f => f.Average(c => c.Real));
                    /*
                result.similarityX = compareAllTraces_Old(splineXs, endTimes, 10);
                result.similarityXWithAvgTrace = compareTracesWithAverage_Old(splineXs, endTimes, 10);
                result.similarityY = compareAllTraces_Old(splineYs, endTimes, 10);
                result.similarityYWithAvgTrace = compareTracesWithAverage_Old(splineYs, endTimes, 10);
                    */

                //result.similarityPos = (result.similarityX + result.similarityY) / 2;
                //result.similarityPosWithAvgTrace = (result.similarityXWithAvgTrace + result.similarityYWithAvgTrace) / 2;
                    
                results.AddLast(result);
            }

            return results;
        }


        //public double RMSError_Old(AkimaSplineInterpolation f1, double endTime1, AkimaSplineInterpolation f2, double endTime2, double timeStepSize)
        //{
        //    var minTime = Math.Min(endTime1, endTime2);

        //    double error = 0;
        //    int cnt = 0;
        //    for(double t = 0; t <= minTime; t += timeStepSize)
        //    {
        //        var e = f1.Interpolate(t) - f2.Interpolate(t);
        //        error += e * e;
        //        cnt++;
        //    }

        //    //ToDo: add penalty for different timeLength?
        //    //error oder result += maxTime - minTime squared?

        //    return Math.Sqrt(error/cnt);
        //}

        public double RMSError(ICollection<double> data1, ICollection<double> data2)
        {
            var sqrError = data1.Zip(data2, (x, y) => (x - y) * (x - y)).Sum();
            //ToDo: add penalty for different timeLength?
            //error oder result += maxTime - minTime squared?

            return Math.Sqrt(sqrError / Math.Min(data1.Count, data2.Count));
        }

        public double DTWCosts(ICollection<double> data1, ICollection<double> data2)
        {
            var d1 = new NDtw.SeriesVariable(data1.ToArray());
        }

        /// <summary>
        /// all traces need the same discreteTimeStep
        /// </summary>
        /// <param name="traces"></param>
        /// <returns></returns>
        public double[] createAverageTrace(IEnumerable<IList<double>> traces)
        {
            var maxN = traces.Max(t => t.Count);
            var avgTrace = new double[maxN];

            for(int i=0;i<maxN;++i)
            {
                double sum = 0;
                int cnt = 0;
                foreach(var trace in traces)
                {
                    if(i < trace.Count)
                    {
                        sum += trace[i];
                        ++cnt;
                    }
                }

                avgTrace[i] = sum / cnt;
            }

            return avgTrace;
        }

        //public AkimaSplineInterpolation createAverageTrace_Old(IEnumerable<AkimaSplineInterpolation> functions, IEnumerable<double> endTimes, double timeStepSize)
        //{
        //    var maxTime = endTimes.Max();

        //    var values = new LinkedList<double>();
        //    var avgValues = new LinkedList<double>();
        //    var timePoints = new LinkedList<double>();
        //    for (double t = 0; t <= maxTime; t += timeStepSize)
        //    {
        //        foreach (var f in functions)
        //            if(f != null) values.AddLast(f.Interpolate(t));

        //        avgValues.AddLast(values.Average());
        //        timePoints.AddLast(t);

        //        values.Clear();
        //    }

        //    return new AkimaSplineInterpolation(timePoints.ToList(), avgValues.ToList());
        //}

        //public double compareTracesWithAverage_Old(IEnumerable<AkimaSplineInterpolation> functions, IEnumerable<double> endTimes, double timeStepSize)
        //{
        //    var avgFunc = createAverageTrace_Old(functions, endTimes, timeStepSize);

        //    double sum = 0;
        //    //double sum2 = 0;
        //    double maxTime = endTimes.Max();
        //    var timeIter = endTimes.GetEnumerator();
        //    int cnt = 0;
        //    foreach (var func in functions)
        //    {
        //        timeIter.MoveNext();
        //        if (func == null) continue;
        //        var curError = RMSError_Old(avgFunc, maxTime, func, timeIter.Current, timeStepSize);
        //        sum += curError;
        //        //sum2 += curError * curError;
        //        cnt++;
        //    }

        //    double avg = sum / cnt;
        //    //double deviation = Math.Sqrt(sum2) / cnt;

        //    return avg;
        //}

        public double[] compareTracesWithAverage(IEnumerable<IList<double>> traces)
        {
            var avgGraph = createAverageTrace(traces);

            //double sum = 0;
            ////double sum2 = 0;
            //int cnt = 0;
            //foreach (var trace in traces)
            //{
            //    var curError = RMSError(avgGraph, trace);
            //    sum += curError;
            //    //sum2 += curError * curError;
            //    cnt++;
            //}

            //double avg = sum / cnt;
            ////double deviation = Math.Sqrt(sum2) / cnt;

            return traces.Select(t=>RMSError(avgGraph, t)).ToArray();
        }

        /// <summary>
        /// O(n^2)
        /// </summary>
        /// <param name="functions"></param>
        /// <param name="endTimes"></param>
        /// <param name="timeStepSize"></param>
        /// <returns></returns>
        //public double compareAllTraces_Old(IEnumerable<AkimaSplineInterpolation> functions, IEnumerable<double> endTimes, double timeStepSize)
        //{
        //    var timeIter = endTimes.GetEnumerator();
        //    var timeIter2 = endTimes.GetEnumerator();
        //    double sum = 0;
        //    int cnt = 0;
        //    foreach (var func1 in functions)
        //    {
        //        timeIter.MoveNext();
        //        foreach (var func2 in functions)
        //        {
        //            timeIter2.MoveNext();
        //            if (func1 == null || func2 == null) continue;
        //            var curError = RMSError_Old(func1, timeIter.Current, func2, timeIter2.Current, timeStepSize);
        //            sum += curError;
        //            cnt++;
        //        }
        //    }

        //    double avg = sum / cnt;

        //    return avg;
        //}

        public double compareAllTraces(IEnumerable<IList<double>> traces)
        {
            var crossJoin = from t1 in traces from t2 in traces select RMSError(t1, t2);
            return crossJoin.Average();
        }

        public double[] similarityWithOtherTraces(IEnumerable<IList<double>> tracesUser, IEnumerable<IEnumerable<IList<double>>> tracesOtherUsers)
        {
            var avgUserTrace = createAverageTrace(tracesUser);
            var avgTraceOtherUsers = tracesOtherUsers.Select(t => createAverageTrace(t));

            return avgTraceOtherUsers.Select(t => RMSError(avgUserTrace,t)).ToArray();
        }

        //public void evaluateTraces(Dictionary<string, ICollection<ICollection<Touch>>> userTraces)
        //{

        //    foreach(var userSet in userTraces)
        //    {

        //        string curUser = userSet.Key;
        //        var traces = userSet.Value;

        //    }

        //}


        //private Series traceToSeries(ICollection<Touch> trace)
        //{
        //    var seriesX = new Series();
        //    var seriesY = new Series();

        //    long startTime = trace.First().Time;

        //    List<double> timePoints = trace.Select(t => (double)(t.Time - startTime)).ToList();
        //    List<double> xValues = trace.Select(t => (double)t.X).ToList();
        //    List<double> yValues = trace.Select(t => (double)t.Y).ToList();

        //    var splineX = new MathNet.Numerics.Interpolation.Algorithms.CubicSplineInterpolation(timePoints, xValues);
        //    var splineY = new MathNet.Numerics.Interpolation.Algorithms.CubicSplineInterpolation(timePoints, yValues);

            

        //    return null;
        //}

    }
}
