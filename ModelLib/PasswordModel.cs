using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LfS.ModelLib.Common;
using LfS.GestureDatabase;
using LfS.ModelLib.Common.Distributions;

namespace LfS.ModelLib.Models
{
    //public class ProxelSet : IEnumerable<Proxel>
    //{
    //    private Dictionary<State, Proxel> proxels = new Dictionary<State, Proxel>();

    //    public void Add(Proxel p)
    //    {
    //        //here the Proxelmerging happens
    //        Proxel stateP = getProxelByState(p.State);
    //        if (stateP != null) stateP.P += p.P; //ToDo: check if change to stateP is propagated to the dictionary
    //        else
    //        {
    //            proxels[p.State] = p;
    //            Count++;
    //        }
    //    }

    //    public int Count { get; private set; }

    //    public Proxel getProxelByState(State s)
    //    {
    //        Proxel proxel;
    //        return (proxels.TryGetValue(s, out proxel)) ? proxel : null;
    //    }

    //    public IEnumerator<Proxel> GetEnumerator()
    //    {
    //        return proxels.Values.GetEnumerator();
    //    }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return proxels.Values.GetEnumerator();
    //    }
    //}

    //public class Proxel
    //{
    //    public State State { get; private set; }
    //    public double P { get; set; }

    //    public Proxel(State s, double p)
    //    {
    //        State = s;
    //        P = p;
    //    }
    //}

    public class Observation
    {
        public String Symbol { get; set; }
        public long Time { get; set; }

        public Observation(String sym, long time)
        {
            Symbol = sym;
            Time = time;
        }

        public override string ToString()
        {
            return "("+ Symbol + ";" + Time + ")";
        }
    }

    [Serializable()]
    public class State: IModelState
    {
        public LinkedList<Transition> Transitions
        {
            get;
            private set;
        }


        public State()
        {
            Transitions = new LinkedList<Transition>();
        }
        public State(IEnumerable<Transition> transitions)
        {
            Transitions = new LinkedList<Transition>(transitions);
        }

        public void addTransition(Transition t)
        {
            Transitions.AddLast(t);
        }
    }

    public class StartState : State
    {
        //public int Depth { get; private set; }
        public double InitialProbability { get; set; }


        public StartState()
            : base()
        {
            InitialProbability = 1.0;
        }
        public StartState(double initProb)
            : base()
        {
            InitialProbability = initProb;
        }
        public StartState(Transition[] transitions)
            : base(transitions)
        {
            InitialProbability = 1.0;
        }
        public StartState(double initProb, Transition[] transitions) : base(transitions)
        {
            InitialProbability = initProb;
        }
    }
    

    [Serializable()]
    public class Transition
    {
        public Transition(State s, IDistribution dist, Dictionary<string, double> probs)
        {
            PostState = s;
            Dist = dist;
            OutputProbs = probs;
        }

        public State PostState
        {
            get;
            set;
        }

        public IDistribution Dist
        {
            get;
            private set;
        }

        public Dictionary<string, double> OutputProbs
        {
            get;
            private set;
        }
    }

    [Serializable()]
    public class HiddenModel
    {
        StartState[] startStates;

        //private double maxEvaluation = 0;

        //public double MaxEvaluation { get { return maxEvaluation; } }

        public StartState[] StartStates { get { return startStates; } }
        public State EndState { get; private set; }

        public HiddenModel(StartState start, State endState)
        {
            startStates = new StartState[] { start };
            EndState = endState;

            //maxEvaluation = maxEvaluate();
        }
        public HiddenModel(StartState[] starts, State endState)
        {
            startStates = starts;
            EndState = endState;

            //maxEvaluation = maxEvaluate();
        }

        private ProxelSet forwardStep(ProxelSet proxels, int dT, string symbol)
        {
            ProxelSet newProxels = new ProxelSet();

            foreach (Proxel p in proxels)
            {
                var transitions = ((State)p.State).Transitions;
                if (transitions == null || transitions.Count < 1) continue;

                double P = 1; //Psojourn
                foreach (Transition t in transitions)
                    P *= 1 - t.Dist.getCDF(dT);

                if (P == 0)
                    continue;

                //create childproxel for every active transition
                double b;
                foreach (Transition t in transitions)
                    if (t.OutputProbs.TryGetValue(symbol, out b))
                    {
                        double z = t.Dist.getHRF(dT);

                        if (z > 0)
                            newProxels.Add(new Proxel(t.PostState, p.P * P * z * b));
                    }
            }

            return newProxels;
        }

        public double evaluate(IEnumerable<Observation> O, bool endStateOnly = false)
        {
            var currentProxels = new ProxelSet();
            //create initialProxel
            for (int i = 0; i < startStates.Length; i++)
                currentProxels.Add(new Proxel(startStates[i], startStates[i].InitialProbability));

            long prevTime = O.First().Time;
            foreach (var o in O.Skip(1))
            {
                if (currentProxels.Count <= 0) break;
                currentProxels = forwardStep(currentProxels, (int)(o.Time - prevTime), o.Symbol);
                prevTime = o.Time;
            }

            var sum = 0d;

            if (endStateOnly)
            {
                var endProxels = currentProxels.Where(p => p.State == EndState);
                foreach (Proxel p in endProxels)
                    sum += p.P;
            }
            else
            {
                foreach (Proxel p in currentProxels)
                    sum += p.P;
            }

            return sum;
        }

        //public double maxEvaluate()
        //{
        //    var P = 1d;
        //    State curState = startStates[0];
        //    var t = curState.Transitions.First();
        //    while (true)
        //    {
        //        int bestTime = 0;
        //        if (t.Dist is NormalDistribution)
        //            bestTime = ((NormalDistribution)t.Dist).Mean;
        //        if (t.Dist is UniformDistribution)
        //            bestTime = ((UniformDistribution)t.Dist).Min;

        //        if (t.Dist is DeterministicDistribution) P *= 1;
        //        else P *= (1 - t.Dist.getCDF(bestTime)) * t.Dist.getHRF(bestTime) * 0.9;
        //        curState = t.PostState;
        //        if (curState.Transitions.Count == 0) break;
        //        t = curState.Transitions.First();
        //    }

        //    return P;
        //}

        public override string ToString()
        {
            State state = this.startStates[0];
            var res = "";

            while(state != null)
            {
                var t = state.Transitions.First();
                string symbols = "(";
                
                foreach(var e in t.OutputProbs)
                    symbols += e.Key + "-" + e.Value.ToString() + ",";

                symbols += ")";

                res += symbols;
                state = t.PostState;
            }

            return res.ToString();
        }

        /// <summary>
        /// for now only linear transitions (no branches)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Transition> getTransitions()
        {
            State state = startStates[0];

            while (state != null && state.Transitions.Count > 0)
            {
                var t = state.Transitions.First();

                yield return t;
                
                state = t.PostState;
            }
        }

        public void saveToFile(string fileName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            var f = File.Create(fileName);
            bf.Serialize(f,this);
            f.Close();
        }

        public static HiddenModel readFromFile(string fileName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (HiddenModel)bf.Deserialize(File.OpenRead(fileName));
        }
    }

    public class GestureModel
    {
        //a model per feature
        HiddenModel[] models;
        List<TraceFeatures> featureTraces = new List<TraceFeatures>(50);
        private string[] activeFeatures = { "X", "Y" };

        public GestureModel(IEnumerable<ICollection<Touch>> userTraces)
        {
            models = new HiddenModel[activeFeatures.Length];
            var modelCreator = new SimpleMultiplePathsModelCreator();
            //calc features foreach trace
            
            foreach (var trace in userTraces)
                featureTraces.Add(new TraceFeatures(trace));

            int iModel = 0;
            foreach(var feature in activeFeatures)
            {
                var symbolTracesByFeature = new LinkedList<ICollection<Observation>>();
                var symbolGenerator = new ExtremaSymbolGenerator(feature, true);
                foreach(var featureTrace in featureTraces)
                {
                    var symbolTraces = symbolGenerator.generateSymbolTrace(featureTrace);
                    symbolTracesByFeature.AddLast(symbolTraces);

                }

                models[iModel++] = modelCreator.createModel(symbolTracesByFeature);
            }
        }

        public double evaluate(ICollection<Touch> trace, bool endStateOnly = false)
        {
            var evals = new double[activeFeatures.Length];
            var traceFeature = new TraceFeatures(trace);

            int iModel = 0;
            foreach (var feature in activeFeatures)
            {
                var symbolGenerator = new ExtremaSymbolGenerator(feature, true);
                evals[iModel] = models[iModel].evaluate(symbolGenerator.generateSymbolTrace(traceFeature), endStateOnly);
                iModel++;
            }

            if (evals.Any(e => e == 0 || double.IsNaN(e))) return 0;
            return evals.Average();
        }
    }
}
