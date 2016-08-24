using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LfS.ModelLib.Models;

namespace GestureRecognitionLib.CHnMM
{

    /*
    public class Observation
    {
        public string Symbol;
        /// <summary>
        /// time in ms since start
        /// </summary>
        public int Time; 

        public Observation(string sym, int time)
        {
            Symbol = sym;
            Time = time;
        }
    }
    */
    class SymbolGenerator
    {
        private StrokeMap[] maps;

        public SymbolGenerator(StrokeMap[] maps  /* parameter ?*/)
        {
            this.maps = maps;
        }

        public LinkedList<Observation[]> createPossibleSymbolTraces(GestureTrace trace)
        {
            var stroke = trace.LongestStroke;
            var startPoint = stroke.TrajectoryPoints[0];

            var candidateAreas = maps.Select(m => m.Areas[0]);

            //does stroke start in a candidate area?
            var activeMaps = maps.Where(map => map.Areas[0].PointInArea(startPoint.X, startPoint.Y));
            

            return null;
        }

    }
}
