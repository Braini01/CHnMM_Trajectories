using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LfS.ModelLib.Common.Distributions;

namespace GestureRecognitionLib.CHnMM.Estimators
{
    public abstract class DistributionEstimator
    {
        public abstract void addData(int dT);
        public abstract IDistribution createDistribution();
    }

    public class DeterministicDistCreator : DistributionEstimator
    {
        private int duration;

        public DeterministicDistCreator(int duration = 0)
        {
            this.duration = duration;
        }

        public override void addData(int dT) { }

        public override IDistribution createDistribution()
        {
            return new DeterministicDistribution(duration);
        }
    }

    public class NaiveUniformEstimator : DistributionEstimator
    {
        protected int n;
        protected int min;
        protected int max;
        protected int toleranceTime; //in milliseconds

        public NaiveUniformEstimator(int tolerance = 100)
        {
            toleranceTime = tolerance;
            min = int.MaxValue;
            max = int.MinValue;
        }

        public override void addData(int dT)
        {
            if (dT < min) min = dT;
            if (dT > max) max = dT;
            n++;
        }

        public override IDistribution createDistribution()
        {
            if (n <= 0) throw new ArgumentOutOfRangeException("No data available for distribution");
            if (min < 0 || max < 0 || min > max) throw new ArgumentException("Unplausible min, max values");

            var half = toleranceTime / 2;
            var estMin = Math.Max(0, min - half);
            var estMax = max + half;

            //naive   
            return new UniformDistribution(estMin, estMax);
        }
    }

    public class AdaptiveUniformEstimator : NaiveUniformEstimator
    {
        public override IDistribution createDistribution()
        {
            if (n <= 0) throw new ArgumentOutOfRangeException("No data available for distribution");
            if (min < 0 || max < 0 || min > max) throw new ArgumentException("Unplausible min, max values");

            var f = ((n + 1) / n); //converges from 2 to 1
            var tolf = toleranceTime / 4 * f;
            //minimum variance
            var estMin = Math.Max(0, min - tolf);
            var estMax = ((n + 1) / n) * max;
            return new UniformDistribution(min, max);
        }
    }

    public class MinVarianceUniformEstimator : NaiveUniformEstimator
    {
        public override IDistribution createDistribution()
        {
            if (n <= 0) throw new ArgumentOutOfRangeException("No data available for distribution");
            if (min < 0 || max < 0 || min > max) throw new ArgumentException("Unplausible min, max values");

            var estMax = ((n + 1) / n) * max;
            var dif = estMax - max;
            var estMin = Math.Max(0, min - dif);
            return new UniformDistribution(min, max);
        }
    }

    public class NormalEstimator : DistributionEstimator
    {
        protected int n;
        protected int sum;
        protected int sum2;

        public NormalEstimator()
        {
        }

        public override void addData(int dT)
        {
            sum += dT;
            sum2 += dT * dT;
            n++;
        }

        public override IDistribution createDistribution()
        {
            if (n <= 0) throw new ArgumentOutOfRangeException("No data available for distribution");
            if (sum < 0 || sum2 < 0) throw new ArgumentException("Unplausible sum values");

            //ToDo: think about this edge-case and its solutions
            if (n == 1)
            {
                return new NormalDistribution(sum, 50); //50ms standardabweichung vorerst; parameter daraus machen?
            }
            
            var estMean = sum / n;
            var estVariance = (n * sum2 - sum * sum) / (n * (n - 1));

            return new NormalDistribution(estMean, (int)Math.Sqrt(estVariance));
        }
    }

    //TrapezEstimator
}
