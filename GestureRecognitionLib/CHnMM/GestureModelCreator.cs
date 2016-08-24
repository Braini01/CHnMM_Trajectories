using System;
using System.Collections.Generic;
using System.Linq;
using LfS.ModelLib.Models;
using LfS.ModelLib.Common.Distributions;
using System.Diagnostics;
using GestureRecognitionLib.CHnMM.Estimators;

namespace GestureRecognitionLib.CHnMM
{
    public class TransitionData
    {
        private string singleSymbol = null;
        OutputProbabilityCreator probsCreator;
        DistributionEstimator distEstimator;

        public TransitionData(DistributionEstimator est, OutputProbabilityCreator probsCreator)
        {
            if (est == null || probsCreator == null) throw new ArgumentNullException();
            //this.symbol = symbol;
            this.probsCreator = probsCreator;
            this.distEstimator = est;
        }

        public TransitionData(DistributionEstimator est, string singleSymbol)
        {
            if (est == null || string.IsNullOrWhiteSpace(singleSymbol)) throw new ArgumentNullException();
            //this.symbol = symbol;
            this.singleSymbol = singleSymbol;
            this.probsCreator = null;
            this.distEstimator = est;
        }

        public void addData(int dT)
        {
            distEstimator.addData(dT);

            /*
            sum += dT;
            sum2 += dT * dT;
            if (dT < min) min = dT;
            if (dT > max) max = dT;
            n++;
            */
        }

        //create transition via collected sample data
        public Transition createTransition(State post)
        {
            //var probs = new Dictionary<string, double>(10);

            //probs.Add(symbol, 0.9);
            //probs.Add(symbol + "Tolerance", 0.1);


            //int mean = sum / n;
            //if (n > 2)
            //{
            //    int sigma = (int)(Math.Sqrt((sum2 - (sum * sum) / (double)n) / (n - 1)));
            //    return new Transition(post, new NormalDistribution(mean, sigma), probs);
            //}
            //else
            //{
            //var tMin = Math.Max(0, min - 100);
            //var tMax = max + 100;
            //return new Transition(post, new UniformDistribution(tMin, tMax), probs);
            //}

            Dictionary<string, double> outputProbs = null;
            if(singleSymbol != null)
            {
                outputProbs = new Dictionary<string, double>(5);
                outputProbs[singleSymbol] = 1.0;
            }
            else
            {
                outputProbs = probsCreator.createOutputProbabilities();
            }

            return new Transition(post, distEstimator.createDistribution(), outputProbs);
        }

        //public Transition createStartTransition(State post)
        //{
        //    var probs = new Dictionary<string, double>(10);

        //    probs.Add(symbol, 0.9);
        //    probs.Add(symbol + "Tolerance", 0.1);

        //    return new Transition(post, new DeterministicDistribution(0), probs);
        //    //}
        //}

        /*
        public Transition createEndTransition(State post)
        {
            var probs = new Dictionary<string, double>(10);

            probs.Add(symbol, 1);

            //int mean = sum / n;
            //if (n > 2)
            //{
            //    int sigma = (int)(Math.Sqrt((sum2 - (sum * sum) / (double)n) / (n - 1)));
            //    return new Transition(post, new NormalDistribution(mean, sigma), probs);
            //}
            //else
            //{
            var tMin = Math.Max(0, min - 100);
            var tMax = max + 100;
            return new Transition(post, new UniformDistribution(tMin, tMax), probs);
            //}
        }
        */
    }

    public class TransitionCreator
    {
        //config BasicAreaSymbols
        private double hitProbability;
        private string distEstName = "";

        public TransitionCreator(double hitProb, string distEstimator)
        {
            hitProbability = hitProb;
            distEstName = distEstimator;
        }

        public DistributionEstimator createDistributionEstimator()
        {
            switch (distEstName)
            {
                case nameof(NaiveUniformEstimator):
                    return new NaiveUniformEstimator();

                case nameof(AdaptiveUniformEstimator):
                    return new AdaptiveUniformEstimator();

                case nameof(MinVarianceUniformEstimator):
                    return new MinVarianceUniformEstimator();

                case nameof(NormalEstimator):
                    return new NormalEstimator();

                default: throw new ArgumentException("Unknown Distribution Estimator");
            }
        }

        public OutputProbabilityCreator createOutputProbsCreator(string area)
        {
            return new BasicAreaSymbols(area, hitProbability);
        }

        public TransitionData createTransitionDataToArea(string area)
        {
            return new TransitionData(createDistributionEstimator(), createOutputProbsCreator(area));
        }

        /// <summary>
        /// version for FixedAreaNumberModelCreator providing the deterministic distribution
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public TransitionData createTransitionDataToArea(int areaIndex, string area)
        {
            if (areaIndex == 0)
                return new TransitionData(new DeterministicDistCreator(), createOutputProbsCreator(area));
            else
                return createTransitionDataToArea(area);
        }

        public TransitionData createTransitionDataToEnd()
        {
            //ToDo: always start interval at zero?
            return new TransitionData(createDistributionEstimator(), "GestureEnd");
        }
    }

    public abstract class GestureModelCreator
    {
        public GestureModelCreator() { }

        public abstract StrokeMap createStrokeMap(IEnumerable<BaseTrajectory> traces);
        public abstract HiddenModel createModel(IEnumerable<BaseTrajectory> traces, StrokeMap strokeMap);
    }

    class FixedAreaNumberModelCreator: GestureModelCreator
    {
        private TransitionCreator transitionSetup;

        public FixedAreaNumberModelCreator(TransitionCreator setup)
        {
            transitionSetup = setup;
        }

        public override StrokeMap createStrokeMap(IEnumerable<BaseTrajectory> traces)
        {
            return new FixedAreaNumberStrokeMap(traces.ToArray());
        }
        public override HiddenModel createModel(IEnumerable<BaseTrajectory> traces, StrokeMap strokeMap)
        {
            var strokes = traces.ToArray();
            var symbolTraces = strokes.Select(s => strokeMap.getSymbolTrace(s)).ToArray();

            var transitionDatas = strokeMap.Areas.Select((a, i) => transitionSetup.createTransitionDataToArea(i, "S" + strokeMap.ID + "_A" + (i + 1))).ToArray();

            foreach (var symTrace in symbolTraces)
            {
                var prevT = symTrace[0].Time;
                for (int i = 1; i < symTrace.Length;i++ )
                {
                    var dT = (int)(symTrace[i].Time - prevT);
                    prevT = symTrace[i].Time;
                    transitionDatas[i-1].addData(dT);
                }
            }

            var startState = new StartState();
            State curState = startState;
            State nextState;

            foreach(var transData in transitionDatas)
            {
                nextState = new State();
                var transition = transData.createTransition(nextState);
                curState.addTransition(transition);
                curState = nextState;
            }

            var model = new HiddenModel(startState, curState);

            return model;
        }
    }

    class DynamicAreaNumberModelCreator: GestureModelCreator
    {
        private TransitionCreator transitionSetup;

        public DynamicAreaNumberModelCreator(TransitionCreator setup)
        {
            transitionSetup = setup;
        }

        public override StrokeMap createStrokeMap(IEnumerable<BaseTrajectory> traces)
        {
            return new DynamicAreaNumberStrokeMap(traces.ToArray());
        }

        public override HiddenModel createModel(IEnumerable<BaseTrajectory> traces, StrokeMap strokeMap)
        {
            var strokes = traces.ToArray();
            var symbolTraces = strokes.Select(s => strokeMap.getSymbolTrace(s)).ToArray();

            var transitionDatas = strokeMap.Areas.Select((a, i) =>  transitionSetup.createTransitionDataToArea("S" + strokeMap.ID + "_A" + (i + 1)) ).ToArray();

            ////number of areas -> transitiondata to endState
            //var endTransitions = new Dictionary<int,TransitionData>(15);

            var endTransitionData = transitionSetup.createTransitionDataToEnd();
            

            foreach (var symTrace in symbolTraces)
            {
                var prevT = symTrace[0].Time; //GestureStart is dummy symbol marking the beginning
                for (int i = 1; i < symTrace.Length; i++) //go through all symbols of a trace (except GestureEnd)
                {
                    var dT = (int)(symTrace[i].Time - prevT);
                    prevT = symTrace[i].Time;

                    if (i == symTrace.Length - 1)
                    {
                        endTransitionData.addData(dT);
                        //TransitionData endTransition;
                        //if(endTransitions.TryGetValue(i, out endTransition))
                        //{
                        //    endTransition.addData(dT);
                        //}
                        //else
                        //{
                        //    Debug.Assert(symTrace[i].Symbol == "GestureEnd");
                        //    endTransition = transitionSetup.createTransitionDataToEnd(); //use endtransition
                        //    endTransition.addData(dT);
                        //    endTransitions[i] = endTransition;
                        //}
                    }
                    else
                    {
                        transitionDatas[i-1].addData(dT);
                    }
                }
            }

            //var endTransPositions = endTransitions.Select(e => e.Key).OrderBy(x => x).ToArray();
            //Debug.Assert(endTransPositions.Last() - endTransPositions.First() == endTransitions.Count-1);

            var startState = new StartState();
            var areaStates = new State[strokeMap.Areas.Length];
            var endState = new State();
            State curState = startState;
            State nextState;

            int curAreaState = 0;
            foreach (var transData in transitionDatas)
            {
                nextState = new State();
                areaStates[curAreaState++] = nextState;
                var transition = transData.createTransition(nextState);
                curState.addTransition(transition);
                curState = nextState;
            }

            curState.addTransition(endTransitionData.createTransition(endState));

            //add end transitions
            //foreach(var end in endTransitions)
            //{
            //    var prevAreaState = areaStates[end.Key - 2];
            //    prevAreaState.addTransition(end.Value.createTransition(endState));
            //}

            var model = new HiddenModel(startState, endState);

            return model;
        }
    }
}
