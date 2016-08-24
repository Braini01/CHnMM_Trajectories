using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace GestureRecognitionLib
{
    public interface IRecognitionSystem
    {
        IEnumerable<string> getKnownGesturenames();
        string recognizeGesture(BaseTrajectory trace);
        void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces);
        void clearGestures();
    }

    public interface IVerificationSystem
    {
        void clearGestures();
        bool verifyGesture(string gestureName, BaseTrajectory trace);
        void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces);
    }
}
