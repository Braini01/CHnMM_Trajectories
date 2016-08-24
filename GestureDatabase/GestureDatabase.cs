using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LfS.GestureDatabase
{
    //ToDo: add thread safety
    public static class GestureDatabase
    {
        /// <summary>
        /// adds a user to the db
        /// </summary>
        /// <param name="username"></param>
        /// <param name="name"></param>
        /// <returns>return true if the user was added and false if the user already exists</returns>
        public static bool addUser(string username, string name)
        {
            using (dbEntities con = new dbEntities())
            {
                //user already in db?
                User user = con.Users.FirstOrDefault(u => u.Username.Equals(username));
                if(user != null) return false;
    
                user = new User();

                user.Username = username;
                user.Name = name;

                con.Users.Add(user);

                String[] gestures = {"Circle_1Finger","Square_1Finger","OwnForm_1Finger","Circle_2Finger","Square_2Finger","OwnForm_2Finger","Open_5Finger","Close_5Finger","TurnLeft_5Finger","TurnRight_5Finger" };
                foreach (var g in gestures)
                {
                    var gesture = new Gesture();
                    gesture.Name = g;
                    gesture.User = user;
                    con.Gestures.Add(gesture);
                }
                /*
                var gesture1 = new Gesture();
                gesture1.Name = "Gesture1";
                gesture1.User = user;
                var gesture2 = new Gesture();
                gesture2.Name = "Gesture2";
                gesture2.User = user;
                var gesture3 = new Gesture();
                gesture3.Name = "Gesture3";
                gesture3.User = user;
                con.Gestures.Add(gesture1);
                con.Gestures.Add(gesture2);
                con.Gestures.Add(gesture3);
                */

                con.SaveChanges();

                return true;
            }
        }

        public static bool userExists(string username)
        {
            using (dbEntities con = new dbEntities())
            {
                if (con.Users.FirstOrDefault(u => u.Username.Equals(username)) == null) return false;
                else return true;
            }
        }

        public static User getUser(string username)
        {
            using (dbEntities con = new dbEntities())
            {
                return con.Users.FirstOrDefault(u => u.Username.Equals(username));
            }
        }

        public static User getUser(int id)
        {
            using (dbEntities con = new dbEntities())
            {
                return con.Users.Find(id);
            }
        }

        public static Gesture getGesture(int id)
        {
            using (dbEntities con = new dbEntities())
            {
                return con.Gestures.Find(id);
            }
        }

        public static int TraceCount(int gestureId)
        {
            using (dbEntities con = new dbEntities())
            {
                return con.Gestures.Find(gestureId).Traces.Count;
            }
        }

        public static void addTrace(IEnumerable<Touch> touches, int gestureId, DeviceInfo dInfo, TouchField tfInfo)
        {
            try
            {
                using (dbEntities con = new dbEntities())
                {
                    var Gesture = con.Gestures.Find(gestureId);

                    //dInfo or tfInfo already known?
                    var oldDInfo = con.DeviceInfos.FirstOrDefault(d => d.ScreenH == dInfo.ScreenH && d.ScreenW == dInfo.ScreenW && d.Platform.Equals(dInfo.Platform) && d.UserAgent.Equals(dInfo.UserAgent));
                    if (oldDInfo != null) dInfo = oldDInfo;

                    var oldTfInfo = con.TouchFields.FirstOrDefault(t => t.Left == tfInfo.Left && t.Right == tfInfo.Right && t.Top == tfInfo.Top && t.Bottom == tfInfo.Bottom && t.Width == tfInfo.Width && t.Height == tfInfo.Height);
                    if (oldTfInfo != null) tfInfo = oldTfInfo;

                    var trace = new Trace();
                    trace.DeviceInfo = dInfo;
                    trace.TouchField = tfInfo;
                    trace.Gesture = Gesture;

                    foreach (var touch in touches)
                    {
                        touch.Trace = trace;
                        con.Touches.Add(touch);
                    }

                    con.Traces.Add(trace);
                    if (oldTfInfo != tfInfo) con.TouchFields.Add(tfInfo);
                    if (oldDInfo != dInfo) con.DeviceInfos.Add(dInfo);

                    con.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.Write(e.StackTrace);
            }
        }

        public static Trace[] getGestureTraces(string userName, string gestureName)
        {
            using (dbEntities ctx = new dbEntities())
            {
                var gesture = ctx.Gestures.Include("Traces.Touches").First(g => g.Name == gestureName && g.User.Username == userName);
                return gesture.Traces.ToArray();
            }
        }

        public static IGrouping<string, Trace>[] getGestureTraces(string gestureName)
        {
            using (dbEntities ctx = new dbEntities())
            {
                var gestures = ctx.Gestures.Include("Traces.Touches").Where(g => g.Name == gestureName);
                var groups = gestures.SelectMany(g => g.Traces, (g, trace) => new { Username = g.User.Name, Trace = trace }).GroupBy(e => e.Username, e => e.Trace);
                return groups.ToArray();
            }
        }

        /*
        public static void addTrainingTrace(User user, IEnumerable<Observation> observations)
        {
            Trace t = new Trace();

            foreach (Observation o in observations)
                t.Observations.Add(o);

            user.TrainingTraces.Add(t);
        }
         * */
    }
}
