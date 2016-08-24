using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LfS.GestureDatabase;
using LfS.ModelLib.Models;

namespace LfS.ModelLib
{
    

    public abstract class SymbolGenerator
    {
        public abstract LinkedList<Observation> generateSymbolTrace(ICollection<Touch> touchs);

        public LinkedList<LinkedList<Observation>> generateSymbolTrace(IEnumerable<ICollection<Touch>> touchs)
        {
            var traces = new LinkedList<LinkedList<Observation>>();

            foreach (var touchTrace in touchs)
                traces.AddLast(generateSymbolTrace(touchTrace));

            return traces;
        }
    }

    /*

    public class GridCellSymbolGenerator: SymbolGenerator
    {
        private int gridSize = 7;

        public GridCellSymbolGenerator() {}
        public GridCellSymbolGenerator(int gridSize)
        {
            this.gridSize = gridSize;
        }

        public override LinkedList<Observation> generateSymbolTrace(IEnumerable<Touch> touches)
        {
            LinkedList<Observation> observations = new LinkedList<Observation>();

            //finger -> cell
            var prevCells = new Dictionary<long, int>(10);

            foreach (var t in touches)
            {
                int cell = getCell(t.X, t.Y);
                int prevCell;
                if (!prevCells.TryGetValue(t.FingerId, out prevCell)) prevCell = -1;
      
                if (cell != prevCell)
                {
                    observations.AddLast(new Observation(cell.ToString(), t.Time));
                    prevCells[t.FingerId] = cell;
                }
            }

            return observations;
        }

        private int getCell(decimal x, decimal y)
        {
            decimal a = (decimal)1 / gridSize;
            int col = (int)Math.Floor(x / a);
            int row = (int)Math.Floor(y / a);
            if (col == gridSize) col--;
            if (row == gridSize) row--;
            return row * gridSize + col; 
        }
    }

    public class RelativeGridCellSymbolGenerator : SymbolGenerator
    {
        private int gridSize = 7;

        public RelativeGridCellSymbolGenerator() { }
        public RelativeGridCellSymbolGenerator(int gridSize)
        {
            this.gridSize = gridSize;
        }

        public override LinkedList<Observation> generateSymbolTrace(IEnumerable<Touch> touches)
        {
            LinkedList<Observation> observations = new LinkedList<Observation>();

            //finger -> cell
            var prevCells = new Dictionary<long, int>(10);

            foreach (var t in touches)
            {
                int cell = getCell(t.X, t.Y);
                int prevCell;
                if (!prevCells.TryGetValue(t.FingerId, out prevCell)) prevCell = -1;

                if (cell != prevCell)
                {

                    observations.AddLast(new Observation(cell.ToString(), t.Time));
                    prevCells[t.FingerId] = cell;
                }
            }

            return observations;
        }

        private int getCell(decimal x, decimal y)
        {
            decimal a = (decimal)1 / gridSize;
            int col = (int)Math.Floor(x / a);
            int row = (int)Math.Floor(y / a);
            if (col == gridSize) col--;
            if (row == gridSize) row--;
            return row * gridSize + col;
        }
    }


    public class DirectionSymbolGenerator : SymbolGenerator
    {
        private Dictionary<long, String> prevSymbols = new Dictionary<long, String>(20);
        //private const double threshold = 0.002;
        private string[] directions = {"U", "UL", "L", "DL", "D", "DR", "R", "UR"};

        public override LinkedList<Observation> generateSymbolTrace(IEnumerable<Touch> touchs)
        {
            var O = new LinkedList<Observation>();
            Touch prevTouch = null;

            foreach(var touch in touchs)
            {
                if(prevTouch == null)
                {
                    O.AddLast(new Observation("GestureStart", touch.Time));
                    prevTouch = touch;
                    continue;
                }

                var id = touch.FingerId;
                var dX = (double)(touch.X - prevTouch.X);
                var dY = (double)(touch.Y - prevTouch.Y);
                //var len = Math.Sqrt(dX * dX + dY * dY);

                double dir = Math.Atan2(dX, dY);
                int index = (int)(Math.Round((Math.PI + dir) / (Math.PI / 4)) % 8);
                var symbol = directions[index];

                string prevSymbol;
                if (!prevSymbols.TryGetValue(id, out prevSymbol) || prevSymbol != symbol)
                    O.AddLast(new Observation(symbol, touch.Time));

                prevSymbols[id] = symbol;
                prevTouch = touch;
            }

            //O.AddLast(new Observation("GestureEnd", prevTouch.Time));

            return O;
        }
    }

    public class SmoothDirectionSymbolGenerator : SymbolGenerator
    {
        private Dictionary<long, String> prevSymbols = new Dictionary<long, String>(20);
        //private const double threshold = 0.002;
        private string[] directions = { "U", "UL", "L", "DL", "D", "DR", "R", "UR" };

        public override LinkedList<Observation> generateSymbolTrace(IEnumerable<Touch> touchs)
        {
            var O = new LinkedList<Observation>();
            Touch prevTouch = null;

            foreach (var touch in touchs)
            {
                if (prevTouch == null)
                {
                    O.AddLast(new Observation("GestureStart", touch.Time));
                    prevTouch = touch;
                    continue;
                }

                var id = touch.FingerId;
                var dX = (double)(touch.X - prevTouch.X);
                var dY = (double)(touch.Y - prevTouch.Y);
                //var len = Math.Sqrt(dX * dX + dY * dY);

                double dir = Math.Atan2(dX, dY);
                int index = (int)(Math.Round((Math.PI + dir) / (Math.PI / 4)) % 8);
                var symbol = directions[index];

                string prevSymbol;
                if (!prevSymbols.TryGetValue(id, out prevSymbol) || prevSymbol != symbol)
                    O.AddLast(new Observation(symbol, touch.Time));

                prevSymbols[id] = symbol;
                prevTouch = touch;
            }

            //O.AddLast(new Observation("GestureEnd", prevTouch.Time));

            return O;
        }
    }

    public class VelocitySymbolGenerator : SymbolGenerator
    {
        private Dictionary<long, String> prevSymbols = new Dictionary<long, String>(20);
        //private const double threshold = 0.002;
        private string[] directions = { "U", "UL", "L", "DL", "D", "DR", "R", "UR" };

        public override LinkedList<Observation> generateSymbolTrace(IEnumerable<Touch> touchs)
        {
            var O = new LinkedList<Observation>();
            Touch prevTouch = null;

            foreach (var touch in touchs)
            {
                if (prevTouch == null)
                {
                    O.AddLast(new Observation("GestureStart", touch.Time));
                    prevTouch = touch;
                    continue;
                }

                var id = touch.FingerId;
                var dX = (double)(touch.X - prevTouch.X);
                var dY = (double)(touch.Y - prevTouch.Y);
                //var len = Math.Sqrt(dX * dX + dY * dY);

                double dir = Math.Atan2(dX, dY);
                int index = (int)(Math.Round((Math.PI + dir) / (Math.PI / 4)) % 8);
                var symbol = directions[index];

                string prevSymbol;
                if (!prevSymbols.TryGetValue(id, out prevSymbol) || prevSymbol != symbol)
                    O.AddLast(new Observation(symbol, touch.Time));

                prevSymbols[id] = symbol;
                prevTouch = touch;
            }

            //O.AddLast(new Observation("GestureEnd", prevTouch.Time));

            return O;
        }
    }
    */

    //public class FeatureSymbolGenerator : SymbolGenerator
    //{
    //    FeatureGenerator fg;

    //    public FeatureSymbolGenerator(FeatureGenerator fg)
    //    {
    //        this.fg = fg;
    //    }

    //    public override LinkedList<Observation> generateSymbolTrace(IEnumerable<Touch> touchs)
    //    {
    //        //var features = fg.getFeature(touchs);
    //        var firstDeriv = fg.getFeature(touchs, 1);
    //        var secondDeriv = fg.getFeature(touchs, 2);

    //        var O = new LinkedList<Observation>();

    //        //find extremas
    //        var prevV = firstDeriv.First();
    //        foreach (var v in firstDeriv.Skip(1))
    //        {
    //            //special case prevV == 0??

    //            if (prevV.Y > 0 && v.Y <= 0)
    //            {
    //                //maximum
    //                O.AddLast(new Observation("Maximum", (int)prevV.X));
    //            }
    //            else if (prevV.Y < 0 && v.Y >= 0)
    //            {
    //                //minimum
    //                O.AddLast(new Observation("Minimum", (int)prevV.X));
    //            }
    //        }
    //        return O;
    //    }
    //}

    public class ExtremaSymbolGenerator : SymbolGenerator
    {
        private string feature;
        private bool smooth;

        public ExtremaSymbolGenerator(string feature, bool smooth)
        {
            this.feature = feature;
            this.smooth = smooth;
        }
        public override LinkedList<Observation> generateSymbolTrace(ICollection<Touch> touchs)
        {
            TraceFeatures tf = new TraceFeatures(touchs);

            return generateSymbolTrace(tf);

        }

        public LinkedList<Observation> generateSymbolTrace(TraceFeatures tf)
        {
            throw new NotImplementedException();

            //5 ist Glättungsordnung
            //var data = (smooth) ? tf.smoothFeature(feature, 10) : tf[feature].Data;

            //var O = new LinkedList<Observation>();
            //O.AddLast(new Observation("GestureStart", 0));
            //for (int i = 2; i < data.Length - 2; i++)
            //{
            //    var v1 = data[i - 2];
            //    var v2 = data[i - 1];
            //    var v = data[i];
            //    var v3 = data[i + 1];
            //    var v4 = data[i + 2];
            //    if (v > v1 && v > v2 && v >= v3 && v >= v4) O.AddLast(new Observation("Maximum", tf.Time[i]));
            //    if (v < v1 && v < v2 && v <= v3 && v <= v4) O.AddLast(new Observation("Minimum", tf.Time[i]));
            //}

            //O.AddLast(new Observation("GestureEnd", tf.Time.Last()));
            //return O;

            /*
//find extremas
var prevV = firstDeriv.First();
foreach (var v in firstDeriv.Skip(1))
{
    //special case prevV == 0??

    if (prevV.Y > 0 && v.Y <= 0)
    {
        //maximum
        O.AddLast(new Observation("Maximum", (int)prevV.X));
    }
    else if (prevV.Y < 0 && v.Y >= 0)
    {
        //minimum
        O.AddLast(new Observation("Minimum", (int)prevV.X));
    }
}
return O;
 * 
 * */
        }

    }

    //public class RelativeXCoordsSymbolGenerator

    //public class DistanceToStartSymbolGenerator : SymbolGenerator
    //{
    //    public override LinkedList<Observation> generateSymbolTrace(IEnumerable<Touch> touchs)
    //    {
    //        var O = new LinkedList<Observation>();
    //        Touch startTouch = touchs.FirstOrDefault();
    //        if (startTouch != null) O.AddLast(new Observation("GestureStart", startTouch.Time));

    //        double prevDis = 

    //        foreach (var touch in touchs.Skip(1))
    //        {
    //            var id = touch.FingerId;
    //            var dX = (double)(touch.X - startTouch.X);
    //            var dY = (double)(touch.Y - startTouch.Y);
    //            var dis  = Math.Sqrt(dX * dX + dY * dY);

                
    //            int index = (int)(Math.Round((Math.PI + dir) / (Math.PI / 4)) % 8);
    //            var symbol = directions[index];

    //            string prevSymbol;
    //            if (!prevSymbols.TryGetValue(id, out prevSymbol) || prevSymbol != symbol)
    //                O.AddLast(new Observation(symbol, touch.Time));

    //            prevSymbols[id] = symbol;
    //            prevTouch = touch;
    //        }

    //        //O.AddLast(new Observation("GestureEnd", prevTouch.Time));

    //        return O;
    //    }
    //}
}
