using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using LfS.ModelLib;
using LfS.GestureDatabase;

namespace LfS.GestureRecognitionTests
{
    public class ProgramCrossValidate
    {
        static void Main(string[] args)
        {
            //Shaik_Circle = 54
            //Shaik_Square = 55
            /*
            var tracesShaikCircle = SymbolGenerator.getTraces(54,7);
            var tracesShaikSquare = SymbolGenerator.getTraces(55, 7);
            var tracesUserCircle = SymbolGenerator.getTraces(134, 7);

            Model model = ModelCreator.createGridModel(tracesShaikCircle);

            model.saveToFile("C:\\test.model");

            Model reconstructedModel = Model.readFromFile("C:\\test.model");

            foreach (var t in tracesShaikCircle)
            {
                double res = model.evaluate(t);
                Console.WriteLine(res);
            }
            */

            //var fg = new XFeatureGenerator();
            //var generator = new ExtremaSymbolGenerator();
            //var creator = new SimpleModelCreator();

            //var squareTraces = readUserSymbolTraces("OwnForm_1Finger", generator);

            //var model = creator.createModel(squareTraces.First().Value);


            //Console.WriteLine(model.evaluate(squareTraces.First().Value.First()));


            int nSubsets = 5;
            //var generator = new DirectionSymbolGenerator();
            var creator = new SimpleMultiplePathsModelCreator();
            var circleTraces = readUserSymbolTraces("Circle_1Finger");
            var squareTraces = readUserSymbolTraces("Square_1Finger");
            var ownFormTraces = readUserSymbolTraces("OwnForm_1Finger");

            var crossValidator = new CrossValidator();

            ////saveSymbolTracesToDatabase();
            var resCircle = crossValidator.validate(circleTraces, "Circle_1Finger", nSubsets);
            var resSquare = crossValidator.validate(squareTraces, "Square_1Finger", nSubsets);
            var resOwnForm = crossValidator.validate(ownFormTraces, "OwnForm_1Finger", nSubsets);

            var resAll = resCircle.Concat(resSquare).Concat(resOwnForm);
            saveResultsToFile(@"F:\Dropbox\LfS\Code\GestureRecognitionTests\crossValidation_Extrema.csv", resAll);//resAll);
            

            ////DBtoExcel(@"D:\Dropbox\LfS\Data\Multitouch_Userstudy\Traces.csv");
        }

        /*
        private struct UserData
        {
            public User user;
            public IEnumerable<GestureDatabase.Trace> trainTraces;
            public IEnumerable<GestureDatabase.Trace> testTraces;
            public Model model;
        }*/

        /*
        public static void saveSymbolTracesToDatabase()
        {
            int gridSize = 7;
            SymbolGenerator sg = new GridCellSymbolGenerator(gridSize);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (dbEntities ctx = new dbEntities())
            {
                var gestures = ctx.Gestures.Where(g => g.Name.Equals("OwnForm_1Finger"));
                foreach (var g in gestures)
                {
                    var traces = g.TrainingTraces;
                    
                    foreach (var trace in traces)
                    {
                        var symbolTrace = sg.generateSymbolTrace(trace.Touches);

                        foreach (var symbol in symbolTrace)
                        {
                            var sym = new Symbol();
                            sym.Name = symbol.Symbol;
                            sym.Time = symbol.Time;
                            sym.Trace = trace;
                            ctx.Symbols.Add(sym);
                        }
                        ctx.SaveChanges();
                    }
                    Console.WriteLine("Converted all traces of current gesture in " + sw.Elapsed);
                    sw.Restart();
                }

                //ctx.SaveChanges();
            }
        }
        */

        public static Dictionary<string, ICollection<ICollection<Touch>>> readUserSymbolTraces(string gesture)
        {
            var userSymbolTraces = new Dictionary<string, ICollection<ICollection<Touch>>>(20);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (dbEntities ctx = new dbEntities())
            {
                //ctx.Configuration.AutoDetectChangesEnabled = false;
                var gestures = ctx.Gestures.Include("TrainingTraces.Touches").Where(g => g.Name == gesture);

                ICollection<ICollection<Touch>> gestureTraces;
                foreach (var g in gestures)
                {
                    sw.Restart();
                    gestureTraces = new LinkedList<ICollection<Touch>>();
                    foreach (var trace in g.TrainingTraces)
                        gestureTraces.Add(trace.Touches);

                    userSymbolTraces[g.User.Username] = gestureTraces;
                    Console.WriteLine("Needed time: " + sw.Elapsed);
                }
            }

            return userSymbolTraces;
        }

        public static Dictionary<string, ICollection<ICollection<Observation>>> readUserSymbolTraces(string gesture, SymbolGenerator generator)
        {
            var userSymbolTraces = new Dictionary<string, ICollection<ICollection<Observation>>>(20);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (dbEntities ctx = new dbEntities())
            {
                ctx.Configuration.AutoDetectChangesEnabled = false;
                var gestures = ctx.Gestures.Where(g => g.Name == gesture);

                ICollection<ICollection<Observation>> symbolTraces;
                foreach (var g in gestures)
                {
                    sw.Restart();
                    symbolTraces = new LinkedList<ICollection<Observation>>();
                    LinkedList<Observation> symbolTrace;
                    foreach (var trace in g.TrainingTraces)
                    {
                        symbolTrace = generator.generateSymbolTrace(trace.Touches);
                        symbolTraces.Add(symbolTrace);
                    }

                    userSymbolTraces[g.User.Username] = symbolTraces;
                    Console.WriteLine("Needed time: " + sw.Elapsed);
                }
            }

            return userSymbolTraces;
        }

        /*
        public static LinkedList<CrossValidator.ResultRow> crossValidate(int nSubsets, string gesture)
        {
            var userTraces = new Dictionary<string, ICollection<ICollection<Observation>>>(20);
            
            using (dbEntities ctx = new dbEntities())
            {
                //get all Circle_1Finger Traces
                var gestures = ctx.Gestures.Where( g => g.Name == gesture );

                ICollection<ICollection<Observation>> symbolTraces;
                foreach (var g in gestures)
                {
                    symbolTraces = new LinkedList<ICollection<Observation>>();
                    ICollection<Observation> symbolTrace;
                    foreach (var trace in g.TrainingTraces)
                    {
                        symbolTrace = new LinkedList<Observation>();
                        foreach (var symbol in trace.Symbols)
                            symbolTrace.Add(new Observation(symbol.Name, symbol.Time));

                        symbolTraces.Add(symbolTrace);
                    }

                    userTraces[g.User.Username] = symbolTraces;
                }
            }

            var crossValidator = new CrossValidator(userTraces);

            return crossValidator.validate(gesture, nSubsets);
        }
        */ 

        private static void saveResultsToFile(string file, IEnumerable<CrossValidator.ResultRow> results)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine("Gesture;User;Subset;TP;TN;FN;FP;FRR;FAR");
            foreach(var result in results)
            {
                string line = result.gesture + ";" + result.user + ";" + result.subset + ";" + result.TP + ";" + result.TN + ";" + result.FN + ";" + result.FP + ";" + result.FRR + ";" + result.FAR;
                sw.WriteLine(line);
            }

            sw.Close();
        }

        /*
        private static void DBtoExcel(string file)
        {
            var stream = File.OpenWrite(file);
            TextWriter tw = new StreamWriter(stream);

            tw.WriteLine("Gesture;User;Trace;Time;X;Y");

            using (dbEntities ctx = new dbEntities())
            {
                var query = from t in ctx.Touches select new {Gesture = t.Trace.Gesture.Name, User = t.Trace.Gesture.User.Username, Trace = t.Trace.Id, Time = t.Time, X = t.X, Y = t.Y };

                foreach (var e in query)
                {
                    string line = e.Gesture + ";" + e.User + ";" + e.Trace + ";" + e.Time + ";" + e.X + ";" + e.Y;
                    tw.WriteLine(line);
                }
            }

            tw.Close();
        }
        */

        //public static void testCircle1Gesture()
        //{
        //    int gridSize = 7;

        //    LinkedList<UserData> userData = new LinkedList<UserData>();
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    using(dbEntities ctx = new dbEntities())
        //    {
        //        //get all Circle_1Finger Traces
        //        var gestures = ctx.Gestures.Where(g => g.Name.Equals("Circle_1Finger"));
        //        int nTrain = 15;
        //        foreach (var g in gestures)
        //        {
        //            var traces = g.TrainingTraces;
        //            var train = traces.Take(nTrain);
        //            var test = traces.Skip(nTrain);
        //            var data = new UserData();
        //            data.user = g.User;
        //            data.trainTraces = train;
        //            data.testTraces = test;

        //            userData.AddLast(data);
        //            Console.WriteLine("elapsed time: " + sw.Elapsed);
        //        }


        //        //collect all data
        //        /*
        //        foreach (var user in ctx.Users)
        //        {
        //            Console.WriteLine("elapsed time: " + sw.Elapsed);
        //            var gesture = user.Gestures.FirstOrDefault(g => g.Name.Equals("Circle_1Finger"));
        //            Console.WriteLine("elapsed time: " + sw.Elapsed);
        //            var traces = SymbolGenerator.getTraces((int)gesture.Id, gridSize);
        //            Console.WriteLine("elapsed time: " + sw.Elapsed);
        //            int nTrain = 15;
        //            var train = traces.Take(nTrain);
        //            var test = traces.Skip(nTrain);

        //            var data = new UserData();
        //            data.user = user;
        //            data.trainTraces = train;
        //            data.testTraces = test;

        //            userData.AddLast(data);
        //        }
        //        */
        //    }

        //    sw.Stop();
        //    Console.WriteLine("Time needed for reading and creating of Traces: " + sw.Elapsed);
        //    LinkedList<UserData> newUserData = new LinkedList<UserData>();

        //    //generate models
        //    foreach (var user in userData)
        //    {
        //        Model model = null;

        //        var fname = "Circle_1Finger_" + user.user.Username + ".chnmm";
        //        if (!File.Exists(fname))
        //        {
        //            model = ModelCreator.createGridModel(user.trainTraces);
        //            model.saveToFile("Circle_1Finger_" + user.user.Username + ".chnmm");
        //        }
        //        else
        //        {
        //            model = Model.readFromFile(fname);
        //        }

        //        UserData newUser = new UserData();
        //        newUser.user = user.user;
        //        newUser.trainTraces = user.trainTraces;
        //        newUser.testTraces = user.testTraces;
        //        newUser.model = model;

        //        newUserData.AddLast(newUser);
        //    }

            
        //    //test against each other
        //    foreach (var user in newUserData)
        //    {
        //        Console.WriteLine("Testing User " + user.user.Name);
        //        //calc False Rejection Rate (FRR)
        //        int nAccepted = 0;
        //        int nRejected = 0;

        //        foreach (var trace in user.trainTraces)
        //        {
        //            double res = user.model.evaluate(trace);
        //            if (res > 0) nAccepted++; else nRejected++; 
        //            //Console.WriteLine(res);
        //        }

        //        foreach (var trace in user.testTraces)
        //        {
        //            double res = user.model.evaluate(trace);
        //            if (res > 0) nAccepted++; else nRejected++; 
        //            //Console.WriteLine(res);
        //        }

        //        Console.WriteLine("FRR: " + ((double)nRejected / (nAccepted + nRejected)));


        //        foreach (var user2 in newUserData)
        //        {
        //            if (user2.Equals(user)) continue;
        //            nAccepted = 0;
        //            nRejected = 0;

        //            foreach (var trace in user2.trainTraces)
        //            {
        //                double res = user.model.evaluate(trace);
        //                if (res > 0) nAccepted++; else nRejected++;
        //            }

        //            foreach (var trace in user2.testTraces)
        //            {
        //                double res = user.model.evaluate(trace);
        //                if (res > 0) nAccepted++; else nRejected++;
        //            }
        //        }

        //        Console.WriteLine("FAR: " + ((double)nAccepted / (nAccepted + nRejected)) + "\n");

        //    }
        //}

    }
}
