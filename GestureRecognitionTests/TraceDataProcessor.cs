using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestureRecognitionLib;
using GestureRecognitionLib.CHnMM;

namespace LfS.GestureRecognitionTests
{
    using GestureDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public interface ITraceDataProcessor
    {
        GestureDataSet processAllGestures(GestureDataSet data);

        //GestureDataSet processTrainingGestures();

        //GestureDataSet processTestGestures();
    }

    public class DynamicAreaPointSampling : ITraceDataProcessor
    {
        private double pointDistance = 0.05;

        public DynamicAreaPointSampling(double pointDistance)
        {
            this.pointDistance = pointDistance;
        }

        public GestureDataSet processAllGestures(GestureDataSet data)
        {
            return data.ToDictionary(
                e => e.Key,
                e => e.Value.Select(t =>
                {
                    if (t is Stroke)
                    {
                        var s = t as Stroke;
                        return (BaseTrajectory)(new Stroke(samplePoints(s.TrajectoryPoints), s.FingerID));
                    }
                    else throw new NotImplementedException("This subclass of BaseTrajectory is currently not implemented here");
                } ));
        }

        private TrajectoryPoint[] samplePoints(TrajectoryPoint[] srcPoints)
        {
            var interpolated = new LinearInterpolation(srcPoints);
            return LinearInterpolation.getPointsByDistance(interpolated, pointDistance);
        }
    }
}
