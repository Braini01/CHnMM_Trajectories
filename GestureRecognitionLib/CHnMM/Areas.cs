using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestureRecognitionLib.CHnMM.Algorithms;

namespace GestureRecognitionLib.CHnMM
{
	public interface IArea
	{
		/// <summary>
		/// returns whether a given point lies within the area
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		bool PointInArea(double x, double y);

		/// <summary>
		/// returns whether a given point lies within the tolerance area
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		bool PointInToleranceArea(double x, double y);
	}

	public abstract class Circle : IArea
	{
		public static double minimumRadius = 0.03;
		public static double toleranceFactor = 1.4;
        public static bool useAdaptiveTolerance = false;

		public double X { get; protected set; }
		public double Y { get; protected set; }
		public double Radius { get; protected set; }
		public double ToleranceRadius { get; protected set; }

		public bool PointInArea(double x, double y)
		{
			var difX = x - X;
			var difY = y - Y;
			var dis = Math.Sqrt(difX * difX + difY * difY);
			if (dis <= Radius) return true;

			return false;
		}

		public bool PointInToleranceArea(double x, double y)
		{
			var difX = x - X;
			var difY = y - Y;
			var dis = Math.Sqrt(difX * difX + difY * difY);
			if (dis <= ToleranceRadius) return true;

			return false;
		}

		public Circle()
		{
			X = 0; Y = 0; Radius = 0; ToleranceRadius = 0;
		}
	}

	/// <summary>
	/// a circle with the center at the average of all positions 
	/// </summary>
	public class WeightedEnclosingCircle : Circle
	{
		public WeightedEnclosingCircle(IEnumerable<TrajectoryPoint> srcPoints)
		{
			//calculate center
			X = srcPoints.Average(p => p.X);
			Y = srcPoints.Average(p => p.Y);

			//calculate radius
			Radius = srcPoints.Max(p => { var dx = X - p.X; var dy = Y - p.Y; return Math.Sqrt(dx * dx + dy * dy); });
			if (Radius < minimumRadius) Radius = minimumRadius;

            if (useAdaptiveTolerance)
            {
                var n = srcPoints.Count();
                var f = ((double)(n + 1) / n); //converges from 2 to 1
                ToleranceRadius = Radius + (Radius * toleranceFactor - Radius)*f;
            }
            else
            {
                ToleranceRadius = Radius * toleranceFactor;
            }
		}
	}

	/// <summary>
	/// a minimal circle enclosing all training points
	/// </summary>
	public class SmallestEnclosingCircle : Circle
	{
		public SmallestEnclosingCircle(IEnumerable<TrajectoryPoint> srcPoints)
		{
			var enclCirc = SmallestCircleAlgorithm.makeCircle(srcPoints.Select(tp => new Point(tp.X, tp.Y)));

			//calculate center
			X = enclCirc.c.x;
			Y = enclCirc.c.y;

			//calculate radius
			Radius = enclCirc.r;
			if (Radius < minimumRadius) Radius = minimumRadius;

            if (useAdaptiveTolerance)
            {
                var n = srcPoints.Count();
                var f = ((double)(n + 1) / n); //converges from 2 to 1
                ToleranceRadius = Radius + (Radius * toleranceFactor - Radius) * f;
            }
            else
            {
                ToleranceRadius = Radius * toleranceFactor;
            }
        }
	}
}
