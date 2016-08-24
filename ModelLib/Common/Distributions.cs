using System;

namespace LfS.ModelLib.Common.Distributions
{
    public interface IDistribution
    {
        //mu(x)
        double getHRF(int x);

        //F(x)
        double getCDF(int x);

        //f(x)
        double getPDF(int x);

        double? Parameter1 { get; }
        double? Parameter2 { get; }
    }


    /// <summary>
    /// this is basically a hack
    /// </summary>
    [Serializable]
    public class DeterministicDistribution : IDistribution
    {
        private int time;

        public double? Parameter1
        {
            get
            {
                return time;
            }
        }

        public double? Parameter2
        {
            get
            {
                return null;
            }
        }

        public DeterministicDistribution(int time)
        {
            this.time = time;
        }

        public double getHRF(int x)
        {
            return (x == time) ? 1 : 0;
        }

        public double getCDF(int x)
        {
            return (x <= time) ? 0 : 1;
        }

        public double getPDF(int x)
        {
            return (x == time) ? 1 : 0;
        }
    }

    [Serializable]
    public class UniformDistribution : IDistribution
    {
        private int min, max;

        public int Min { get { return min; } }

        public double? Parameter1
        {
            get
            {
                return min;
            }
        }

        public double? Parameter2
        {
            get
            {
                return max;
            }
        }

        public UniformDistribution(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public double getHRF(int x)
        {
            double y;

            if ((x >= min) && (x < max))
                y = 1.0 / (max - x);
            else
                y = 0.0;

            return (y);
        }

        public double getCDF(int x)
        {
            if (x <= min)
                return 0;
            if (x >= max)
                return 1;

            return (double)(x - min) / (max - min);
        }

        public double getPDF(int x)
        {
            if ((x >= min) && (x <= max))
                return 1d / (max - min);
            else return 0;
        }
    }

    [Serializable]
    public class ExponentialDistribution : IDistribution
    {
        private double lambda;

        public double? Parameter1
        {
            get
            {
                return lambda;
            }
        }

        public double? Parameter2
        {
            get
            {
                return null;
            }
        }

        public ExponentialDistribution(double E)
        {
            lambda = 1/E;
        }

        public double getHRF(int x)
        {
            return lambda;
        }

        public double getCDF(int x)
        {
            return (x <= 0) ? (0) : (lambda * System.Math.Exp(-lambda * x));
        }

        public double getPDF(int x)
        {
            return (x < 0) ? 0 : (1 - System.Math.Exp(-lambda * x));
        }
    }

    [Serializable]
    public class NormalDistribution : IDistribution
    {
        private int m;
        private int s;

        private static double Pi2 = System.Math.PI * 2;

        public int Mean { get { return m; } }

        public double? Parameter1
        {
            get
            {
                return m;
            }
        }

        public double? Parameter2
        {
            get
            {
                return s;
            }
        }

        public NormalDistribution(int m, int s)
        {
            this.m = m;
            this.s = s;
        }

        public double getHRF(int x)
        {
            double pdf = getPDF(x);
            double cdf = getCDF(x);
            double rate = (pdf / (1 - cdf));
            if (double.IsInfinity(rate)) System.Console.Error.WriteLine("NormalHRF is infinite");
            return rate;
        }

        public double getPDF(int x)
        {
            double z = (x - m) / (double)s;
            return (System.Math.Exp(-z * z / 2) / (System.Math.Sqrt(Pi2) * s));
        }

        public double getCDF(int x)
        {
            double z = (x - m) / (double)s;

            double tmp = 0.5 * gammacdf(z * z / 2, 0.5);

            if (z >= 0)
                return 0.5 + tmp;
            else
                return 0.5 - tmp;
        }

        private double gammacdf(double x, double a)
        {
            if (x <= 0)
                return 0;
            else
                if (x < a + 1)
                    return gammaSeries(x, a);
                else
                    return (1 - gammaCF(x, a));
        }

        private double gammaCF(double x, double a)
        {
            int n, maxit = 100;
            double eps = 0.0000003;
            double gln = logGamma(a), g = 0, gold = 0, a0 = 1, a1 = x, b0 = 0, b1 = 1, fac = 1;
            double an, ana, anf;

            for (n = 1; n <= maxit; n++)
            {
                an = 1.0 * n;
                ana = an - a;
                a0 = (a1 + a0 * ana) * fac;
                b0 = (b1 + b0 * ana) * fac;
                anf = an * fac;
                a1 = x * a0 + anf * a1;
                b1 = x * b0 + anf * b1;
                if (a1 != 0)
                {
                    fac = 1.0 / a1;
                    g = b1 * fac;
                    if (System.Math.Abs((g - gold) / g) < eps)
                        break;
                    gold = g;
                }
            }
            return (System.Math.Exp(-x + a * System.Math.Log(x) - gln) * g);
        }


        private double logGamma(double x)
        {
            double[] coef = { 76.18009173, -86.50532033, 24.01409822, -1.231739516, 0.00120858003, -0.00000536382 };
            double step = 2.50662827465, fpf = 5.5, t, tmp, ser;
            int i;

            t = x - 1;
            tmp = t + fpf;
            tmp = (t + 0.5) * System.Math.Log(tmp) - tmp;
            ser = 1;
            for (i = 1; i <= 6; i++)
            {
                t = t + 1;
                ser = ser + coef[i - 1] / t;
            }
            return (tmp + System.Math.Log(step * ser));
        }

        private double gammaSeries(double x, double a)
        {
            int n, maxit = 100;
            double eps = 0.0000003;
            double sum = 1.0 / a, ap = a, gln = logGamma(a), del = sum;

            for (n = 1; n <= maxit; n++)
            {
                ap++;
                del = del * x / ap;
                sum = sum + del;
                if (System.Math.Abs(del) < System.Math.Abs(sum) * eps) break;
            }
            return (sum * System.Math.Exp(-x + a * System.Math.Log(x) - gln));
        }
    }
}