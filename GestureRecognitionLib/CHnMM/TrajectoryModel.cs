using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LfS.ModelLib.Models;

namespace GestureRecognitionLib.CHnMM
{
    public class TrajectoryModel
    {
        private CHnMMRecognitionSystem recognitionSystem; //ToDo: remove this dependency
        private HiddenModel model; //maybe models for multi-touch
        private StrokeMap strokeMap; //one stroke per gesture for now

        //Gesture- or Username
        public string Name { get; private set; }
        public StrokeMap StrokeMap { get { return strokeMap; } }
        public HiddenModel CHnMM { get { return model; } }

        public TrajectoryModel(CHnMMRecognitionSystem system, string name, IEnumerable<BaseTrajectory> trainingData)
        {
            recognitionSystem = system;
            Name = name;

            strokeMap = system.HiddenModelCreator.createStrokeMap(trainingData);
            model = system.HiddenModelCreator.createModel(trainingData, strokeMap);
        }

        /// <summary>
        /// determines whether the given trace fits the gesture representation
        /// </summary>
        /// <param name="trace"></param>
        /// <returns>an arbitrary double value giving a measure for the similarity; 0 states that the given trace is no fitting example of the gesture </returns>
        public double validateGestureTrace(BaseTrajectory trace)
        {
            var O = strokeMap.getSymbolTrace(trace);

            if(O == null)
            {
                return 0;
            }

            //insert dummy symbol
            //O = new Observation[] { new Observation("Dummy", O[0].Time) }.Concat(O).ToArray();
            //also check time dynamics
            var eval = model.evaluate(O, true);

            //normalize
            return eval /*/ model.MaxEvaluation*/;
        }

        public enum ReasonForFail
        {
            NoFail,
            MissedArea,
            MissedTransition,
            TraceTooShort,
            UNDEFINED
        }

        public double validateGestureTrace(BaseTrajectory trace, out ReasonForFail failReason)
        {
            failReason = ReasonForFail.NoFail;

            var O = strokeMap.getSymbolTrace(trace);

            if (O == null)
            {
                failReason = ReasonForFail.MissedArea;
                return 0;
            }

            //insert dummy symbol
            //O = new Observation[] { new Observation("Dummy", O[0].Time) }.Concat(O).ToArray();
            //also check time dynamics
            var eval = model.evaluate(O, true);

            if (eval == 0) failReason = ReasonForFail.MissedTransition;

            //normalize
            return eval /*/ model.MaxEvaluation*/;
        }
    }
}
