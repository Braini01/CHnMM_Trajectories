using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using LfS.GestureDatabase;
using MathNet;

namespace LfS.ModelLib
{
    //public class Feature
    //{
    //    public string Name { get; private set; }
    //    public decimal[] Data { get; private set; }

    //    public Feature(string name, decimal[] data)
    //    {
    //        Name = name;
    //        Data = data;
    //    }

    //}



    public class Feature
    {
        public string Name { get; private set; }
        public double[] Data { get; private set; }
        public MathNet.Numerics.Interpolation.Algorithms.AkimaSplineInterpolation Spline { get; private set; }

        public Feature(string name, double[] data)
        {
            Name = name;
            Data = data;
        }

        public Feature(string name, MathNet.Numerics.Interpolation.Algorithms.AkimaSplineInterpolation spline)
        {
            Name = name;
            Spline = spline;
        }
    }

    public class TraceFeatures : Dictionary<string,Feature>
    {
        public int[] Time { get; private set; }
        public int[] TimeInterpolated { get; private set; }

        public TraceFeatures(ICollection<Touch> touches)
        {
            var trace = touches.Where(t=>t.FingerId == touches.First().FingerId);
            long startTime = trace.First().Time;
            List<double> timePoints = trace.Select(t => (double)(t.Time - startTime)).ToList();
            List<double> xValues = trace.Select(t => (double)t.X).ToList();
            List<double> yValues = trace.Select(t => (double)t.Y).ToList();


            var X = new double[trace.Count()];
            var Y = new double[trace.Count()];
            Time = new int[trace.Count()];
            var ind = 0;
            foreach(var touch in trace)
            {
                X[ind] = (double)touch.X;
                Y[ind] = (double)touch.Y;
                Time[ind] = (int)(touch.Time - startTime);
                ind++;
            }

            var isNotDistinct = timePoints.GroupBy(n => n).Any(c => c.Count() > 1);
            if (isNotDistinct) return;

            //do interpolation stuff
            var splineX = new MathNet.Numerics.Interpolation.Algorithms.AkimaSplineInterpolation(timePoints, xValues);
            var splineY = new MathNet.Numerics.Interpolation.Algorithms.AkimaSplineInterpolation(timePoints, yValues);

            var fft = new MathNet.Numerics.IntegralTransforms.Algorithms.DiscreteFourierTransform();
            
            //fft.BluesteinForward()

            //var splineX = new MathNet.Numerics.Interpolation.Algorithms.CubicSplineInterpolation(timePoints, xValues);
            //var splineY = new MathNet.Numerics.Interpolation.Algorithms.CubicSplineInterpolation(timePoints, yValues);

            /*
            int nSteps = (int)timePoints.Last() / 10;

            var XInt = new double[nSteps];
            var YInt = new double[nSteps];
            var vX = new double[nSteps];
            var vY = new double[nSteps];
            TimeInterpolated = new int[nSteps];
            //var aX = new double[nSteps];
            //var aY = new double[nSteps];


            for (int i = 0; i < nSteps; i++)
            {
                XInt[i] = splineX.Interpolate(i * 10);
                YInt[i] = splineY.Interpolate(i * 10);
                vX[i] = splineX.Differentiate(i * 10);
                vY[i] = splineY.Differentiate(i * 10);
                TimeInterpolated[i] = (int)(i * 10);
            }
            */

            /*
            //init arrays
            int n = touches.Count;
            var dT = new int?[n];
            var X = new decimal[n]; 
            var Y = new decimal[n];
            var dX = new decimal?[n]; 
            var dY = new decimal?[n];
            var ddX = new decimal?[n]; 
            var ddY = new decimal?[n];
            var StartDistance = new decimal[n];
            var StartDirection = new decimal[n];
            var Direction = new decimal?[n];
            var Curvature = new decimal?[n];
            var CurvX = new decimal?[n];
            var CurvY = new decimal?[n];
            var DirX = new decimal?[n];
            var DirY = new decimal?[n];
            var vX = new decimal?[n];
            var vY = new decimal?[n];
            var aX = new decimal?[n];
            var aY = new decimal?[n];

            Time = new int[n];

            var startTime = touches.First().Time;

            int i = 0;
            foreach(var t in touches)
            {
                X[i] = t.X;
                Y[i] = t.Y;

                Time[i] = (int)(t.Time - startTime);

                dT[i] = (i == 0) ? null : (int?)(Time[i] - Time[i - 1]);

                dX[i] = (i == 0) ? null : (decimal?)(X[i - 1] - X[i]);
                dY[i] = (i == 0) ? null : (decimal?)(Y[i - 1] - Y[i]);

                ddX[i] = (i > 1) ? (decimal?)(dX[i-1] - dX[i]) : null;
                ddY[i] = (i > 1) ? (decimal?)(dY[i-1] - dY[i]) : null;

                var dir = (dX[i].HasValue) ? (decimal?)Math.Atan2((double)dY[i], (double)dX[i]) : null;
                var curv = (ddX[i].HasValue) ? (decimal?)((double)(dX[i] * ddY[i] - dY[i] * ddX[i]) / ((dX[i] == 0 && dY[i] == 0) ? 0 : Math.Pow((double)(dX[i] * dX[i] + dY[i] * dY[i]), 1.5d))) : null;

                var sdX = t.X - X[0];
                var sdY = t.Y - Y[0];
                var sdis = Math.Sqrt((double)(sdX * sdX + sdY * sdY));
                var sdir = Math.Atan2((double)sdY, (double)sdX);

                Direction[i] = dir;
                Curvature[i] = curv;
                StartDistance[i] = (decimal)sdis;
                StartDirection[i] = (decimal)sdir;
                

                vX[i] = (i == 0) ? null : (decimal?)(dX[i] / dT[i]);
                vY[i] = (i == 0) ? null : (decimal?)(dY[i] / dT[i]);

                aX[i] = (i < 2) ? null : (decimal?)(ddX[i] / dT[i]);
                aY[i] = (i < 2) ? null : (decimal?)(ddY[i] / dT[i]);

                i++;
            }
            */

            //create features and add to dictionary
            //var fX = new Feature("X", X);
            //var fY = new Feature("Y", Y);
            //var fXInt = new Feature("X interpolated", XInt);
            //var fYInt = new Feature("Y interpolated", YInt);
            //var fVx = new Feature("Velocity X", vX);
            //var fVy = new Feature("Velocity Y", vY);

            /*
            var fStartDistance = new Feature("StartDistance", StartDistance);
            var fStartDirection = new Feature("StartDirection", StartDirection);
            var fDirection = new Feature("Direction", Direction.SkipWhile(d=>!d.HasValue).Cast<decimal>().ToArray());
            var fCurvature = new Feature("Curvature", Curvature.SkipWhile(d => !d.HasValue).Cast<decimal>().ToArray());
            var fVx = new Feature("Velocity X", vX.SkipWhile(d => !d.HasValue).Cast<decimal>().ToArray());
            var fVy = new Feature("Velocity Y", vY.SkipWhile(d => !d.HasValue).Cast<decimal>().ToArray());
            var fAx = new Feature("Acceleration X", aX.SkipWhile(d => !d.HasValue).Cast<decimal>().ToArray());
            var fAy = new Feature("Acceleration Y", aY.SkipWhile(d => !d.HasValue).Cast<decimal>().ToArray());
            */

            this["X"] = new Feature("X", splineX);
            this["Y"] = new Feature("Y", splineY);

            //this[fX.Name] = fX;
            //this[fY.Name] = fY;
            //this[fXInt.Name] = fXInt;
            //this[fYInt.Name] = fYInt;
            //this[fVx.Name] = fVx;
            //this[fVy.Name] = fVy;
            /*
            this[fAx.Name] = fAx;
            this[fAy.Name] = fAy;
            this[fStartDistance.Name] = fStartDistance;
            this[fStartDirection.Name] = fStartDirection;
            this[fDirection.Name] = fDirection;
            this[fCurvature.Name] = fCurvature;
            */
        }

        //gleitende Mittelwerte berechnen
        public double[] smoothFeature(string feature, int o)
        {
            var data = this[feature].Data;
            var n = data.Length;
            var smoothedData = new double[n - o + 1];
            
            var firstAvg = data.Take(o).Average();
            smoothedData[0] = firstAvg;
            for (int i = o + 1; i <= n; i++)
                smoothedData[i - o] = smoothedData[i - o - 1] + data[i - 1] / o - data[i - o - 1] / o;

            return smoothedData;
        }

        /*
        public decimal[] smoothFeatureLWMA(string feature, int o)
        {
            var data = this[feature].Data;
            var n = data.Length;
            var smoothedData = new decimal[n - o + 1];

            var firstAvg = data.Take(o).Average();
            smoothedData[0] = firstAvg;
            for (int i = o + 1; i <= n; i++)
                smoothedData[i - o] = smoothedData[i - o - 1] + data[i - 1] / o - data[i - o - 1] / o;

            return smoothedData;
        }
        */

        public void fillSeries(Series s, string feature, bool smoothed)
        {
            if (feature == "Trace")
            {
                var fX = this["X"].Data;
                var fY = this["Y"].Data;

                for (int i = 0; i < fX.Length; i++)
                    s.Points.AddXY(fX[i], 1 - fY[i]);
            }
            else if (feature == "Trace interpolated")
            {
                var fX = this["X interpolated"].Data;
                var fY = this["Y interpolated"].Data;

                for (int i = 0; i < fX.Length; i++)
                    s.Points.AddXY(fX[i], 1 - fY[i]);
            }
            else if (feature.Contains("interpolated"))
            {
                var data = this[feature].Data;
                for (int i = 0; i < data.Length; i++)
                    s.Points.AddXY(TimeInterpolated[i], data[i]);
            }
            else
            {
                var data = (smoothed) ? smoothFeature(feature, 10) : this[feature].Data;
                var tOffset = Time.Length - data.Length;
                for (int i = 0; i < data.Length; i++)
                    s.Points.AddXY(Time[i + tOffset], data[i]);
            }
        }

    }

    //public abstract class FeatureGenerator
    //{

    //    /// <summary>
    //    /// keeps time (x-values) and differentiates the y-values
    //    /// </summary>
    //    /// <param name="values"></param>
    //    /// <returns></returns>
    //    protected static IEnumerable<Vector> differentiate(IEnumerable<Vector> values)
    //    {
    //        Vector prevV = values.FirstOrDefault();
    //        foreach (var curV in values.Skip(1))
    //        {
    //            var v = new Vector(curV.X, prevV.Y - curV.Y);
    //            yield return v;

    //            prevV = curV;
    //        }
    //    }

    //    protected static IEnumerable<Vector> differentiate(IEnumerable<Vector> values, int n)
    //    {
    //        var res = values;
    //        for (int i = 0; i < n; i++)
    //            res = differentiate(res);

    //        return res;
    //    }

    //    public string Name { get; protected set; }
    //    public abstract IEnumerable<Vector> getFeature(IEnumerable<Touch> touches);
    //    public IEnumerable<Vector> getFeature(IEnumerable<Touch> touches, int ableitungN)
    //    {
    //        return differentiate(getFeature(touches), ableitungN);
    //    }

    //    public override string ToString()
    //    {
    //        return Name;
    //    }

    //}

    //public class XFeatureGenerator: FeatureGenerator
    //{
    //    public XFeatureGenerator()
    //    {
    //        Name = "X";
    //    }
    //    public override IEnumerable<Vector> getFeature(IEnumerable<Touch> touches)
    //    {
    //        long startTime = touches.FirstOrDefault().Time;
    //        return touches.Select(t => new Vector((double)(t.Time - startTime), (double)t.X));
    //    }
    //}
    //public class YFeatureGenerator : FeatureGenerator
    //{
    //    public YFeatureGenerator()
    //    {
    //        Name = "Y";
    //    }
    //    public override IEnumerable<Vector> getFeature(IEnumerable<Touch> touches)
    //    {
    //        long startTime = touches.FirstOrDefault().Time;
    //        return touches.Select(t => new Vector((double)(t.Time - startTime), (double)t.Y));
    //    }
    //}
    //public class DistanceFeatureGenerator : FeatureGenerator
    //{
    //    public DistanceFeatureGenerator()
    //    {
    //        Name = "Distance";
    //    }

    //    public override IEnumerable<Vector> getFeature(IEnumerable<Touch> touches)
    //    {
    //        var first = touches.FirstOrDefault();
    //        var prevV = new Vector((double)first.X, (double)first.Y);
    //        var startTime = first.Time;
    //        var distance = 0d;

    //        foreach (var t in touches.Skip(1))
    //        {
    //            var v = new Vector((double)t.X, (double)t.Y);
    //            var dis = (prevV - v).Length;
    //            yield return new Vector((double)(t.Time - startTime),(distance += dis));

    //            prevV = v;
    //        }
    //    }

    //}

    //public class StartDistanceFeatureGenerator : FeatureGenerator
    //{
    //    public StartDistanceFeatureGenerator()
    //    {
    //        Name = "Start Distance";
    //    }

    //    public override IEnumerable<Vector> getFeature(IEnumerable<Touch> touches)
    //    {
    //        var first = touches.FirstOrDefault();
    //        var fVector = new Vector((double)first.X, (double)first.Y);
    //        var startTime = first.Time;

    //        foreach (var t in touches.Skip(1))
    //        {
    //            var v = new Vector((double)t.X, (double)t.Y);
    //            var dis = (fVector - v).Length;
    //            yield return new Vector((double)(t.Time - startTime), dis);
    //        }
    //    }

    //}

    //public class AngleFeatureGenerator : FeatureGenerator
    //{
    //    public AngleFeatureGenerator()
    //    {
    //        Name = "Angle Between";
    //    }

    //    public override IEnumerable<Vector> getFeature(IEnumerable<Touch> touches)
    //    {
    //        var first = touches.FirstOrDefault();
    //        var second = touches.Skip(1).FirstOrDefault();
    //        var prevV = new Vector((double)first.X, (double)first.Y);
    //        var prevDif = prevV - new Vector((double)second.X, (double)second.Y);
    //        var startTime = first.Time;

    //        foreach (var t in touches.Skip(2))
    //        {
    //            var v = new Vector((double)t.X, (double)t.Y);
    //            var dif = (prevV - v);
    //            yield return new Vector((double)(t.Time - startTime), Vector.AngleBetween(prevDif,dif));

    //            prevDif = dif;
    //            prevV = v;
    //        }
    //    }
    //}

    //public class DirectionFeatureGenerator : FeatureGenerator
    //{
    //    public DirectionFeatureGenerator()
    //    {
    //        Name = "Direction";
    //    }

    //    public override IEnumerable<Vector> getFeature(IEnumerable<Touch> touches)
    //    {
    //        var first = touches.FirstOrDefault();
    //        var prevV = new Vector((double)first.X, (double)first.Y);
    //        var startTime = first.Time;

    //        foreach (var t in touches.Skip(1))
    //        {
    //            var v = new Vector((double)t.X, (double)t.Y);
    //            var dif = (prevV - v);
    //            yield return new Vector((double)(t.Time - startTime), Math.Atan2(dif.Y, dif.X));

    //            prevV = v;
    //        }
    //    }
    //}

    ///////////////////////////////////////////////////////
    //////////////////////////////////////////////////////
    /////////////////////////////////////////////////////

    /*
    public class VelocityFeatureGenerator : FeatureGenerator
    {

        public VelocityFeatureGenerator()
        {
            Name = "Velocity";
        }

        public override IEnumerable<double> getFeature(IEnumerable<Touch> touches)
        {
            var distance = new DistanceFeatureGenerator().getFeature(touches);
            return Differentiator.differentiate(distance);
        }

        public override IEnumerable<double> getTimes(IEnumerable<Touch> touches)
        {
            var start = touches.FirstOrDefault().Time;
            return touches.Select(t => (double)(t.Time - start)).Skip(1);
        }
    }

    public class AccelerationFeatureGenerator : FeatureGenerator
    {

        public AccelerationFeatureGenerator()
        {
            Name = "Acceleration";
        }

        public override IEnumerable<double> getFeature(IEnumerable<Touch> touches)
        {
            var velos = new VelocityFeatureGenerator().getFeature(touches);
            return Differentiator.differentiate(velos);
        }

        public override IEnumerable<double> getTimes(IEnumerable<Touch> touches)
        {
            var start = touches.FirstOrDefault().Time;
            return touches.Select(t => (double)(t.Time - start)).Skip(1);
        }
    }


    public class StartDistanceFeatureGenerator: FeatureGenerator
    {
        public StartDistanceFeatureGenerator()
        {
            Name = "DistanceToStart";
        }

        public override IEnumerable<double> getFeature(IEnumerable<Touch> touches)
        {
            var first = touches.FirstOrDefault();
            var fx = first.X;
            var fy = first.Y;

            return touches.Select(t => calcLength(fx,fy,t.X,t.Y));
        }

        public override IEnumerable<double> getTimes(IEnumerable<Touch> touches)
        {
            var times = touches.Select(t => t.Time);
            var start = times.FirstOrDefault();

            return times.Select(t => (double)(t - start));
        }
    }


    public class AngleFeatureGenerator : FeatureGenerator
    {
        public AngleFeatureGenerator()
        {
            Name = "Angle";
        }

        public override IEnumerable<double> getFeature(IEnumerable<Touch> touches)
        {
            var first = touches.FirstOrDefault();
            var fx = first.X;
            var fy = first.Y;
            return touches.Select(t => calcLength(fx, fy, t.X, t.Y));
        }

        public override IEnumerable<double> getTimes(IEnumerable<Touch> touches)
        {
            var times = touches.Select(t => t.Time);
            var start = times.FirstOrDefault();

            return times.Select(t => (double)(t - start));
        }
    }
    */

            //    var series = new Series();

            //int i = 0;
            //LfS.GestureDatabase.Touch prev = null;
            //long startTime = 0;
            //Vector prevDif = new Vector(0, 0); ;

            //foreach (var p in points)
            //{
            //    if (i++ == 0)
            //    {
            //        series.Points.AddXY(0, 0);
            //        prev = p;
            //        startTime = p.Time;
            //        continue;
            //    }
            //    else
            //    {
            //        var dif = new Vector((double)(p.X - prev.X), (double)(p.Y - prev.Y));
            //        var v = dif.Length;

            //        if (i > 2)
            //        {
            //            var angle = Vector.AngleBetween(prevDif, dif);
            //            var curv = angle / 360; // /v;
            //            series.Points.AddXY(prev.Time - startTime, curv);
            //        }

            //        prevDif = dif;
            //        prev = p;
            //    }
            //}

}
