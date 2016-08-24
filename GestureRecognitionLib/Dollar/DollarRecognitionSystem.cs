using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib.Dollar
{
    public class DollarRecognitionSystem : IRecognitionSystem
    {
        private Recognizer recognizer = new Recognizer();

        public void clearGestures()
        {
            recognizer.ClearGestures();
        }

        public IEnumerable<string> getKnownGesturenames()
        {
            return recognizer.Gestures.Select(us => us.Name).Distinct();
        }

        public string recognizeGesture(BaseTrajectory trace)
        {
            var points = trace.TrajectoryPoints.Select(p => new WobbrockLib.TimePointF(p.X, p.Y, p.Time)).ToList();
            var results = recognizer.Recognize(points, true);

            return results.Name;
        }

        public void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces)
        {
            var convertedTraces = trainingTraces.Select(tt =>
                tt.TrajectoryPoints.Select(p => new WobbrockLib.TimePointF(p.X, p.Y, p.Time)).ToList());

            recognizer.AddTemplates(gestureName, convertedTraces);
        }
    }
}
