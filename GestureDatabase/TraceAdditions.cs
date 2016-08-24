using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.Interpolation.Algorithms;
using MathNet.Numerics;
using System.Numerics;

namespace LfS.GestureDatabase
{
    partial class Trace
    {
        public class Stroke
        {
            private Touch[] points;
            private AkimaSplineInterpolation splineX;
            private AkimaSplineInterpolation splineY;

            private double[] tfX;
            private double[] tfY;
            private double[] tfDistance;
            private double[] tfStartDistance;
            private double[] tfDirection;
            private double[] tfDirectionSin;
            private double[] tfDirectionCos;
            private double[] tfVelocityX;
            private double[] tfVelocityY;
            private double[] tfVelocity;
            private double[] tfBBDiagonalLength; //length of boundingbox diagonal
            private double[] tfBBDiagonalAngle; //angle of boundingbox diagonal
            private double[,] tfBoundingbox;
            

            public long FingerID { get; private set; }

            public int Duration { get; private set; }
            public double[] EquidistantTime { get; private set; }

            public double EquidistantTimeStep{ get; private set; }

            public int NumberOfPoints { get{return points.Length;} }

            public bool HasDistinctTimePoints { get; private set; }

            public Stroke(IEnumerable<Touch> points, long fingerID)
            {
                this.points = points.ToArray();
                HasDistinctTimePoints = !(Time.GroupBy(t => t).Any(c => c.Count() > 1));
                //if (HasDistinctTimePoints) throw new NotImplementedException();//ToDo: remove the broken point

                FingerID = fingerID;
                Duration = (int)(points.Last().Time - points.First().Time);
                EquidistantTimeStep = (double)Duration / NumberOfPoints;

                double time = 0;
                EquidistantTime = new double[NumberOfPoints];
                for (int i = 0; i < NumberOfPoints; i++)
                {
                    EquidistantTime[i] = time;
                    time += EquidistantTimeStep;
                }
            }

            public IEnumerable<decimal> X
            {
                get
                {
                    return points.Select(p => p.X);
                }
            }
            public IEnumerable<decimal> Y
            {
                get
                {
                    return points.Select(p => p.Y);
                }
            }

            public IEnumerable<int> Time
            {
                get
                {
                    var startTime = points.First().Time;
                    return points.Select(p => (int)(p.Time - startTime));
                }
            }
            public IEnumerable<double> Frequency
            {
                get
                {
                    double ndelta = NumberOfPoints * Duration;


                    yield return 0;

                    for (int i = 1; i < NumberOfPoints-1; i++)
                    {
                        if(i < NumberOfPoints/2) yield return i/(ndelta);
                        else if(i == NumberOfPoints/2) yield return 1/(2*Duration);
                        else yield return -(NumberOfPoints-i) / (ndelta);
                    }

                }
            }

            public AkimaSplineInterpolation SplineX
            {
                get
                {
                    if (!HasDistinctTimePoints) return null; //ToDo: add wrong timeStep detection?
                    if (splineX == null)
                        splineX = new AkimaSplineInterpolation(Time.Select(t => (double)t).ToArray(), X.Select(t => (double)t).ToArray());

                    return splineX;
                }
            }
            public AkimaSplineInterpolation SplineY
            {
                get
                {
                    if (!HasDistinctTimePoints) return null;
                    if (splineY == null)
                        splineY = new AkimaSplineInterpolation(Time.Select(t => (double)t).ToArray(), Y.Select(t => (double)t).ToArray());

                    return splineY;
                }
            }
            
            public IEnumerable<Complex> FrequencyX
            {
                get
                {
                    var fft = new MathNet.Numerics.IntegralTransforms.Algorithms.DiscreteFourierTransform();

                    var Xsamples = FeatureX.Select(v => new Complex(v, 0)).ToArray();

                    fft.BluesteinForward(Xsamples, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);


                    return Xsamples;
                }
            }
            public IEnumerable<Complex> FrequencyY
            {
                get
                {
                    var fft = new MathNet.Numerics.IntegralTransforms.Algorithms.DiscreteFourierTransform();

                    var Ysamples = FeatureY.Select(v => new Complex(v, 0)).ToArray();

                    fft.BluesteinForward(Ysamples, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);


                    return Ysamples;
                }
            }


            public double[] FeatureX
            {
                get
                {
                    if (tfX == null)
                        tfX = EquidistantTime.Select(t => SplineX.Interpolate(t)).ToArray();
                    return tfX;
                }
            }
            public double[] FeatureY
            {
                get
                {
                    if (tfY == null)
                        tfY = EquidistantTime.Select(t => SplineY.Interpolate(t)).ToArray();
                    return tfY;
                }
            }
            public double[] FeatureDistance
            {
                get
                {
                    if (tfDistance == null)
                    {
                        tfDistance = new double[NumberOfPoints];
                        tfDistance[0] = 0;

                        var positions = FeatureX.Zip(FeatureY, (x, y) => new { X = x, Y = y });
                        double distance = 0;
                        var prevPos = positions.First();
                        int i = 1;
                        foreach (var pos in positions.Skip(1))
                        {
                            var difX = pos.X - prevPos.X;
                            var difY = pos.Y - prevPos.Y;
                            distance += Math.Sqrt(difX * difX + difY * difY);
                            prevPos = pos;

                            tfDistance[i++] = distance;
                        }
                    }
                    return tfDistance;
                }
            }
            public double[] FeatureStartDistance
            {
                get
                {
                    if (tfStartDistance == null)
                    {
                        var positions = FeatureX.Zip(FeatureY, (x, y) => new { X = x, Y = y });
                        var startPos = positions.First();
                        tfStartDistance = positions.Select(pos =>
                            {
                                var difX = pos.X - startPos.X;
                                var difY = pos.Y - startPos.Y;
                                return Math.Sqrt(difX * difX + difY * difY);
                            }
                        ).ToArray();
                    }
                    return tfStartDistance;
                }
            }
            public double[] FeatureDirection
            {
                get
                {
                    if (tfDirection == null)
                        tfDirection = EquidistantTime.Select(t => Math.Atan2(SplineY.Differentiate(t), SplineX.Differentiate(t))).ToArray();

                    return tfDirection;
                }
            }
            public double[] FeatureSinDirection
            {
                get
                {
                    if (tfDirectionSin == null)
                        tfDirectionSin = FeatureDirection.Select(d => Math.Sin(d)).ToArray();

                    return tfDirectionSin;
                }
            }
            public double[] FeatureCosDirection
            {
                get
                {
                    if (tfDirectionCos == null)
                        tfDirectionCos = FeatureDirection.Select(d => Math.Cos(d)).ToArray();

                    return tfDirectionCos;
                }
            }
            public double[] FeatureVelocityX
            {
                get
                {
                    if (tfVelocityX == null)
                        tfVelocityX = EquidistantTime.Select(t => SplineX.Differentiate(t)).ToArray();

                    return tfVelocityX;
                }
            }
            public double[] FeatureVelocityY
            {
                get
                {
                    if (tfVelocityY == null)
                        tfVelocityY = EquidistantTime.Select(t => SplineY.Differentiate(t)).ToArray();

                    return tfVelocityY;
                }
            }
            public double[] FeatureVelocity
            {
                get
                {
                    if (tfVelocity == null)
                        tfVelocity = FeatureVelocityX.Zip(FeatureVelocityY, (x, y) => Math.Sqrt(x * x + y * y)).ToArray();

                    return tfVelocity;
                }
            }
            public double[,] FeatureBoundingBox
            {
                get
                {
                    if (tfBoundingbox == null)
                    {
                        tfBoundingbox = new double[NumberOfPoints,4];

                        var maxX = -1d;
                        var maxY = -1d;
                        var minX = 2d;
                        var minY = 2d;
                        for(int i=0; i<NumberOfPoints; i++)
                        {
                            if (minX > FeatureX[i]) minX = FeatureX[i];
                            if (maxX < FeatureX[i]) maxX = FeatureX[i];
                            if (minY > FeatureY[i]) minY = FeatureY[i];
                            if (maxY < FeatureY[i]) maxY = FeatureY[i];

                            tfBoundingbox[i, 0] = minX;
                            tfBoundingbox[i, 1] = minY;
                            tfBoundingbox[i, 2] = maxX;
                            tfBoundingbox[i, 3] = maxY;
                        }

                    }

                    return tfBoundingbox;
                }
            }
            public double[] FeatureBBDiagonalLength
            {
                get
                {
                    if (tfBBDiagonalLength == null)
                    {
                        tfBBDiagonalLength = new double[NumberOfPoints];
                        for(int i=0;i<NumberOfPoints;i++)
                        {
                            var xDif = FeatureBoundingBox[i, 2] - FeatureBoundingBox[i, 0];
                            var yDif = FeatureBoundingBox[i, 3] - FeatureBoundingBox[i, 1];
                            tfBBDiagonalLength[i] = Math.Sqrt(xDif * xDif + yDif * yDif);
                        }

                    }


                    return tfBBDiagonalLength;
                }
            }
            public double[] FeatureBBDiagonalAngle
            {
                get
                {
                    if (tfBBDiagonalAngle == null)
                    {
                        tfBBDiagonalAngle = new double[NumberOfPoints];
                        for (int i = 0; i < NumberOfPoints; i++)
                        {
                            var xDif = FeatureBoundingBox[i, 2] - FeatureBoundingBox[i, 0];
                            var yDif = FeatureBoundingBox[i, 3] - FeatureBoundingBox[i, 1];
                            tfBBDiagonalAngle[i] = Math.Atan2(yDif, xDif);
                        }

                    }


                    return tfBBDiagonalAngle;
                }
            }

        }

        public Stroke LongestStroke
        {
            get
            {
                return Strokes.Aggregate((agg, next) => next.Duration > agg.Duration ? next : agg);
            }
        }

        public IEnumerable<Stroke> Strokes 
        {
            get
            {
                var strokeGroups = Touches.OrderBy(t => t.Time).GroupBy(t => t.FingerId);
                return strokeGroups.Select(g => new Stroke(g, g.Key));          
            }
        }
    }
}
