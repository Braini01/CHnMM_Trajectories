using GestureRecognitionLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LfS.GestureRecognitionTests
{
    using GestureDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public interface ISubsetCreator
    {
        void createSubsets(GestureDataSet allGestureTraces);
        IEnumerable<GestureDataSet> getTrainingSubsets();
        IEnumerable<GestureDataSet> getTestSubsets();
    }

    public interface IVerificationSubsetCreator
    {
        void createSubsets(GestureDataSet allGestureTraces);
        IEnumerable<GestureDataSet> getTrainingSubsets();
        IEnumerable<GestureDataSet> getGenuineSubsets();
        IEnumerable<GestureDataSet> getForgerySubsets();
    }

    public class SimpleSplitSubsetCreator : ISubsetCreator
    {
        private int nTraining;
        private GestureDataSet trainingSet;
        private GestureDataSet testSet;

        public SimpleSplitSubsetCreator(int nTraining)
        {
            this.nTraining = nTraining;
        }

        public void createSubsets(GestureDataSet allGestureTraces)
        {
            trainingSet = allGestureTraces.ToDictionary(e => e.Key, e => e.Value.Take(nTraining));
            testSet = allGestureTraces.ToDictionary(e => e.Key, e => e.Value.Skip(nTraining));
        }

        public IEnumerable<GestureDataSet> getTestSubsets()
        {
            yield return testSet;
        }

        public IEnumerable<GestureDataSet> getTrainingSubsets()
        {
            yield return trainingSet;
        }
    }

    public class CrossvalidationSubsetCreator : ISubsetCreator
    {
        private int nSubsets;
        private GestureDataSet[] trainingSubsets;
        private GestureDataSet[] testSubsets;

        public CrossvalidationSubsetCreator(int nSubsets)
        {
            this.nSubsets = nSubsets;
        }

        public void createSubsets(GestureDataSet allGestureTraces)
        {
            trainingSubsets = new GestureDataSet[nSubsets];
            testSubsets = new GestureDataSet[nSubsets];

            var subsetSize = allGestureTraces.Values.First().Count() / nSubsets;

            for(int curSubset=0;curSubset<nSubsets;curSubset++)
            {
                trainingSubsets[curSubset] = allGestureTraces.ToDictionary(
                    e => e.Key, 
                    e => (IEnumerable<BaseTrajectory>)e.Value.Where( (_,index)=>(index/subsetSize) != curSubset).ToArray()
                    );

                testSubsets[curSubset] = allGestureTraces.ToDictionary(
                    e => e.Key,
                    e => (IEnumerable<BaseTrajectory>)e.Value.Where((_, index) => (index / subsetSize) == curSubset).ToArray()
                    );
            }
        }

        public IEnumerable<GestureDataSet> getTestSubsets()
        {
            return testSubsets;
        }

        public IEnumerable<GestureDataSet> getTrainingSubsets()
        {
            return trainingSubsets;
        }
    }
}
