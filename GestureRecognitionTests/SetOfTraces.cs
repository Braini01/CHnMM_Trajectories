using System;
using System.Linq;
using System.Diagnostics;
using GestureRecognitionLib;
using System.Collections.Generic;
using System.Collections;
using GestureRecognitionLib.Dollar;
using GestureRecognitionLib.CHnMM;
using LfS.GestureRecognitionTests.Experiments;

namespace LfS.GestureRecognitionTests
{
    public class SetOfGestureTraces: IEnumerable<KeyValuePair<string,GestureTraces>>
    {
        //gesturename mapped to a set of traces of the gesture
        private Dictionary<string, GestureTraces> data;

        public int GestureCount { get { return data.Count; } }
        public int ExampleCountPerGesture { get; private set; }

        public SetOfGestureTraces(Dictionary<string, GestureTraces> data)
        {
            this.data = data;
            ExampleCountPerGesture = data.First().Value.Count;
        }

        public void createSubsets(int nSubsets, bool shuffle = false)
        {
            foreach (var traceSet in data.Values)
            {
                traceSet.createSubsets(nSubsets, shuffle);
            }
        }

        public void trainRecognitionSystem(IRecognitionSystem system, int subsetIndex)
        {
            foreach (var e in data)
            {
                system.trainGesture(e.Key, e.Value.getTrainSet(subsetIndex).Select(gt => gt.LongestStroke));
            }
        }

        public IEnumerable<GestureRecognitionResults.SingleResult> testRecognitionSystem(CHnMMRecognitionSystem system, int subsetIndex)
        {
            Stopwatch sw = new Stopwatch();

            //e => GestureName, GestureTraces
            foreach (var gesture in data)
            {
                foreach (var trace in gesture.Value.getTestSet(subsetIndex))
                {
                    TrajectoryModel.ReasonForFail failReason;
                    sw.Restart();
                    var recogGesture = system.recognizeGesture(gesture.Key, trace.LongestStroke, out failReason);
                    var recogTime = sw.ElapsedMilliseconds;
                    
                    yield return new GestureRecognitionResults.SingleResult(gesture.Key, recogGesture, (int)recogTime, trace.ID, failReason);
                }
            }
        }

        public IEnumerable<GestureRecognitionResults.SingleResult> testRecognitionSystem(IRecognitionSystem system, int subsetIndex)
        {
            Stopwatch sw = new Stopwatch();

            //e => GestureName, GestureTraces
            foreach (var gesture in data)
            {
                foreach (var trace in gesture.Value.getTestSet(subsetIndex))
                {
                    //GestureRepresentation.ReasonForFail failReason;
                    sw.Restart();
                    var recogGesture = system.recognizeGesture(trace.LongestStroke);
                    var recogTime = sw.ElapsedMilliseconds;

                    yield return new GestureRecognitionResults.SingleResult(gesture.Key, recogGesture, (int)recogTime, trace.ID, TrajectoryModel.ReasonForFail.UNDEFINED);
                }
            }
        }

        public IEnumerator<KeyValuePair<string, GestureTraces>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }


    [Serializable]
    public class GestureTraces: IEnumerable<GestureTrace>
    {
        private static int shuffleSeed = 35;
        private GestureTrace[] traces;
        private GestureTrace[] shuffledTraces;
        private ArraySegment<GestureTrace>[] subsets;
        private ArraySegment<GestureTrace>[] shuffledSubsets;
        private bool isShuffled;

        public GestureTraces(GestureTrace[] traces)
        {
            this.traces = traces;
            shuffledTraces = new GestureTrace[traces.Length];
            Array.Copy(traces, shuffledTraces, traces.Length);
            new Random(shuffleSeed).Shuffle(shuffledTraces);

            isShuffled = false;
        }

        public int Count { get { return traces.Length; } }

        public void createSubsets(int nSubsets, bool shuffle = false)
        {
            isShuffled = shuffle;
            //already created?
            if (subsets != null && subsets.Length == nSubsets) return;

            Debug.Assert(traces.Length % nSubsets == 0);
            var subsetSize = traces.Length / nSubsets;

            subsets = new ArraySegment<GestureTrace>[nSubsets];
            shuffledSubsets = new ArraySegment<GestureTrace>[nSubsets];
            for (int i = 0; i < nSubsets; i++)
            {
                subsets[i] = new ArraySegment<GestureTrace>(traces, i * subsetSize, subsetSize);
                shuffledSubsets[i] = new ArraySegment<GestureTrace>(shuffledTraces, i * subsetSize, subsetSize);
            }
        }

        public IEnumerable<GestureTrace> getTrainSet(int index)
        {
            if(isShuffled)
                return shuffledSubsets.Where((ss, ssIndex) => ssIndex != index).SelectMany(ss => ss);
            else
                return subsets.Where( (ss,ssIndex) => ssIndex != index).SelectMany(ss => ss);
        }

        public IEnumerable<GestureTrace> getTestSet(int index)
        {
            if (isShuffled)
                return shuffledSubsets[index];
            else
                return subsets[index];
        }

        //public IEnumerable<IEnumerable<GestureTrace>> getTrainingSets()
        //{
        //    for(int i=0;i< subsets.Length;i++)
        //    {
        //        yield return getTrainSet(i);
        //    }
        //}

        //public IEnumerable<IEnumerable<GestureTrace>> getTestSets()
        //{
        //    for (int i = 0; i < subsets.Length; i++)
        //    {
        //        yield return getTestSet(i);
        //    }
        //}

        public IEnumerator<GestureTrace> GetEnumerator()
        {
            if (isShuffled)
                return ((IEnumerable<GestureTrace>)shuffledTraces).GetEnumerator();
            else
                return ((IEnumerable<GestureTrace>)traces).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (isShuffled)
                return ((IEnumerable<GestureTrace>)shuffledTraces).GetEnumerator();
            else
                return ((IEnumerable<GestureTrace>)traces).GetEnumerator();
        }
    }
}
