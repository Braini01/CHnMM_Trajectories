using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using MathNet.Numerics.Interpolation.Algorithms;
using LfS.ModelLib.Models;

namespace GestureRecognitionLib.CHnMM
{

//    public enum IntersectionType
//    {
//        None = 0,
//        LineEntersArea,
//        LineInArea,
//        LineLeavesArea,
//        LineCompletelyIntersectsArea
//    }

//    public class Area
//    {
//        //private TouchPoint[] srcPoints;
//        private double minimumRadius = 0.03;

//        public double X { get; private set; }
//        public double Y { get; private set; }

//        //ToDo: evtl. Ellipse?
//        public double Radius { get; private set; }

//        public double ToleranceRadius { get; private set; }

//        /// <summary>
//        /// returns whether a given points lies within the area
//        /// </summary>
//        /// <param name="x"></param>
//        /// <param name="y"></param>
//        /// <returns></returns>
//        public bool PointInArea(double x, double y)
//        {
//            var difX = x - X;
//            var difY = y - Y;
//            var dis = Math.Sqrt( difX*difX + difY*difY );
//            if (dis <= Radius) return true;

//            return false;
//        }

//        /// <summary>
//        /// returns whether a given points lies within the tolerance area
//        /// </summary>
//        /// <param name="x"></param>
//        /// <param name="y"></param>
//        /// <returns></returns>
//        public bool PointInToleranceArea(double x, double y)
//        {
//            var difX = x - X;
//            var difY = y - Y;
//            var dis = Math.Sqrt(difX * difX + difY * difY);
//            if (dis <= ToleranceRadius) return true;

//            return false;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="x1"></param>
//        /// <param name="y1"></param>
//        /// <param name="x2"></param>
//        /// <param name="y2"></param>
//        /// <returns> 0 - line does not intersect, 1 - line enters area, 2 - line lies within area, 3 - line leaves area, 4 - line completely intersects area</returns>
//        public IntersectionType LineIntersectsArea(double x1, double y1, double x2, double y2)
//        {
            

//            //if (inArea1 == 1)
//            //{
//            //    //erster Punkt in Area
                 
//            //}


//            //Geradenparameter aus gegebenen Punkten bestimmen
//            //Koordinatenform für Geraden: ax + by = c
//            //Quelle: http://de.wikipedia.org/wiki/Koordinatenform#Aus_der_Zweipunkteform
//            var a = y1 - y2;
//            var b = x2 - x1;
//            var c = x2 * y1 - x1 * y2;

//            //Anzahl der Schnittpunkte bestimmen
//            //Quelle: http://de.wikipedia.org/wiki/Schnittpunkt#Schnittpunkte_einer_Gerade_mit_einem_Kreis
//            var d = c - a * X - b * Y;
//            var det = Radius * Radius * (a * a + b * b);
            
//            if(det < d)
//            {
//                //kein Schnittpunkt
//                return 0;
//            }
//            else
//            {
//                var inArea1 = PointInArea(x1,y1);
//                var inArea2 = PointInArea(x2,y2);

//                if (det > d)
//                {
//                    //2 Schnittpunkte
//                    if(inArea1)
//                    {
//                        if (inArea2) return IntersectionType.LineInArea;
//                        else return IntersectionType.LineLeavesArea;
//                    }
//                    else
//                    {
//                        if (inArea2) return IntersectionType.LineEntersArea;
//                        else
//                        {
//                            //beide Punkte außerhalb der Area; Gerade hat 2 Schnittpunkte
//                            //einen der 2 Schnittpunkte berechnen:

//                            var t1 = -(Math.Sqrt((-(x1 * x1) + 2 * X * x1 - X * X + Radius * Radius) * y2 * y2 + (((2 * x1 - 2 * X) * x2 - 2 * X * x1 + 2 * X * X - 2 * Radius * Radius) * y1 + ((2 * X - 2 * x1) * x2 + 2 * x1 * x1 - 2 * X * x1) * Y) * y2 + (-(x2 * x2) + 2 * X * x2 - X * X + Radius * Radius) * y1 * y1 +
//(2 * x2 * x2 + (-2 * x1 - 2 * X) * x2 + 2 * X * x1) * Y * y1 + (-(x2 * x2) + 2 * x1 * x2 - x1 * x1) * Y * Y + Radius * Radius * x2 * x2 - 2 * Radius * Radius * x1 * x2 + Radius * Radius * x1 * x1) - y2 * y2 + (y1 + Y) * y2 - Y * y1 - x2 * x2 + (x1 + X) * x2 - X * x1) / (y2 * y2 - 2 * y1 * y2 + y1 * y1 + x2 * x2 - 2 *
//x1 * x2 + x1 * x1);
//                            if (t1 >= 0 && t1 <= 1) return IntersectionType.LineCompletelyIntersectsArea;
//                            else return IntersectionType.None;

//                        }
//                    }

//                }
//                else //det == d
//                {
//                    //1 Schnittpunkt
//                    throw new NotImplementedException();
//                }
//            }
            
//        }

//        public Area(IEnumerable<TouchPoint> srcPoints, double toleranceFactor, double minRadius)
//        {
//            //ToDo: Varianten: Umkreis um Punkte oder gewichteten Umkreis um Punkte; momentan gewichtet
//            //srcPoints = points;

//            minimumRadius = minRadius;

//            //calculate Area center
//            X = srcPoints.Average(p => p.X);
//            Y = srcPoints.Average(p => p.Y);

//            //calculate radius
//            Radius = srcPoints.Max(p => { var dx = X - p.X; var dy = Y - p.Y; return Math.Sqrt(dx * dx + dy * dy); });
//            if(Radius < minimumRadius) Radius = minimumRadius;

//            ToleranceRadius = Radius * toleranceFactor;
//        }
//    }


    /// <summary>
    /// represents the areas created from a set of strokes
    /// </summary>
    public abstract class StrokeMap
    {
        public static bool translationInvariant = false;

        //used to provide unique IDs for each strokeMap which are used for symbols
        protected static int IDCounter = 0;
        protected IStrokeInterpolation[] interpolatedStrokes;

        public int ID { get; private set; }
        public IArea[] Areas { get; protected set; }

        public StrokeMap(BaseTrajectory[] srcStrokes)
        {
            //verwendet vorerst lineare Interpolation
            interpolatedStrokes = (translationInvariant) ?
                srcStrokes.Select(s => new LinearInterpolation(s.getInvariantPoints())).ToArray() :
                srcStrokes.Select(s => new LinearInterpolation(s.TrajectoryPoints)).ToArray();
            ID = IDCounter++;

            //sind Strokes in etwa gleichlang?
            var minArcLength = interpolatedStrokes.Min(s => s.ArcLength);
            var maxArcLength = interpolatedStrokes.Max(s => s.ArcLength);
            //Debug.Assert(maxArcLength - minArcLength < 0.5);
        }

        public abstract Observation[] getSymbolTrace(BaseTrajectory s);
    }

    /// <summary>
    /// represents the areas created from a set of strokes
    /// </summary>
    public class FixedAreaNumberStrokeMap : StrokeMap
    {
        public static int nAreas = 10;
        public static bool useSmallestCircle = true;

        public FixedAreaNumberStrokeMap(BaseTrajectory[] srcStrokes) 
            : base(srcStrokes)
        {
            if (useSmallestCircle)
                generateSmallestEnclosingCircles();
            else
                generateWeightedEnclosingCircles();
        }

        private void generateWeightedEnclosingCircles()
        {
            var equiDistantStrokePoints = interpolatedStrokes.Select(s => LinearInterpolation.getEquidistantPoints(s, nAreas)).ToArray();
            Areas = Enumerable.Range(0, nAreas).Select(
                        a => new WeightedEnclosingCircle(equiDistantStrokePoints.Select(points => points[a]))
                    ).ToArray();
        }

        private void generateSmallestEnclosingCircles()
        {
            var equiDistantStrokePoints = interpolatedStrokes.Select(s => LinearInterpolation.getEquidistantPoints(s, nAreas)).ToArray();
            Areas = Enumerable.Range(0, nAreas).Select(
                        a => new SmallestEnclosingCircle(equiDistantStrokePoints.Select(points => points[a]))
                    ).ToArray();
        }

        //public override bool strokeFitsMap(Stroke s)
        //{
        //    var areaPoints = LinearInterpolation.getEquidistantPoints(new LinearInterpolation(s.Points), areas.Length);
        //    return !areaPoints.Zip(areas,
        //                (p, a) => a.PointInToleranceArea(p.X, p.Y)
        //           ).Any(b => !b);
        //}

        // usage: someObject.SingleItemAsEnumerable();
        public static IEnumerable<T> SingleItemAsEnumerable<T>(T item)
        {
            yield return item;
        }

        public override Observation[] getSymbolTrace(BaseTrajectory s)
        {
            var strokeInterpolation = (translationInvariant) ?
                new LinearInterpolation(s.getInvariantPoints()) :
                new LinearInterpolation(s.TrajectoryPoints);
            var areaPoints = LinearInterpolation.getEquidistantPoints(strokeInterpolation, Areas.Length);

            int counter = 1;
            var query = areaPoints.Zip(Areas, (p, a) =>
            {
                string sym = "S" + ID + "_A";
                long time;
                if (a.PointInArea(p.X, p.Y))
                {
                    sym += counter++ + "_Hit";
                    time = p.Time;
                }
                else if (a.PointInToleranceArea(p.X, p.Y))
                {
                    sym += counter++ + "_Tolerance";
                    time = p.Time;
                }
                else return null;

                return new Observation(sym, time);
            });

            //GestureStart symbol
            var startO = new Observation("GestureStart", s.TrajectoryPoints[0].Time);
            var symbolTrace = SingleItemAsEnumerable(startO).Concat(query.TakeWhile(o => o != null)).ToArray();

            //wurden alle Areas erwischt?
            if (symbolTrace.Length < Areas.Length) return null;

            return symbolTrace;
        }
    }

    public class DynamicAreaNumberStrokeMap : StrokeMap
    {
        public static double AreaPointDistance = 0.05;
        public static bool useSmallestCircle = true;

        public DynamicAreaNumberStrokeMap(BaseTrajectory[] srcStrokes)
            : base(srcStrokes)
        {
            if (useSmallestCircle)
                generateSmallestEnclosingCircles();
            else
                generateWeightedEnclosingCircles();
        }

        private void generateWeightedEnclosingCircles()
        {
            var areaPointsPerStroke = interpolatedStrokes.Select(s => LinearInterpolation.getPointsByDistance(s, AreaPointDistance)).ToArray();
            //var maxNArea = areaPointsPerStroke.Max(areaPoints => areaPoints.Length);

            //Areas = Enumerable.Range(0, maxNArea).Select(
            //            a => new WeightedEnclosingCircle(areaPointsPerStroke.Where(points => points.Length > a).Select(points => points[a])) //Where was added here due to different area counts
            //        ).ToArray();

            var minNArea = areaPointsPerStroke.Min(areaPoints => areaPoints.Length);

            Areas = Enumerable.Range(0, minNArea).Select(
                        a => new WeightedEnclosingCircle(areaPointsPerStroke.Select(points => points[a]))
                    ).ToArray();
        }

        private void generateSmallestEnclosingCircles()
        {
            var areaPointsPerStroke = interpolatedStrokes.Select(s => LinearInterpolation.getPointsByDistance(s, AreaPointDistance)).ToArray();
            //var maxNArea = areaPointsPerStroke.Max(areaPoints => areaPoints.Length);

            //Areas = Enumerable.Range(0, maxNArea).Select(
            //            a => new SmallestEnclosingCircle(areaPointsPerStroke.Where(points => points.Length > a).Select(points => points[a])) //Where was added here due to different area counts
            //        ).ToArray();

            var minNArea = areaPointsPerStroke.Min(areaPoints => areaPoints.Length);

            Areas = Enumerable.Range(0, minNArea).Select(
                        a => new SmallestEnclosingCircle(areaPointsPerStroke.Select(points => points[a]))
                    ).ToArray();
        }

        //public override bool strokeFitsMap(Stroke s)
        //{
        //    var areaPoints = LinearInterpolation.getPointsByDistance(new LinearInterpolation(s.Points), AreaPointDistance);
        //    return !areaPoints.Zip(areas,
        //                (p, a) => a.PointInToleranceArea(p.X, p.Y)
        //           ).Any(b => !b);
        //}

        public override Observation[] getSymbolTrace(BaseTrajectory s)
        {
            var strokeInterpolation = (translationInvariant) ?
                new LinearInterpolation(s.getInvariantPoints()) :
                new LinearInterpolation(s.TrajectoryPoints);
            var areaPoints = LinearInterpolation.getPointsByDistance(strokeInterpolation, AreaPointDistance);

            //ToDo: tolerate longer inputs than trained?
            //if (areaPoints.Length > Areas.Length) return null;

            //ToDo: tolerate shorter inputs?
            if (areaPoints.Length < Areas.Length)
            {
                return null;
            }

            var observations = new Observation[Areas.Length + 2]; //+ GS and GE
            int iObs = 1;

            string baseSym = "S" + ID + "_A";
            string symbol;
            long time;
            for (int iArea = 0; iArea < Areas.Length; iArea++)
            {
                var area = Areas[iArea];
                if (iArea >= areaPoints.Length) break;

                var point = areaPoints[iArea];

                if(area.PointInArea(point.X, point.Y))
                {
                    symbol = baseSym + (iArea + 1) + "_Hit";
                    time = point.Time;
                }
                else if (area.PointInToleranceArea(point.X, point.Y))
                {
                    symbol = baseSym + (iArea + 1) + "_Tolerance";
                    time = point.Time;
                }
                else return null;

                observations[iObs++] = new Observation(symbol, time);
            }

            //GestureStart symbol
            observations[0] = new Observation("GestureStart", s.TrajectoryPoints[0].Time);
            //GestureEnd symbol
            observations[observations.Length - 1] = new Observation("GestureEnd", s.TrajectoryPoints[s.TrajectoryPoints.Length - 1].Time);

            return observations;
        }

    }
}
