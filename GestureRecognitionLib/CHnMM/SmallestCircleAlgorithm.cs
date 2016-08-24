using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib.CHnMM.Algorithms
{
	public static class SmallestCircleAlgorithm
	{
		private static void Shuffle<T>(List<T> list)
		{
			Random rng = new Random();
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static Circle makeCircle(IEnumerable<Point> points)
		{
			// Clone list to preserve the caller's data, randomize order
			var shuffled = points.ToList();
			Shuffle(shuffled);
		
			// Progressively add points to circle or recompute circle
			Circle c = null;
			for (int i = 0; i < shuffled.Count; i++)
			{
				Point p = shuffled[i];
				if (c == null || !c.contains(p))
					c = makeCircleOnePoint(shuffled.Take(i + 1).ToList(), p);
			}
			return c;
		}
	
	
		// One boundary point known
		private static Circle makeCircleOnePoint(List<Point> points, Point p) 
		{
			Circle c = new Circle(p, 0);
			for (int i = 0; i < points.Count; i++) 
			{
				Point q = points[i];
				if (!c.contains(q)) 
				{
					if (c.r == 0)
						c = makeDiameter(p, q);
					else
						c = makeCircleTwoPoints(points.Take(i + 1).ToList(), p, q);
				}
			}
			return c;
		}
	
		// Two boundary points known
		private static Circle makeCircleTwoPoints(List<Point> points, Point p, Point q)
		{
			Circle temp = makeDiameter(p, q);
			if (temp.contains(points))
				return temp;
		
			Circle left = null;
			Circle right = null;
			foreach (Point r in points) 
			{  // Form a circumcircle with each point
				Point pq = q.subtract(p);
				double cross = pq.cross(r.subtract(p));
				Circle c = makeCircumcircle(p, q, r);
				if (c == null)
					continue;
				else if (cross > 0 && (left == null || pq.cross(c.c.subtract(p)) > pq.cross(left.c.subtract(p))))
					left = c;
				else if (cross < 0 && (right == null || pq.cross(c.c.subtract(p)) < pq.cross(right.c.subtract(p))))
					right = c;
			}
			return right == null || left != null && left.r <= right.r ? left : right;
		}
	
	
		private static Circle makeDiameter(Point a, Point b) 
		{
			return new Circle(new Point((a.x + b.x)/ 2, (a.y + b.y) / 2), a.distance(b) / 2);
		}

		private static Circle makeCircumcircle(Point a, Point b, Point c)
		{
			// Mathematical algorithm from Wikipedia: Circumscribed circle
			double d = (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) * 2;
			if (d == 0)
				return null;
			double x = (a.norm() * (b.y - c.y) + b.norm() * (c.y - a.y) + c.norm() * (a.y - b.y)) / d;
			double y = (a.norm() * (c.x - b.x) + b.norm() * (a.x - c.x) + c.norm() * (b.x - a.x)) / d;
			Point p = new Point(x, y);
			return new Circle(p, p.distance(a));
		}
	}

	public class Circle 
	{
		private static double EPSILON = 1e-12;
	
		public Point c;   // Center
		public double r;  // Radius
	
		public Circle(Point c, double r) 
		{
			this.c = c;
			this.r = r;
		}
	
		public bool contains(Point p) 
		{
			return c.distance(p) <= r + EPSILON;
		}
	
		public bool contains(IEnumerable<Point> ps) 
		{
			foreach(var p in ps) 
			{
				if (!contains(p))
					return false;
			}
			return true;
		}
	}

	public class Point 
	{
		public double x;
		public double y;
	
	
		public Point(double x, double y) 
		{
			this.x = x;
			this.y = y;
		}
	
	
		public Point subtract(Point p) 
		{
			return new Point(x - p.x, y - p.y);
		}
	
		public double distance(Point p) 
		{
			var difX = x - p.x;
			var difY = y - p.y;
			return Math.Sqrt(difX * difX + difY * difY);
		}
	
		// Signed area / determinant thing
		public double cross(Point p) 
		{
			return x * p.y - y * p.x;
		}
	
		// Magnitude squared
		public double norm() 
		{
			return x * x + y * y;
		}
	}
}
