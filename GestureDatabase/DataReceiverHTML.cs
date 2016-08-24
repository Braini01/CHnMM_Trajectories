using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace LfS.GestureDatabase
{
    //public delegate void DataReceivedEvent(string data, string serviceName);

    //public class DataReceiverHTML
    //{
    //    public event DataReceivedEvent dataReceived;

    //    public DataReceiverHTML()
    //    {
    //        HttpListener receiver = new HttpListener();
    //        receiver.Prefixes.Add("http://*:55555/");
    //        receiver.Start();
    //        receiver.BeginGetContext(receiveHTTPRequest, receiver);
    //    }

    //    public DataReceiverHTML(string prefix)
    //    {
    //        HttpListener receiver = new HttpListener();
    //        receiver.Prefixes.Add("http://*:55555/"+prefix+"/");
    //        receiver.Start();
    //        receiver.BeginGetContext(receiveHTTPRequest, receiver);
    //    }

    //    private void receiveHTTPRequest(IAsyncResult ar)
    //    {
    //        HttpListener listener = (HttpListener)ar.AsyncState;
    //        listener.BeginGetContext(receiveHTTPRequest, listener);
    //        // Call EndGetContext to complete the asynchronous operation.
    //        HttpListenerContext context = listener.EndGetContext(ar);

    //        switch (context.Request.HttpMethod)
    //        {

    //            case "GET":
    //                createResponseGET(context);
    //                break;

    //            case "POST":
    //                createResponsePOST(context);
    //                break;
    //            default:
    //                createResponseBadRequest(context.Response);
    //                break;
    //        }
    //    }

    //    /*
    //    private void createResponseOPTIONS(HttpListenerContext c)
    //    {
    //        var req = c.Request;
    //        var res = c.Response;

    //        var origin = req.Headers["Origin"];
    //        var method = req.Headers["Access-Control-Request-Method"];
    //        var headers = req.Headers["Access - Control - Request - Headers"];

    //        //res.AddHeader("Allow", "OPTIONS,POST");
    //        //res.AddHeader("Content-Length", "0");
    //        res.AddHeader("Access-Control-Allow-Origin", "*");
    //        res.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
    //        //res.AddHeader("Access-Control-Allow-Methods", "OPTIONS");
    //        //res.Headers.Add("Access-Control-Max-Age",);
    //        //res.Headers.Add("Access-Control-Allow-Credentials","true");

    //        res.StatusCode = 200;
    //        res.StatusDescription = "OK";
    //        res.Close();
    //    }
    //    */

    //    private void createResponseGET(HttpListenerContext c)
    //    {
    //        var req = c.Request;
    //        var res = c.Response;

    //        var reqFile = (req.RawUrl.Equals("/")) ? "/auth.html" : req.RawUrl;
    //        var localFilePath = "site" + reqFile;

    //        if (File.Exists(localFilePath))
    //        {
    //            var ext = Path.GetExtension(localFilePath);
    //            var type = (ext.Equals(".js")) ? "text/javascript; charset=UTF-8" : "text/html; charset=UTF-8";
    //            var buffer = File.ReadAllBytes(localFilePath);
    //            res.ContentType = type;
    //            res.OutputStream.Write(buffer, 0, buffer.Length);

    //            res.StatusCode = 200;
    //            res.StatusDescription = "OK";
    //            res.Close();
    //        }
    //        else
    //        {
    //            res.ContentType = "text/plain; charset=UTF-8";
    //            res.StatusCode = 404;
    //            res.StatusDescription = "File Not Found";
    //            res.Close();
    //        }
    //    }

    //    private void createResponsePOST(HttpListenerContext c)
    //    {
    //        var req = c.Request;
    //        var res = c.Response;

    //        //send data to submitted processes
    //        byte[] buffer = new byte[req.ContentLength64];
    //        req.InputStream.Read(buffer, 0, (int)req.ContentLength64);

    //        var ascii = new System.Text.ASCIIEncoding();

    //        string json = ascii.GetString(buffer);

    //        dataReceived(json, req.RawUrl);

    //        res.StatusCode = 200;
    //        res.StatusDescription = "OK";
    //        res.Close();
    //    }

    //    private void createResponseBadRequest(HttpListenerResponse res)
    //    {
    //        res.StatusCode = 400;
    //        res.StatusDescription = "Bad Request";
    //        res.Close();
    //    }
    //}
}
