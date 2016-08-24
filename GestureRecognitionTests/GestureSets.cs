using GestureRecognitionLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SignatureDatabase;
using MoreLinq;

namespace LfS.GestureRecognitionTests
{
    using TrajectoryDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;
    using GestureDataSet = Dictionary<string, GestureTraces>;

    public class GestureSet
    {
        public string Name { get; private set; }

        public GestureSet(string setName)
        {
            Name = setName;
        }
    }

    /// <summary>
    /// multiple gestures of one user
    /// </summary>
    public class UserGestureSet : GestureSet
    {
        public string UserName { get; private set; }
        public string[] GestureNames { get; private set; }

        public UserGestureSet(string setName, string user, string[] gestures) : base(setName)
        {
            UserName = user;
            GestureNames = gestures;
        }        
    }

    /// <summary>
    /// one gesture by several users
    /// </summary>
    public class MultipleUserGestureSet : GestureSet
    {
        public string[] UserNames { get; private set; }
        public string GestureName { get; private set; }

        public MultipleUserGestureSet(string setName, string[] users, string gestureName) : base(setName)
        {
            UserNames = users;
            GestureName = gestureName;
        }
    }

    public static class DataSets
    {
        public static string cachePath = "CachedGestureSets\\";

        private static Dictionary<string, GestureSet> gestureSets = new Dictionary<string, GestureSet>
        {
            { "TimSimple", new UserGestureSet("TimSimple", "Tim", new string[] { "Triangle_Slow", "Triangle_Fast", "Circle_Slow", "Circle_Fast", "D_Slow", "D_Fast" }) },
            { "TimAdvanced", new UserGestureSet("TimAdvanced", "Tim" , new string[] { "Triangle_Slow", "Triangle_Fast", "Circle_Slow", "Circle_Fast", "D_Slow", "D_Fast", "Circle_FastLeftSlowRight", "Circle_SlowLeftFastRight"/*, "Circle_1Stop_Bottom", "Circle_2Stop_LeftRight"*/ }) },
            { "TimHard", new UserGestureSet("TimHard", "Tim", new string[] { "Triangle_Slow", "Triangle_Normal", "Triangle_Fast", "Circle_Slow", "Circle_Normal", "Circle_Fast", "D_Slow", "D_Normal", "D_Fast", "Circle_FastLeftSlowRight", "Circle_SlowLeftFastRight" }) },
            { "OwnForms", new MultipleUserGestureSet("OwnForms", new string[] { "Shaik", "Bahl", "Graham", "Christian", "Krull", "Tim", "Roxi", "Anton", "Antje", "Clemens" }, "OwnForm_1Finger") }
        };

        public static Dictionary<string, IEnumerable<BaseTrajectory>> getSignatures(bool useDB1)
        {
            var allSignatures = SignatureDatabase.SignatureDatabase.getAllSignatures(useDB1,5);
            return allSignatures.ToDictionary(sigs => sigs.First().UserID.ToString(), sigs => sigs.Cast<BaseTrajectory>());
        }

        //private static Dictionary<string, GestureTraces> getCachedSignatureSet(GestureSet set)
        //{
        //    string gestureSetPath = "..\\..\\" + cachePath + set.Name + ".gestures";

        //    Stream fileStream;
        //    BinaryFormatter serializer = new BinaryFormatter();
        //    Dictionary<string, GestureTraces> gestureSet;

        //    if (File.Exists(gestureSetPath))
        //    {
        //        fileStream = File.OpenRead(gestureSetPath);
        //        gestureSet = (Dictionary<string, GestureTraces>)serializer.Deserialize(fileStream);
        //        fileStream.Close();
        //    }
        //    else
        //    {
        //        if (set is UserGestureSet)
        //        {
        //            var newSet = set as UserGestureSet;
        //            gestureSet = newSet.GestureNames.Select(g => new { GestureName = g, Traces = Helper.getGestureTraces(newSet.UserName, g) }).ToDictionary(e => e.GestureName, e => new GestureTraces(e.Traces));

        //            fileStream = File.Create(gestureSetPath);
        //            serializer.Serialize(fileStream, gestureSet);
        //            fileStream.Close();
        //        }
        //        else if (set is MultipleUserGestureSet)
        //        {
        //            var newSet = set as MultipleUserGestureSet;
        //            gestureSet = newSet.UserNames.Select(u => new { GestureName = newSet.GestureName + "_" + u, Traces = Helper.getGestureTraces(u, newSet.GestureName) }).ToDictionary(e => e.GestureName, e => new GestureTraces(e.Traces));

        //            fileStream = File.Create(gestureSetPath);
        //            serializer.Serialize(fileStream, gestureSet);
        //            fileStream.Close();
        //        }
        //        else throw new ArgumentException();
        //    }

        //    return gestureSet;
        //}

        public static GestureDataSet getGestureSet(string gestureSetName)
        {
            var set = gestureSets[gestureSetName];
            return getCachedGestureSet(set);
        }

        public static IEnumerable<TrajectoryDataSet> getVerificationDataSet(string setName, int nTraining, bool session2)
        {
            switch(setName)
            {
                case "PseudoSigsSkilled": return getDoodleVerificationSet(true, null, nTraining, true, session2);
                case "PseudoSigsRandom": return getDoodleVerificationSet(true, null, nTraining, false, session2);
                case "DoodlesSkilled": return getDoodleVerificationSet(false, null, nTraining, true, session2);
                case "DoodlesRandom": return getDoodleVerificationSet(false, null, nTraining, false, session2);

                default: throw new ArgumentException("Unknown data set");
            }
        }

        public static IEnumerable<TrajectoryDataSet> getDoodleVerificationSet(bool usePseudoSigs, int? nUser, int? nTraining, bool useSkilledForgeries, bool session2)
        {
            TrajectoryDataSet trainingSets = new TrajectoryDataSet();
            TrajectoryDataSet genuineSets = new TrajectoryDataSet();
            TrajectoryDataSet forgerySets = new TrajectoryDataSet();

            var allTrajectories = (usePseudoSigs) ? 
                    SignatureDatabase.SignatureDatabase.getAllPseudoSigs(nUser: nUser) :
                    SignatureDatabase.SignatureDatabase.getAllDoodles(nUser: nUser);

            foreach (var userTraces in allTrajectories)
            {
                string user = userTraces.First().UserID.ToString();
                var trainingTraces = (nTraining.HasValue) ?
                    userTraces.Where(t => !t.IsForgery && !t.IsSession2).Take(nTraining.Value).ToArray() :
                    userTraces.Where(t => !t.IsForgery && !t.IsSession2).ToArray();
                var rest = userTraces.Except(trainingTraces).ToArray();
                var genuineTraces = rest.Where(t => !t.IsForgery && t.IsSession2 == session2).ToArray();
                var forgeryTraces = (useSkilledForgeries) ?
                    rest.Where(t => t.IsForgery).ToArray() :
                    allTrajectories.Select(t => t.First()).Where(d => d.UserID.ToString() != user).ToArray();

                trainingSets[user] = trainingTraces;
                genuineSets[user] = genuineTraces;
                forgerySets[user] = forgeryTraces;
            }

            yield return trainingSets;
            yield return genuineSets;
            yield return forgerySets;
        }

        public static TrajectoryDataSet getGenuineDoodles()
        {
            var allSignatures = SignatureDatabase.SignatureDatabase.getAllDoodles();
            return allSignatures.ToDictionary(sigs => sigs.First().UserID.ToString(), sigs => sigs.Cast<BaseTrajectory>());
        }
        public static TrajectoryDataSet getForgeryDoodles()
        {
            var allSignatures = SignatureDatabase.SignatureDatabase.getAllDoodles(nUser: 15);
            return allSignatures.ToDictionary(sigs => sigs.First().UserID.ToString(), sigs => sigs.Cast<BaseTrajectory>());
        }
        public static TrajectoryDataSet getPseudoSigs()
        {
            var allSignatures = SignatureDatabase.SignatureDatabase.getAllPseudoSigs(true, 15);
            return allSignatures.ToDictionary(sigs => sigs.First().UserID.ToString(), sigs => sigs.Cast<BaseTrajectory>());
        }
        public static TrajectoryDataSet getForgeryPseudoSigs()
        {
            var allSignatures = SignatureDatabase.SignatureDatabase.getAllPseudoSigs(true, 15);
            return allSignatures.ToDictionary(sigs => sigs.First().UserID.ToString(), sigs => sigs.Cast<BaseTrajectory>());
        }

        public static TrajectoryDataSet getTrajectoryDataSet(string datasetName)
        {
            switch (datasetName)
            {
                case "SyntheticSigs1":
                    return getSignatures(true);

                case "SyntheticSigs2":
                    return getSignatures(false);

                case "Doodles":
                    return getGenuineDoodles();

                case "PseudoSigs":
                    return getPseudoSigs();

                default:
                    var set = gestureSets[datasetName];
                    return getCachedTrajectoryDataSet(set);
            }
        }

        private static TrajectoryDataSet getCachedTrajectoryDataSet(GestureSet set)
        {
            string gestureSetPath = "..\\..\\" + cachePath + set.Name + ".trajectories";

            Stream fileStream;
            BinaryFormatter serializer = new BinaryFormatter();
            TrajectoryDataSet gestureSet;

            if (File.Exists(gestureSetPath))
            {
                fileStream = File.OpenRead(gestureSetPath);
                gestureSet = (TrajectoryDataSet)serializer.Deserialize(fileStream);
                fileStream.Close();
            }
            else
            {
                if (set is UserGestureSet)
                {
                    var newSet = set as UserGestureSet;
                    gestureSet = newSet.GestureNames.Select(g => new { GestureName = g, Traces = Helper.getGestureTraces(newSet.UserName, g) }).ToDictionary(e => e.GestureName, e => (IEnumerable<BaseTrajectory>)(e.Traces.Select(t => (BaseTrajectory)t.LongestStroke).ToArray()));

                    fileStream = File.Create(gestureSetPath);
                    serializer.Serialize(fileStream, gestureSet);
                    fileStream.Close();
                }
                else if (set is MultipleUserGestureSet)
                {
                    var newSet = set as MultipleUserGestureSet;
                    gestureSet = newSet.UserNames.Select(u => new { GestureName = newSet.GestureName + "_" + u, Traces = Helper.getGestureTraces(u, newSet.GestureName) }).ToDictionary(e => e.GestureName, e => (IEnumerable<BaseTrajectory>)(e.Traces.Select(t => (BaseTrajectory)t.LongestStroke).ToArray()));

                    fileStream = File.Create(gestureSetPath);
                    serializer.Serialize(fileStream, gestureSet);
                    fileStream.Close();
                }
                else throw new ArgumentException();
            }

            return gestureSet;
        }

        private static GestureDataSet getCachedGestureSet(GestureSet set)
        {
            string gestureSetPath = "..\\..\\" + cachePath + set.Name + ".gestures";

            Stream fileStream;
            BinaryFormatter serializer = new BinaryFormatter();
            GestureDataSet gestureSet;

            if (File.Exists(gestureSetPath))
            {
                fileStream = File.OpenRead(gestureSetPath);
                gestureSet = (GestureDataSet)serializer.Deserialize(fileStream);
                fileStream.Close();
            }
            else
            {
                if (set is UserGestureSet)
                {
                    var newSet = set as UserGestureSet;
                    gestureSet = newSet.GestureNames.Select(g => new { GestureName = g, Traces = Helper.getGestureTraces(newSet.UserName, g) }).ToDictionary(e => e.GestureName, e => new GestureTraces(e.Traces));

                    fileStream = File.Create(gestureSetPath);
                    serializer.Serialize(fileStream, gestureSet);
                    fileStream.Close();
                }
                else if (set is MultipleUserGestureSet)
                {
                    var newSet = set as MultipleUserGestureSet;
                    gestureSet = newSet.UserNames.Select(u => new { GestureName = newSet.GestureName + "_" + u, Traces = Helper.getGestureTraces(u, newSet.GestureName) }).ToDictionary(e => e.GestureName, e => new GestureTraces(e.Traces));

                    fileStream = File.Create(gestureSetPath);
                    serializer.Serialize(fileStream, gestureSet);
                    fileStream.Close();
                }
                else throw new ArgumentException();
            }

            return gestureSet;
        }

    }
}
