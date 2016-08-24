using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LfS.GestureDatabase
{
    
//    class AuthenticationServer
//    {
//        private DataReceiverHTML htmlReceiver;
//        private string dbPath;

//        public AuthenticationServer(string dbPath)
//        {
//            htmlReceiver = new DataReceiverHTML();
//            htmlReceiver.dataReceived += onDataReceived;
//            this.dbPath = dbPath;
//        }

//        public bool authenticateKnownUser(string user, IEnumerable<Observation> trace)
//        {
//            throw new NotImplementedException();

//            //if (!db.ContainsKey(user))
//            //{
//            //    Console.Error.WriteLine("User '" + user + "' unknown");
//            //    return false;
//            //}

//            //var pw = db[user];
//            //return pw.checkTrace(trace);
//        }

//        public bool authenticateUserByTrace(IEnumerable<Observation> trace)
//        {
//            //ToDo: go through all users 
//            throw new NotImplementedException();
//            //return false;
//        }

//        private void onDataReceived(string data, string serviceName)
//        {
//            int pos = data.IndexOf("|");
//            string username = data.Substring(0, pos);
//            string strTrace = data.Substring(pos+1);

//            if (serviceName.Equals("/training"))
//            {
//                using (dbEntities con = new dbEntities())
//                {
//                    User user = con.Users.FirstOrDefault(u => u.Username == username);
//                    if (user != null)
//                    {

//                        Console.WriteLine(con.Observations.First().Time);
//                        Trace trace = new Trace();
//                        trace.User = user;
//                        foreach (Observation o in strToTrace(strTrace))
//                        {
//                            trace.Observations.Add(o);
//                        }

//                        user.TrainingTraces.Add(trace);
//                        con.SaveChanges();
//                    }
//                }

//                /*
//                var user = db[username];
//                if (user == null)
//                    user = new User(username);

//                user.addTrainingTrace(strToTrace(trace));
//                db[username] = user;
//                 */
//            }

//            if (serviceName.Equals("/authentication"))
//            {
//            }
//        }


//        private IEnumerable<Observation> strToTrace(string str)
//        {
//            string[] strValues = new string[2];
//            int curValue = 0;
//            string strValue = "";
//            Observation o;
//            foreach (char c in str)
//                if (c == '|')
//                {
//                    strValues[curValue] = strValue;

//                    o = new Observation();
//                    o.Symbol = strValues[0];
//                    o.Time = long.Parse(strValues[1]);

//                    yield return o;

//                    curValue = 0;
//                    strValue = "";
//                }
//                else if (c == ';')
//                {
//                    strValues[curValue++] = strValue;
//                    strValue = "";
//                }
//                else strValue += c;

//            yield break;
//        }
//    }
}
