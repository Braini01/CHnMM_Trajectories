using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib.CHnMM
{
    public abstract class OutputProbabilityCreator
    {
        public abstract Dictionary<string, double> createOutputProbabilities();

    }

    public class BasicAreaSymbols : OutputProbabilityCreator
    {
        private string areaName;
        private double hitProb;

        public BasicAreaSymbols(string area, double hitProb)
        {
            if (hitProb < 0 || hitProb > 1.0) throw new ArgumentOutOfRangeException();
            this.areaName = area;
            this.hitProb = hitProb;
        }

        public override Dictionary<string, double> createOutputProbabilities()
        {
            var probs = new Dictionary<string, double>(10);

            probs.Add(areaName + "_Hit", hitProb);
            probs.Add(areaName + "_Tolerance", 1 - hitProb);

            return probs;
        }
    }
}
