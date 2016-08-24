using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GestureRecognitionLib;
using System.Runtime.Serialization.Formatters.Binary;

namespace SignatureDatabase
{
    [Serializable]
    public class Signature: BaseTrajectory
    {
        public int UserID { get; }
        public int TraceID { get; }
        public override TrajectoryPoint[] TrajectoryPoints { get; }

        public Signature(int user, int trace, TrajectoryPoint[] points)
        {
            UserID = user;
            TraceID = trace;
            TrajectoryPoints = points;
        }
    }

    [Serializable]
    public class Doodle : BaseTrajectory
    {
        public int UserID { get; }
        public int TraceID { get; }
        public bool IsSession2 { get; }
        public bool IsForgery { get; }
        public byte CaptureBlock { get; }
        public int ForgeryUserID { get; }
        public override TrajectoryPoint[] TrajectoryPoints { get; }

        public Doodle(int user, int trace, bool isSession2, bool isForgery, byte block, int forgeryUserId, TrajectoryPoint[] points)
        {
            UserID = user;
            TraceID = trace;
            IsSession2 = isSession2;
            IsForgery = isForgery;
            CaptureBlock = block;
            ForgeryUserID = forgeryUserId;
            TrajectoryPoints = points;
        }
    }

    //public class Point
    //{
    //    public int X { get; }
    //    public int Y { get; }
    //    public long Time { get; }

    //    public Point(int x, int y, long t)
    //    {
    //        X = x;
    //        Y = y;
    //        Time = t;
    //    }
    //}

    public static class SignatureDatabase
    {
        private const string dbSSig1 = @"..\..\..\Databases\ATVS-SSig_DB\DS1_Modification_TimeFunctions";
        private const string dbSSig2 = @"..\..\..\Databases\ATVS-SSig_DB\DS2_Modification_LNparameters";
        private const string dbDoodles = @"..\..\..\Databases\ATVS-Doo_DB\DOODLES";
        private const string dbPseudoSigs = @"..\..\..\Databases\ATVS-Doo_DB\PSEUDO-SIGNATURES";

        private static bool normalizeCoords(ref double x, ref double y, int w, int h)
        {
            double max = Math.Max(w, h);

            x = x / max;
            y = y / max;

            if (x > 1.0 || x < 0 || y > 1.0 || y < 0)
                return false;

            return true;
        }

        public static IEnumerable<IEnumerable<Signature>> getAllSignatures(bool useDB1)
        {
            var path = Path.GetFullPath((useDB1) ? dbSSig1 : dbSSig2);
            var userFolders = Directory.GetDirectories(path);

            foreach (var userFolder in userFolders)
                yield return readSSigUserFolder(userFolder);
        }

        public static IEnumerable<IEnumerable<Signature>> getAllSignatures(bool useDB1, int nUser)
        {
            var path = Path.GetFullPath((useDB1) ? dbSSig1 : dbSSig2);
            var userFolders = Directory.GetDirectories(path);

            int i = 0;
            foreach (var userFolder in userFolders)
                if(i++ < nUser) yield return readSSigUserFolder(userFolder);
        }

        public static bool SerializeAllDoodles(Doodle[][] doodles, string dbPath, bool normalize = true, int? nUser = null, bool? getForgeries = null)
        {
            Stream fileStream;
            BinaryFormatter serializer = new BinaryFormatter();

            var normalized = normalize ? "_normalized" : "";
            var txtForgeries = (getForgeries.HasValue) ? (getForgeries.Value ? "_ForgeriesOnly" : "_GenuineOnly") : "";
            var txtUser = (nUser.HasValue) ? "_" + nUser.Value.ToString() : "";
            var dbName = $"\\PseudoSigs{normalized}{txtUser}{txtForgeries}.serialized";
            var filePath = dbPath + dbName;
            if (!File.Exists(filePath))
            {
                fileStream = File.Create(dbPath + dbName);
                serializer.Serialize(fileStream, doodles);
                fileStream.Close();
                return true;
            }

            return false;
        }

        public static Doodle[][] DeserializeAllDoodles(string dbPath, bool normalize = true, int? nUser = null, bool? getForgeries = null)
        {
            Stream fileStream;
            BinaryFormatter serializer = new BinaryFormatter();

            var normalized = normalize ? "_normalized" : "";
            var txtUser = (nUser.HasValue) ? "_" + nUser.Value.ToString() : "";
            var txtForgeries = (getForgeries.HasValue) ? (getForgeries.Value ? "_ForgeriesOnly" : "_GenuineOnly") : "";
            var dbName = $"\\PseudoSigs{normalized}{txtUser}{txtForgeries}.serialized";
            var filePath = dbPath + dbName;
            if (File.Exists(filePath))
            {
                fileStream = File.OpenRead(filePath);
                var data = (Doodle[][])serializer.Deserialize(fileStream);
                fileStream.Close();
                return data;
            }

            return null;
        }

        public static IEnumerable<IEnumerable<Doodle>> getAllPseudoSigs(bool normalize = true, int? nUser = null, bool? getForgeries = null)
        {
            return getAllDoodles(normalize, dbPseudoSigs, nUser, getForgeries);
        }

        public static Doodle[][] getAllDoodles(bool normalize = true, string dbPath = dbDoodles, int? nUser = null, bool? getForgeries = null)
        {
            var doodles = DeserializeAllDoodles(dbPath, normalize, nUser);
            if (doodles == null)
            {
                var doodlesEnum = readAllDoodles(normalize, dbPath, nUser);
                doodles = doodlesEnum.Select(u => u.ToArray()).ToArray();
                SerializeAllDoodles(doodles, dbPath, normalize, nUser);
            }

            return doodles;
        }

        public static IEnumerable<IEnumerable<Doodle>> readAllDoodles(bool normalize = true, string dbPath = dbDoodles, int? nUser = null, bool? getForgeries = null)
        {
            var path = Path.GetFullPath(dbPath);
            var userFolders = Directory.GetDirectories(path);

            int i = 0;
            foreach (var userFolder in userFolders)
            {
                var session1 = readDoodleUserFolder(userFolder + "\\SS1", false, normalize);
                var session2 = readDoodleUserFolder(userFolder + "\\SS2", true, normalize);

                IEnumerable<Doodle> set1, set2;
                if (!getForgeries.HasValue)
                {
                    //return all
                    yield return session1.Concat(session2);
                }
                else if (getForgeries.Value)
                {
                    //return forgeries
                    set1 = session1.Where(d => d.IsForgery);
                    set2 = session2.Where(d => d.IsForgery);
                    yield return set1.Concat(set2);
                }
                else
                {
                    //return genuine
                    set1 = session1.Where(d => !d.IsForgery);
                    set2 = session2.Where(d => !d.IsForgery);
                    yield return set1.Concat(set2);
                }

                if (nUser.HasValue && (++i == nUser)) break;
            }
        }

        public static IEnumerable<Signature> readSSigUserFolder(string folderName)
        {
            var files = Directory.GetFiles(folderName);

            foreach(var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var split = name.Split('_');
                var user = Convert.ToInt32(split[0].Substring(1));
                var trace = Convert.ToInt32(split[1].Substring(2));

                yield return new Signature(user, trace, readSSigFile(file).ToArray());
            }
        }

        public static IEnumerable<Doodle> readDoodleUserFolder(string folderName, bool isSession2, bool normalize)
        {
            var files = Directory.GetFiles(folderName);

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var split = name.Split('_');
                var isForgery = split[1].Substring(0, 3).Equals("FOR");
                var captureBlock = Convert.ToByte(split[1].Substring(3));
                var userId = Convert.ToInt32(split[2].Substring(2, 4));
                var forgeryUserID = Convert.ToInt32(split[3].Substring(2,4));
                var trace = Convert.ToInt32(split[4]);

                yield return new Doodle(userId, trace, isSession2, isForgery, captureBlock, forgeryUserID, readDooDBFile(file, normalize).ToArray());
            }
        }

        public static IEnumerable<TrajectoryPoint> readTrajectoryTextFile(string fileName, char delimiter, int linesToSkip, bool hasTimeIntervals, bool removeZeroCoords, bool normalize, int width, int height)
        {
            var lines = File.ReadLines(fileName).Skip(linesToSkip);

            long timestamp = 0;
            foreach (var line in lines)
            {
                if (line == "") continue;
                var split = line.Split(delimiter);

                int x = Convert.ToInt32(split[0]);
                int y = Convert.ToInt32(split[1]);
                long t = Convert.ToInt64(split[2]);

                if(hasTimeIntervals) timestamp += t;

                if (removeZeroCoords && x == 0 && y == 0)
                    continue;

                double dx = x;
                double dy = y;
                if (normalize)
                {
                    if( ! normalizeCoords(ref dx, ref dy, width, height) )
                        throw new ArgumentException("TrajectoryPoint out of bounds");
                }

                yield return new TrajectoryPoint(dx, dy, hasTimeIntervals ? timestamp : t);
            }
        }

        private static IEnumerable<TrajectoryPoint> readSSigFile(string fileName)
        {
            return readTrajectoryTextFile(fileName, ' ', 0, false, false, false, 0, 0);
        }

        private static IEnumerable<TrajectoryPoint> readDooDBFile(string fileName, bool normalize = true)
        {
            return readTrajectoryTextFile(fileName, '\t', 0, true, true, normalize, 2000, 3500);
        }
    }
}
