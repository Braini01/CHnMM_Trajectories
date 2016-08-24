using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GestureRecognitionLib.CHnMM
{
    public interface IStrokeInterpolation
    {
        TrajectoryPoint[] Points { get; }
        double ArcLength { get; }
        TrajectoryPoint getByArcLength(double arcLen);
    }

    public class LinearInterpolation : IStrokeInterpolation
    {
        private TrajectoryPoint[] srcPoints;
        private double[] arcLengths;

        public double ArcLength { get { return arcLengths[srcPoints.Length-1]; } }

        public LinearInterpolation(TrajectoryPoint[] points)
        {
            if (points.Length < 2) throw new ArgumentException("Mindestanzahl an Punkten für die lineare Interpolation ist 2", "points");
            srcPoints = points;

            //Streckenlängen berechnen
            double curArcLen = 0;
            arcLengths = new double[points.Length];
            arcLengths[0] = 0;
            var prevTp = points.First();
            int i = 1;
            foreach (var tp in points.Skip(1))
            {
                var difX = prevTp.X - tp.X;
                var difY = prevTp.Y - tp.Y;
                var dis = Math.Sqrt(difX * difX + difY * difY);

                curArcLen += dis;
                arcLengths[i++] = curArcLen;
                prevTp = tp;
            }
        }

        public TrajectoryPoint[] Points { get { return srcPoints; } }

        public TrajectoryPoint getByArcLength(double arcLen)
        {
            //sanity check
            if (arcLen < 0 || arcLen > ArcLength) throw new ArgumentOutOfRangeException("arcLen");

            //fast routes
            if (arcLen == 0) return srcPoints[0];
            if (arcLen == ArcLength) return srcPoints.Last();

            //binary search to find two corresponding points
            int p = -1;
            int u = 0;
            int o = arcLengths.Length-1;

            //solange bis 2 benachbarte Punkte oder der exakte Wert gefunden sind
            while ((o - u) > 1)
            {
                p = u + ((o - u) / 2);
                if (arcLengths[p] > arcLen)
                {
                    o = p;
                }
                else if(arcLengths[p] < arcLen)
                {
                    u = p;
                }
                else
                {
                    return srcPoints[p];
                }
            }

            Debug.Assert(arcLengths[u] < arcLen && arcLengths[o] > arcLen);

            //zwischen den 2 benachbarten Punkten linear interpolieren
            var p1 = srcPoints[u];
            var p2 = srcPoints[o];

            var dirVec = new { X = p2.X - p1.X, Y = p2.Y - p2.Y };
            var vecLen = arcLengths[o] - arcLengths[u];
            var newVecLen = arcLen - arcLengths[u];
            var scale = newVecLen / vecLen;

            var time = p1.Time + (long)((p2.Time - p1.Time) * scale);
            return new TrajectoryPoint(p1.X + dirVec.X * scale,p1.Y + dirVec.Y * scale, time);
        }

        //ToDo: könnte für lineare Interpolation optimiert werden (Verzicht auf binäre Suche) (dann nicht mehr static)
        public static TrajectoryPoint[] getEquidistantPoints(IStrokeInterpolation s, int nNewPoints)
        {
            if (nNewPoints < 2) throw new ArgumentException("Stroke muss in mindestens 2 Abschnitte eingeteilt werden", "nNewPoints");

            var result = new TrajectoryPoint[nNewPoints];

            //first one is clear
            result[0] = s.getByArcLength(0);

            var stepSize = s.ArcLength / (nNewPoints - 1);
            var curArcLen = stepSize;
            for (int j = 1; j < nNewPoints - 1; j++)
            {
                result[j] = s.getByArcLength(curArcLen);
                curArcLen += stepSize;
            }

            //last one is also clear
            result[nNewPoints - 1] = s.getByArcLength(s.ArcLength);

            return result;
        }


        //ToDo: könnte für lineare Interpolation optimiert werden (Verzicht auf binäre Suche) (dann nicht mehr static)
        public static TrajectoryPoint[] getPointsByDistance(IStrokeInterpolation s, double distance)
        {
            if (distance <= double.Epsilon) throw new ArgumentException("Distanz muss größer sein", "distance");

            var nPoints =  (int)(s.ArcLength / distance);

            var result = new TrajectoryPoint[nPoints];

            var curArcLen = distance; 
            for (int i = 0; i < nPoints; i++)
            {
                result[i] = s.getByArcLength(curArcLen);
                curArcLen += distance;
            }

            //what about points exactly at or close to the end?

            return result;
        }
    }
}
