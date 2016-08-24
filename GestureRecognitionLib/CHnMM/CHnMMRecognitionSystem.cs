using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib.CHnMM
{
    public class CHnMMRecognitionSystem : IRecognitionSystem, IVerificationSystem
    {
        private Dictionary<string, TrajectoryModel> knownGestures = new Dictionary<string, TrajectoryModel>(100);

        //public Configuration Parameter { get; private set; }

        public GestureModelCreator HiddenModelCreator;

        public CHnMMRecognitionSystem(Configuration parameter)
        {
            //Parameter = parameter;

            //apply parameters
            Circle.minimumRadius = parameter.minRadiusArea;
            Circle.toleranceFactor = parameter.toleranceFactorArea;
            Circle.useAdaptiveTolerance = parameter.useAdaptiveTolerance;

            StrokeMap.translationInvariant = parameter.isTranslationInvariant;
            FixedAreaNumberStrokeMap.nAreas = parameter.nAreaForStrokeMap;
            FixedAreaNumberStrokeMap.useSmallestCircle = parameter.useSmallestCircle;
            DynamicAreaNumberStrokeMap.AreaPointDistance = parameter.areaPointDistance;
            DynamicAreaNumberStrokeMap.useSmallestCircle = parameter.useSmallestCircle;


            var transitionSetup = new TransitionCreator(parameter.hitProbability, parameter.distEstName);

            if (parameter.useFixAreaNumber)
            {
                //HiddenModelCreator = new FixedAreaNumberModelCreator(transitionSetup);
                HiddenModelCreator = new FixedAreaNumberModelCreator(transitionSetup);
            }
            else
            {
                HiddenModelCreator = new DynamicAreaNumberModelCreator(transitionSetup);
            }

        }

        public void clearGestures()
        {
            knownGestures.Clear();
        }

        public IEnumerable<string> getKnownGesturenames()
        {
            return knownGestures.Keys;
        }

        public TrajectoryModel getTrajectoryModel(string gestureName)
        {
            return knownGestures[gestureName];
        }

        public void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces)
        {
            knownGestures[gestureName] = new TrajectoryModel(this, gestureName, trainingTraces);
        }

        public bool verifyGesture(string userName, BaseTrajectory trace)
        {
            var targetGesture = knownGestures[userName];

            var similarity = targetGesture.validateGestureTrace(trace);
            //ToDo: evtl. Schwellwerte hier prüfen?

            return (similarity > 0);
        }

        public bool authenticateUser(string userName, GestureTrace trace, out TrajectoryModel.ReasonForFail failReason)
        {
            var targetGesture = knownGestures[userName];
            var similarity = targetGesture.validateGestureTrace(trace.LongestStroke, out failReason);

            //ToDo: evtl. Schwellwerte hier prüfen?
            return (similarity > 0);
        }

        public string recognizeGesture(BaseTrajectory trace)
        {
            var calculations = knownGestures.Select(gest => new { GestureName = gest.Key, Similarity = gest.Value.validateGestureTrace(trace) });

            var bestGesture = calculations.MaxBy(g => g.Similarity);

            if (bestGesture.Similarity == 0) return null;
            else return bestGesture.GestureName;
        }

        public string recognizeGesture(string gestureName, BaseTrajectory trace, out TrajectoryModel.ReasonForFail failReason)
        {
            //GestureRepresentation.ReasonForFail failReason;
            //var calculations = knownGestures.Select((gest,i) => new { GestureName = gest.Key, Similarity = gest.Value.validateGestureTrace(trace, out failReasons[i]) });

            double best = 0;
            string winner = "";
            TrajectoryModel.ReasonForFail tmp;
            failReason = TrajectoryModel.ReasonForFail.UNDEFINED;
            foreach (var gest in knownGestures)
            {
                var sim = gest.Value.validateGestureTrace(trace, out tmp);
                if (gest.Key == gestureName) failReason = tmp;
                if (sim > best)
                {
                    best = sim;
                    winner = gest.Key;
                }
            }

            if (best == 0) return null;
            else return winner;
        }
    }
}
