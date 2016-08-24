using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDtw;

namespace GestureRecognitionLib.DTW
{
    public class TrajectoryModel
    {
        public string Name { get; }

        public Template[] Templates { get; }

        public TrajectoryModel(string name, IEnumerable<IEnumerable<TrajectoryPoint>> trainingTraces)
        {
            Name = name;
            Templates = trainingTraces.Select(t => new Template(t)).ToArray();
        }

        public double getBestCost(Template trace, bool withTime=false)
        {
            return Templates.Min(t => t.getDTW(trace, withTime).GetCost());
        } 
    }

    public class Template
    {
        public double[] Series_X { get; }
        public double[] Series_Y { get; }
        public double[] Series_Time;

        public Template(IEnumerable<TrajectoryPoint> stroke)
        {
            Series_X = stroke.Select(p => p.X).ToArray();
            Series_Y = stroke.Select(p => p.Y).ToArray();

            //normalisieren
            var start = stroke.First().Time;
            Series_Time = stroke.Select(p => (double)(p.Time - start)).ToArray();
        }

        public Dtw getDTW(Template t, bool withTime = false)
        {
            var x = new SeriesVariable(Series_X, t.Series_X, "X-Coord", new NDtw.Preprocessing.NormalizationPreprocessor());
            var y = new SeriesVariable(Series_Y, t.Series_Y, "Y-Coord", new NDtw.Preprocessing.NormalizationPreprocessor());
            var time = new SeriesVariable(Series_Time, t.Series_Time, "Timestamp", new NDtw.Preprocessing.NormalizationPreprocessor());

            return (withTime) ? 
                new Dtw(new SeriesVariable[] { x, y, time }, DistanceMeasure.Euclidean) : 
                new Dtw(new SeriesVariable[] { x, y }, DistanceMeasure.Euclidean);
        }
    }
}
