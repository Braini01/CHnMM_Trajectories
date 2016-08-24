using GestureRecognitionLib;
using LfS.GestureDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LfS.GestureRecognitionTests
{
    public static class Helper
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static GestureTrace TouchesToGestureTrace(ICollection<Touch> touches, long traceID)
        {
            var strokes = touches.GroupBy(t => t.FingerId, t => new TrajectoryPoint((double)t.X, (double)t.Y, t.Time))
                                 .Select(grp => new Stroke(grp.OrderBy(t => t.Time).ToArray(), grp.Key));
            return new GestureTrace(strokes.ToArray(), traceID);
        }

        public static GestureTrace[] getGestureTraces(string userName, string gestureName)
        {
            var traces = GestureDatabase.GestureDatabase.getGestureTraces(userName, gestureName);
            return traces.Select(t => TouchesToGestureTrace(t.Touches, t.Id)).ToArray();
        }

        public static Dictionary<string, GestureTrace[]> getGestureTraces(string gestureName)
        {
            var groups = GestureDatabase.GestureDatabase.getGestureTraces(gestureName);
            return groups.ToDictionary(grp => grp.Key + "_" + gestureName, traces => traces.Select(trace => TouchesToGestureTrace(trace.Touches, trace.Id)).ToArray());            
        }
    }
}
