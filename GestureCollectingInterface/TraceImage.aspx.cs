using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using LfS.GestureDatabase;

namespace GestureCollectingInterface
{
    public partial class TraceImage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String strTraceID = Request.QueryString["id"];

            int traceID = int.Parse(strTraceID);
            Bitmap img = new Bitmap(@"D:\Dropbox\LfS\Code\GestureCollectingInterface\canvasBG.png");
            var g = Graphics.FromImage(img);
            Pen pen = new Pen(Color.Black, 2);
            Brush b = new SolidBrush(Color.Red);

            using (var ctx = new dbEntities())
            {
                var points = from t in ctx.Touches where t.Trace.Id == traceID orderby t.Time select new {X = t.X, Y = t.Y};

                int size = 10;
                foreach(var p in points)
                {
                    var rect = new Rectangle((int)(p.X*600 - size), (int)(p.Y*600 - size), size*2, size*2);
                    g.DrawEllipse(pen, rect);
                    g.FillEllipse(b, rect);
                }
            }

            g.Dispose();

			Response.Clear();
			Response.ContentType = "image/png";
			img.Save(this.Response.OutputStream, ImageFormat.Png);
        }
    }
}