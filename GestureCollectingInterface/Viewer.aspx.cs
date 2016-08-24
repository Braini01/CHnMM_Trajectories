using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using LfS.GestureDatabase;

namespace GestureCollectingInterface
{
    public partial class Viewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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

        protected void cbTraces_SelectedIndexChanged(object sender, EventArgs e)
        {
            canvasBG.ImageUrl = "TraceImage.aspx?id=" + cbTraces.SelectedValue;
        }
    }
}
