using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LfS.GestureDatabase;
using LfS.ModelLib.Models;
using LfS.ModelLib.Common.Distributions;
//using LfS.HiddenModels;

namespace LfS.ModelLib
{
    interface IAdvancedModelGenerator
    {
        HiddenModel createModel(IEnumerable<ICollection<Touch>> touches);
    }

    public abstract class ModelCreator
    {
        protected struct TransitionData
        {
            Dictionary<string, int> symbols;
            int min;
            int max;
            int sum;
            int sum2;
            int n;

            public void addData(String symbol, int dT)
            {
                if (symbols == null) symbols = new Dictionary<string, int>(10);
                int symN;
                if (symbols.TryGetValue(symbol, out symN))
                    symbols[symbol] = symN + 1;
                else
                    symbols[symbol] = 1;

                //times.AddLast(dT);
                sum += dT;
                sum2 += dT * dT;
                if (dT < min) min = dT;
                if (dT > max) max = dT;

                n++;
            }

            //create transition via collected sample data
            public Transition createTransition(State post)
            {
                var probs = new Dictionary<string, double>(symbols.Count * 2);
                foreach (var pair in symbols)
                    probs.Add(pair.Key, (double)pair.Value / n);

                int mean = sum / n;
                //if (n > 2)
                //{
                //    int sigma = (int)(Math.Sqrt((sum2 - (sum * sum) / (double)n) / (n - 1)));
                //    return new Transition(post, new Distributions.NormalDistribution(mean, sigma), probs);
                //}
                //else
                //{
                    var tMin = Math.Max(0, min - 100);
                    var tMax = max + 100;
                    return new Transition(post, new UniformDistribution(tMin, tMax), probs);
                //}
            }
        }

        public abstract HiddenModel createModel(IEnumerable<ICollection<Observation>> traces);
    }

    public class SimpleModelCreator : ModelCreator
    {
        /// <summary>
        /// symbol traces have to contain GestureStart and GestureEnd symbols
        /// </summary>
        /// <param name="traces"></param>
        /// <returns></returns>
        public override HiddenModel createModel(IEnumerable<ICollection<Observation>> traces)
        {
            //needed number of states?
            int n = 0;
            foreach (var trace in traces)
                if (trace.Count > n) n = trace.Count;

            TransitionData[] transData = new TransitionData[n-1];

            long prevTime;
            int i, dT;
            foreach (var trace in traces)
            {
                i = 0;
                dT = 0;
                prevTime = 0;
                foreach (var touch in trace)
                {
                    dT = (int)(touch.Time - prevTime);

                    if (i > 0) transData[i - 1].addData(touch.Symbol, dT);

                    prevTime = touch.Time;
                    i++;
                }
            }

            StartState start = new StartState(1d);
            State curState = start;
            State postState;
            foreach (var t in transData)
            {
                postState = new State();
                curState.addTransition(t.createTransition(postState));
                curState = postState;
            }

            //curState == EndState
            return new HiddenModel(new StartState[] {start}, curState);
        }
    }

    public class SimpleMultiplePathsModelCreator : ModelCreator
    {
        public override HiddenModel createModel(IEnumerable<ICollection<Observation>> traces)
        {
            //classify traces by length
            int nTraces = traces.Count();
            var classifiedTraces = traces.ToLookup(trace => trace.Count, trace => trace);

            //create simple model for each trace length
            var simpleMG = new SimpleModelCreator();
            var models = new Dictionary<int, HiddenModel>(50);
            foreach( var group in classifiedTraces)
                models[group.Key] = simpleMG.createModel(group);

            //merge all models
            var newStartStates = new LinkedList<StartState>();
            var newEndState = new State();
            foreach (var model in models)
            {
                int nTracesByLength = classifiedTraces[model.Key].Count();
                double prob = (double)nTracesByLength/nTraces;
                foreach (var start in model.Value.StartStates)
                {
                    start.InitialProbability *= prob;
                    newStartStates.AddLast(start);
                }

                //change end state
                State curState = model.Value.StartStates.First();
                Transition prevTrans = null;
                //find end state/transition
                while (curState.Transitions.Count > 0)
                {
                    prevTrans = curState.Transitions.First();
                    curState = curState.Transitions.First().PostState;
                }
                
                prevTrans.PostState = newEndState;
            }

            return new HiddenModel(newStartStates.ToArray(), newEndState);
        }
    }

    //public class GridModelCreator : ModelCreator
    //{
    //    public override Model createModel(IEnumerable<ICollection<Observation>> traces)
    //    {
    //        //needed number of states?
    //        int n = 0;
    //        foreach (var trace in traces)
    //            if (trace.Count > n) n = trace.Count;


    //        var startTrans = new Dictionary<string, TransitionData>(10);
    //        var startSymbols = new Dictionary<string,int>(10);
    //        int startSymN = 0;
 
    //        TransitionData[] transData = new TransitionData[n-2];

    //        long prevTime;
    //        int i, dT;
    //        string prevSym;
    //        foreach (var trace in traces)
    //        {
    //            i = 0;
    //            dT = 0;
    //            prevTime = 0;
    //            prevSym = "";
    //            foreach (var touch in trace)
    //            {
    //                dT = (int)(touch.Time - prevTime);
    //                if (i == 1)
    //                {
    //                    TransitionData t;
    //                    if (startTrans.TryGetValue(prevSym, out t))
    //                    {
    //                        t.addData(touch.Symbol, dT);
    //                        startTrans[prevSym] = t;
    //                    }
    //                    else
    //                    {
    //                        t = new TransitionData();
    //                        t.addData(touch.Symbol, dT);
    //                        startTrans[prevSym] = t;
    //                    }
    //                }
    //                else if (i > 0)
    //                {
    //                    transData[i-2].addData(touch.Symbol, dT);
    //                }
    //                else
    //                {
    //                    int symN;
    //                    if (startSymbols.TryGetValue(touch.Symbol, out symN))
    //                        startSymbols[touch.Symbol] = symN + 1;
    //                    else
    //                        startSymbols[touch.Symbol] = 1;

    //                    startSymN++;
    //                }
    //                prevTime = touch.Time;
    //                prevSym = touch.Symbol;
    //                i++;
    //            }
    //        }

    //        LinkedList<State> starts = new LinkedList<State>();
    //        LinkedList<double> initV = new LinkedList<double>();
    //        State postState = new State();
    //        foreach (var pair in startSymbols)
    //        {
    //            State s = new State();

    //            s.addTransition(startTrans[pair.Key].createTransition(postState));
    //            initV.AddLast((double)startSymbols[pair.Key] / startSymN);
    //            starts.AddLast(s);
    //        }

    //        State curState = postState;
    //        foreach (var t in transData)
    //        {
    //            postState = new State();
    //            curState.addTransition(t.createTransition(postState));
    //            curState = postState;
    //        }

    //        return new Model(initV.ToArray(), starts.ToArray(), curState);
    //    }
    //}


    //    /*
    //    private class State
    //    {
    //        public LinkedList<Transition> Transitions
    //        {
    //            get; private set;
    //        }
    //        public string Symbol
    //        {
    //            get; set;
    //        }

    //        public State(string symbol)
    //        {
    //            Symbol = symbol;
    //            Transitions = new LinkedList<Transition>();
    //        }

    //        public void AddTransition(Transition t)
    //        {
    //            Transitions.AddLast(t);
    //        }
    //    }

    //    private class Transition
    //    {
    //        public State postState
    //        {
    //            get; private set;
    //        }

    //        private int n = 0;
    //        private int sum = 0;
    //        private int min = int.MaxValue;
    //        private int max = int.MinValue;

    //        public Transition(State s)
    //        {
    //            postState = s;
    //        }

    //        public void addTime(int dT)
    //        {
    //            n++;
    //            sum += dT;
    //            if (dT < min) min = dT;
    //            if (dT > max) max = dT;
    //        }

    //        public int Mean
    //        {
    //            get { return sum / n; }
    //        }

    //        public int Min
    //        {
    //            get { return min; }
    //        }

    //        public int Max
    //        {
    //            get { return max; }
    //        }

    //        public int N
    //        {
    //            get { return n; }
    //        }
    //    }

    //    private class PasswordModel
    //    {
    //        private int nKeyStates;
    //        private State[] keyStates;
    //        private int nStates = 0;
    //        private int nTransitions = 0;

    //        public PasswordModel(IList<string> keys)
    //        {
    //            nKeyStates = keys.Count;
    //            nStates = nKeyStates;
    //            //create key states
    //            keyStates = new State[nKeyStates];
    //            for (int i = 0; i < nKeyStates; i++)
    //                keyStates[i] = new State(keys[i]);
    //        }

    //        private void AddSubTraceData(string[] symbols, int[] dTs, int keyState)
    //        {
    //            State curState = keyStates[keyState];
    //            State nextKeyState = keyStates[keyState + 1];

    //            string symbol = null;
    //            int dT = -1;
    //            bool transitionFound;
    //            for (int i = 0; i < symbols.Length; i++)
    //            {
    //                symbol = symbols[i];
    //                dT = dTs[i];
    //                transitionFound = false;

    //                //does transition already exist?
    //                foreach (Transition t in curState.Transitions)
    //                {
    //                    if (t.postState.Symbol.Equals(symbol))
    //                    {
    //                        //transition already exists -> add dT to transition
    //                        if (symbol.Equals(nextKeyState.Symbol))
    //                        {
    //                            t.addTime(dT);
    //                            if(i < symbols.Length - 1) throw new ArgumentException("There are still symbols left but key already reached");
    //                            transitionFound = true;
    //                            break;
    //                        }
    //                        else
    //                        {
    //                            t.addTime(dT);
    //                            curState = t.postState;
    //                            transitionFound = true;
    //                            break;
    //                        }
    //                    }
    //                }

    //                if (!transitionFound)
    //                {
    //                    //transition doesnt exist -> create one with according State
    //                    Transition newT;
    //                    //check for last (key-) symbol
    //                    if (symbol.Equals(nextKeyState.Symbol))
    //                    {
    //                        newT = new Transition(nextKeyState);
    //                        nTransitions++;
    //                        newT.addTime(dT);
    //                        curState.AddTransition(newT);
    //                        if (i < symbols.Length - 1) throw new ArgumentException("There are still symbols left but key already reached");
    //                    }
    //                    else
    //                    {
    //                        State newState = new State(symbol);
    //                        nStates++;
    //                        newT = new Transition(newState);
    //                        nTransitions++;
    //                        newT.addTime(dT);
    //                        curState.AddTransition(newT);
    //                        curState = newState;
    //                    }
    //                }
    //            }
    //        }

    //        public void AddTraces(Observation[][] traces, IList<string> keys)
    //        {
    //            LinkedList<string> subTraceSymbols = new LinkedList<string>();
    //            LinkedList<int> subTraceDTs = new LinkedList<int>();

    //            long prevTime = 0;
    //            int curKey = 0;
    //            int dT;
    //            foreach (var O in traces)
    //            {
    //                prevTime = 0;
    //                curKey = 0;
    //                foreach (var o in O)
    //                {
    //                    dT = (int)(o.Time - prevTime);
    //                    prevTime = o.Time;

    //                    if (o.Symbol.Equals(keys[curKey]))
    //                    {
    //                        if (curKey > 0)
    //                        {
    //                            subTraceSymbols.AddLast(o.Symbol);
    //                            subTraceDTs.AddLast(dT);
    //                            AddSubTraceData(subTraceSymbols.ToArray(), subTraceDTs.ToArray(), curKey - 1);
    //                        }
    //                        subTraceSymbols = new LinkedList<string>();
    //                        subTraceDTs = new LinkedList<int>();
    //                        curKey++;
    //                    }
    //                    else
    //                    {
    //                        if (curKey > 0)
    //                        {
    //                            subTraceSymbols.AddLast(o.Symbol);
    //                            subTraceDTs.AddLast(dT);
    //                        }
    //                    }

    //                    //ignore symbols after last key
    //                    if (curKey >= keys.Count) break;
    //                }
    //            }
    //        }

    //        private ProxelSet forwardStep(CHnMM model, ProxelSet proxels, int dT, String symbol)
    //        {
    //            ProxelSet newProxels = new ProxelSet();

    //            foreach (Proxel p in proxels)
    //            {
    //                var transitions = model.Transitions[p.State];
    //                if (transitions == null || transitions.Length < 1) continue;

    //                double P = CHnMM_Calculations.calcPsojourn(p, model, dT);
    //                if (P == 0) continue;

    //                #region ProxelCreation
    //                var baseAgeVector = new AgeVector(p.AgeVector); //create a copy


    //                //create childproxel for every active transition
    //                foreach (Transition t in transitions)
    //                {
    //                    double Psym = (t.B.ContainsKey(symbol)) ? t.B[symbol] : 0;
    //                    if (Psym > 0)
    //                    {
    //                        var newAge = new AgeVector(baseAgeVector);
    //                        double z = t.Dist.getHRF(dT);

    //                        if (z > 0)
    //                            newProxels.Add(new Proxel(t.PostState, p.Alpha * P * z * Psym, newAge));
    //                    }
    //                }
    //                #endregion
    //            }

    //            return newProxels;
    //        }
    //    */
    //        public CHnMM createCHnMM()
    //        {
    //            CHnMM model = new CHnMM();

    //            LinkedList<int> preStatesOfTransitions = new LinkedList<int>();
    //            LinkedList<LfS.Common.Transition> transitions = new LinkedList<LfS.Common.Transition>();
    //            List<State> visitedStates = new List<State>(nStates);
    //            Dictionary<State, int> stateIDs = new Dictionary<State, int>(nStates);

    //            State curState = keyStates[0];
    //            stateIDs.Add(curState, 0);

    //            processState(curState, transitions, visitedStates, stateIDs, preStatesOfTransitions);

    //            model.Pi = new double[1] { 1 };
    //            model.NumberOfStates = nStates;
    //            model.NumberOfTransitions = nTransitions;
    //            model.IsTReset = true;

    //            //set transitions
    //            LinkedList<LfS.Common.Transition>[] chnmmTransitions = new LinkedList<LfS.Common.Transition>[nStates];
    //            for(int i=0;i<nStates;i++)
    //                chnmmTransitions[i] = new LinkedList<LfS.Common.Transition>();

    //            int j=0;
    //            var iter = preStatesOfTransitions.GetEnumerator();
    //            iter.MoveNext();
    //            foreach(LfS.Common.Transition t in transitions)
    //            {

    //                int preState = iter.Current;
    //                iter.MoveNext();
    //                chnmmTransitions[preState].AddLast(t);
    //                j++;
    //            }

    //            for (int i = 0; i < nStates; i++)
    //                model.Transitions[i] = chnmmTransitions[i].ToArray();

    //            return model;
    //        }

    //        private void processState(State s, LinkedList<LfS.Common.Transition> transitions, List<State> visited, Dictionary<State, int> stateIDs, LinkedList<int> preStates)
    //        {
    //            visited.Add(s);

    //            //process next states
    //            foreach (Transition t in s.Transitions)
    //            {
    //                //already visited?
    //                if (visited.IndexOf(t.postState) >= 0)
    //                {
    //                    //yes -> just add transition, no further processing
    //                    var newT = new LfS.Common.Transition(stateIDs[t.postState], new UniformDistribution(t.Min, t.Max), new string[1] { t.postState.Symbol }, new double[1] { 1 });
    //                    transitions.AddLast(newT);
    //                    preStates.AddLast(stateIDs[s]);
    //                }
    //                else
    //                {
    //                    //no -> add transition and process state
    //                    var newT = new LfS.Common.Transition(visited.Count, new UniformDistribution(t.Min, t.Max), new string[1] { t.postState.Symbol }, new double[1] { 1 });
    //                    transitions.AddLast(newT);
    //                    preStates.AddLast(stateIDs[s]);
    //                    stateIDs.Add(t.postState, visited.Count);

    //                    processState(t.postState, transitions, visited, stateIDs, preStates);
    //                }                    
    //            }
    //        }
    //    }

        /*
        private class KeyObservations
        {
            public LinkedList<int> timesToKey = new LinkedList<int>();
            public LinkedList<int> timesFromKey = new LinkedList<int>();
            public LinkedList<string> symbolsFromKey = new LinkedList<string>();
        }
        */
        //private KeyObservations[] keyObservations;
        //private SubPaths[] allSubPaths;

        /// <summary>
        /// creates a CHnMM 
        /// </summary>
        /// <returns></returns>
        //public CHnMM createCHnMM_1(Observation[][] trainingData, params string[] passwordKeys)
        //{
        //    CHnMM model = new CHnMM();
        //    int nKeys = passwordKeys.Length;
        //    int nStates = nKeys * 2 - 1;
        //    int nTransitions = (nKeys - 1) * 3;


        //    //sort data to categories



        //    Transition[][] transitions = new Transition[nStates][];
        //    LinkedList<int>[][] dTs = new LinkedList<int>[nStates][];
        //    Dictionary<string,int>[][] symbols = new Dictionary<string,int>[nStates][];
        //    int[][] symbolCount = new int[nStates][];

        //    //build basic model
        //    model.IsTReset = false;
        //    model.NumberOfStates = nStates;
        //    model.NumberOfTransitions = nTransitions;
        //    model.Transitions = transitions;
        //    model.Pi = new double[1] {1};

        //    //create transitions
        //    for (int i = 0; i < nStates-1; i++)
        //    {
        //        //keyState?
        //        if (i % 2 == 0)
        //        {
        //            //keyState
        //            transitions[i] = new Transition[1];
        //            transitions[i][0] = new Transition(i+1,null,null,null,false);

        //            dTs[i] = new LinkedList<int>[1];
        //            dTs[i][0] = new LinkedList<int>();
        //            symbols[i] = new Dictionary<string, int>[1];
        //            symbols[i][0] = new Dictionary<string, int>();
        //            symbolCount[i] = new int[1];
        //        }
        //        else
        //        {
        //            //state between keyStates
        //            transitions[i] = new Transition[2];

        //            //reflexive transition
        //            transitions[i][0] = new Transition(i, null, null, null, false);

        //            //transition to next keyState
        //            transitions[i][1] = new Transition(i + 1, null, null, null, true);

        //            dTs[i] = new LinkedList<int>[2];
        //            dTs[i][0] = new LinkedList<int>();
        //            dTs[i][1] = new LinkedList<int>();
        //            symbols[i] = new Dictionary<string, int>[2];
        //            symbols[i][0] = new Dictionary<string, int>();
        //            symbols[i][1] = new Dictionary<string, int>();
        //            symbolCount[i] = new int[2];
        //        }
        //    }

        //    //train model
        //    var iter = passwordKeys.GetEnumerator();
        //    String key;
        //    int keyState = 0;
        //    long keyTime = 0;
        //    long prevTime = 0;
        //    int dT = 0;
        //    byte curPhase = 0; //0-PreKeyState, 1-PostKeyState, 2-GapState

        //    foreach(Observation[] trace in trainingData)
        //    {
        //        keyState = 0;
        //        keyTime = 0;
        //        prevTime = 0;
        //        iter.Reset();
        //        iter.MoveNext();
        //        key = (string)iter.Current;

        //        foreach (Observation o in trace)
        //        {
        //            dT = (int)(o.Time - prevTime);
        //            prevTime = o.Time;

        //            //preKeyState?
        //            if (o.Symbol.Equals(key))
        //            {
        //                if (iter.MoveNext()) key = (string)iter.Current;
        //                else key = null;
        //                if(keyState > 0) dTs[keyState - 1][1].AddLast((int)(o.Time - keyTime));
        //                if (key == null) break;
        //                curPhase = 1; //PostKeyState
        //            }
        //            else
        //            {
        //                //symbol is from old keyState or between two keyStates
        //                if (curPhase == 1)
        //                {
        //                    dTs[keyState][0].AddLast(dT);
        //                    if (symbols[keyState][0].ContainsKey(o.Symbol)) symbols[keyState][0][o.Symbol]++;
        //                    else symbols[keyState][0][o.Symbol] = 1;
        //                    symbolCount[keyState][0]++;
        //                    keyTime = o.Time;
        //                    keyState += 2;
        //                    curPhase = 2;
        //                }
        //                else
        //                {
        //                    //symbol from gapState
        //                    dTs[keyState-1][0].AddLast(dT);
        //                    if (symbols[keyState-1][0].ContainsKey(o.Symbol)) symbols[keyState-1][0][o.Symbol]++;
        //                    else symbols[keyState-1][0][o.Symbol] = 1;
        //                    symbolCount[keyState-1][0]++;
        //                }
        //            }
        //        }
        //    }

        //    //apply distributions and symbols
        //    int mean = 0;
        //    int sig = 0;
        //    iter.Reset();
        //    iter.MoveNext();
        //    for (int s = 0; s < nStates-1; s++)
        //    {
        //        if (s % 2 == 0)
        //        {
        //            mean = calcMean(dTs[s][0]);
        //            sig = calcSigma(dTs[s][0], mean);

        //            transitions[s][0].B = createB(symbols[s][0], symbolCount[s][0]);
        //            transitions[s][0].Dist = new NormalDistribution(mean, sig);
        //        }
        //        else
        //        {
        //            mean = calcMean(dTs[s][0]);
        //            //var = calcVariance(dTs[s][0], mean);

        //            transitions[s][0].B = createB(symbols[s][0], symbolCount[s][0]);
        //            transitions[s][0].Dist = new ExponentialDistribution(mean);

        //            mean = calcMean(dTs[s][1]);
        //            sig = calcSigma(dTs[s][1], mean);

        //            iter.MoveNext();
        //            transitions[s][1].B = new Dictionary<string,double>(4);
        //            transitions[s][1].B.Add((string)iter.Current, 1.0);
        //            transitions[s][1].Dist = new NormalDistribution(mean, sig);
        //        }
        //    }

        //    return model;
        //}

    //    public CHnMM createCHnMM_2(Observation[][] traces, params string[] keys)
    //    {
    //        PasswordModel helperModel = new PasswordModel(keys);
    //        helperModel.AddTraces(traces, keys);
    //        return helperModel.createCHnMM();

    //        /*
    //        int nKeys = keys.Length;
    //        int nTraces = traces.Length;
    //        keyObservations = new KeyObservations[nKeys];
    //        allSubPaths = new SubPaths[nKeys - 1];

    //        for (int i = 0; i < (nKeys - 1); i++)
    //            allSubPaths[i] = new SubPaths();

    //        long prevTime = 0;
    //        int dT = 0;
    //        int curKey = 0;
    //        bool inKeyState = false;
    //        LinkedList<string> curSymbolPath = null;
    //        LinkedList<int> curTimingPath = null;

    //        for (int i = 0; i < nKeys; i++)
    //            keyObservations[i] = new KeyObservations();

    //        //sort data to categories
    //        foreach (var O in traces)
    //        {
    //            prevTime = 0;
    //            curKey = 0;
    //            inKeyState = false;
    //            foreach (var o in O)
    //            {
    //                dT = (int)(o.Time - prevTime);
    //                prevTime = o.Time;

    //                if (o.Symbol.Equals(keys[curKey]))
    //                {
    //                    //toKey
    //                    keyObservations[curKey].timesToKey.AddLast(dT);
    //                    inKeyState = true;

    //                    //finish old subpath
    //                    if (curKey > 0)
    //                    {
    //                        allSubPaths[curKey - 1].symbols.AddLast(curSymbolPath);
    //                        allSubPaths[curKey - 1].times.AddLast(curTimingPath);
    //                    }
    //                }
    //                else if (inKeyState)
    //                {
    //                    //fromKey
    //                    inKeyState = false;
    //                    keyObservations[curKey].timesFromKey.AddLast(dT);
    //                    keyObservations[curKey].symbolsFromKey.AddLast(o.Symbol);
    //                    curKey++;

    //                    //start new subPath
    //                    curSymbolPath = new LinkedList<string>();
    //                    curTimingPath = new LinkedList<int>();
    //                }
    //                else
    //                {
    //                    //betweenKeys
    //                    curSymbolPath.AddLast(o.Symbol);
    //                    curTimingPath.AddLast(dT);
    //                }
    //            }
    //        }

    //        //create model
    //        CHnMM model = new CHnMM();

    //        */

    //        /*
    //        //train model
    //        var iter = passwordKeys.GetEnumerator();
    //        String key;
    //        int keyState = 0;
    //        long keyTime = 0;
    //        long prevTime = 0;
    //        int dT = 0;
    //        byte curPhase = 0; //0-PreKeyState, 1-PostKeyState, 2-GapState

    //        foreach (Observation[] trace in traces)
    //        {
    //            keyState = 0;
    //            keyTime = 0;
    //            prevTime = 0;
    //            iter.Reset();
    //            iter.MoveNext();
    //            key = (string)iter.Current;

    //            foreach (Observation o in trace)
    //            {
    //                dT = (int)(o.Time - prevTime);
    //                prevTime = o.Time;

    //                //preKeyState?
    //                if (o.Symbol.Equals(key))
    //                {
    //                    if (iter.MoveNext()) key = (string)iter.Current;
    //                    if (keyState > 0) dTs[keyState - 1][1].AddLast((int)(o.Time - keyTime));

    //                    curPhase = 1; //PostKeyState
    //                }
    //                else
    //                {
    //                    //symbol is from old keyState or between two keyStates
    //                    if (curPhase == 1)
    //                    {
    //                        dTs[keyState][0].AddLast(dT);
    //                        if (symbols[keyState][0].ContainsKey(o.Symbol)) symbols[keyState][0][o.Symbol]++;
    //                        else symbols[keyState][0][o.Symbol] = 1;
    //                        symbolCount[keyState][0]++;
    //                        keyTime = o.Time;
    //                        keyState += 2;
    //                        curPhase = 2;
    //                    }
    //                    else
    //                    {
    //                        //symbol from gapState
    //                        dTs[keyState - 1][0].AddLast(dT);
    //                        if (symbols[keyState - 1][0].ContainsKey(o.Symbol)) symbols[keyState - 1][0][o.Symbol]++;
    //                        else symbols[keyState - 1][0][o.Symbol] = 1;
    //                        symbolCount[keyState - 1][0]++;
    //                    }
    //                }
    //            }
    //        }

    //        //apply distributions and symbols
    //        int mean = 0;
    //        int sig = 0;
    //        iter.Reset();
    //        iter.MoveNext();
    //        for (int s = 0; s < nStates - 1; s++)
    //        {
    //            if (s % 2 == 0)
    //            {
    //                mean = calcMean(dTs[s][0]);
    //                sig = calcSigma(dTs[s][0], mean);

    //                transitions[s][0].B = createB(symbols[s][0], symbolCount[s][0]);
    //                transitions[s][0].Dist = new NormalDistribution(mean, sig);
    //            }
    //            else
    //            {
    //                mean = calcMean(dTs[s][0]);
    //                //var = calcVariance(dTs[s][0], mean);

    //                transitions[s][0].B = createB(symbols[s][0], symbolCount[s][0]);
    //                transitions[s][0].Dist = new ExponentialDistribution(mean);

    //                mean = calcMean(dTs[s][1]);
    //                sig = calcSigma(dTs[s][1], mean);

    //                iter.MoveNext();
    //                transitions[s][1].B = new Dictionary<string, double>(4);
    //                transitions[s][1].B.Add((string)iter.Current, 1.0);
    //                transitions[s][1].Dist = new NormalDistribution(mean, sig);
    //            }
    //        }
    //        */
    //    }

    //    private int calcMean(LinkedList<int> list)
    //    {
    //        int sum = 0;
    //        foreach (int x in list)
    //            sum += x;

    //        return sum/list.Count;
    //    }

    //    private int calcSigma(LinkedList<int> list, int mean)
    //    {
    //        int sum = 0;
    //        foreach (int x in list)
    //            sum += (x-mean)*(x-mean);

    //        return (int)System.Math.Sqrt((double)sum / (list.Count-1));
    //    }

    //    private Dictionary<string, double> createB(Dictionary<string,int> symbols, int count)
    //    {
    //        Dictionary<string, double> res = new Dictionary<string, double>();

    //        foreach (var pair in symbols)
    //            res.Add(pair.Key, (double)pair.Value / count);

    //        return res;
    //    }
    //}
}
