using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data.Entity;
using LfS.GestureDatabase;

namespace GestureCollectingInterface
{
    public partial class InputData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.HttpMethod == "POST" && !IsPostBack)
            {
                //Gesture data was sent via POST request
                var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                var inputStream = new StreamReader(Request.InputStream);
                var jsonData = inputStream.ReadToEnd();
                var data = jss.Deserialize<List<object>>(jsonData);

                var userID = int.Parse((string)data[0]);
                var gestureID = int.Parse((string)data[1]);
                //var userID = 1;
                //var gestureID = 1;
                var deviceInfo = (Dictionary<string,object>)data[2];
                var touchFieldInfo = (Dictionary<string, object>)data[3];
                var gestureData = (object[])data[4];

                DeviceInfo dInfo = new DeviceInfo();
                dInfo.ScreenW = (int)deviceInfo["screenW"];
                dInfo.ScreenH = (int)deviceInfo["screenH"];
                dInfo.Platform = (string)deviceInfo["platform"];
                dInfo.UserAgent = (string)deviceInfo["userAgent"];

                TouchField tfInfo = new TouchField();
                tfInfo.Top = (int)touchFieldInfo["top"];
                tfInfo.Bottom = (int)touchFieldInfo["bottom"];
                tfInfo.Left = (int)touchFieldInfo["left"];
                tfInfo.Right = (int)touchFieldInfo["right"];
                tfInfo.Width = (int)touchFieldInfo["width"];
                tfInfo.Height = (int)touchFieldInfo["height"];

                //var trace = new SortedList<long, Touch>(gestureData.Length);
                var trace = new LinkedList<Touch>();

                foreach (object[] touchEvent in gestureData)
                {
                    foreach (object touchData in touchEvent)
                    {
                        var touchObject = (Dictionary<string, object>)touchData;
                        var touch = new Touch();

                        if (touchObject["id"] is long)
                        {
                            touch.FingerId = (long)touchObject["id"];
                        }
                        else
                        {
                            touch.FingerId = (int)touchObject["id"];
                        }
                        touch.X = (decimal)touchObject["x"];
                        touch.Y = (decimal)touchObject["y"];
                        
                        /*
                        touch.RadiusX = (decimal)touchObject["radiusX"];
                        touch.RadiusY = (decimal)touchObject["radiusY"];
                        touch.RotationAngle = (decimal)touchObject["rotationAngle"];
                        touch.Force = (decimal)touchObject["force"];
                         */ 
                        touch.Time = (long)touchObject["time"];

                        //trace.Add(touch.Time, touch);
                        trace.AddLast(touch);
                    }
                }

                GestureDatabase.addTrace(trace, gestureID, dInfo, tfInfo);

                Response.StatusCode = 200;
                Response.StatusDescription = "OK";
                Response.SuppressContent = true;
                Response.ClearContent();
            }
            
            /*
            if (Request.HttpMethod == "GET")
            {
                string idStr = Request.Params.Get("GetTraceCount");
                if(!string.IsNullOrEmpty(idStr))
                {
                    int n = GestureDatabase.TraceCount(int.Parse(idStr));

                    Response.Clear();
                    Response.StatusCode = 200;
                    Response.Status = "200 OK";
                    Response.ContentType = "text/plain";
                    Response.Output.Write(n);
                    Response.
                }
            }
            */
        }
    }
}
