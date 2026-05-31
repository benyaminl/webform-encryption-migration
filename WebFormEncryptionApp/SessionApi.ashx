<%@ WebHandler Language="C#" Class="WebFormEncryptionApp.SessionApiHandler" %>

using System;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace WebFormEncryptionApp
{
    public class SessionApiHandler : IHttpHandler, IRequiresSessionState
    {
        private const string ApiKey = "00000000-0000-0000-0000-000000000001";

        public void ProcessRequest(HttpContext context)
        {
            var key = context.Request.Headers["X-Session-ApiKey"];
            if (key != ApiKey)
            {
                context.Response.StatusCode = 401;
                return;
            }

            context.Response.ContentType = "application/json";

            var session = context.Session;
            var serializer = new JavaScriptSerializer();

            if (context.Request.HttpMethod == "PUT" || context.Request.HttpMethod == "POST")
            {
                var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                var values = serializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(body);
                if (values != null)
                {
                    foreach (var kv in values)
                    {
                        session[kv.Key] = kv.Value;
                    }
                }
            }

            var data = new System.Collections.Generic.Dictionary<string, object>();
            foreach (string k in session.Keys)
            {
                data[k] = session[k];
            }

            var result = new System.Collections.Generic.Dictionary<string, object>();
            result["_debug"] = new
            {
                receivedSessionId = session.SessionID,
                cookieHeader = context.Request.Headers["Cookie"],
                sessionCount = session.Count,
                isNewSession = session.IsNewSession
            };
            result["session"] = data;

            // This is for debug only!
            // context.Response.Write(serializer.Serialize(result)); 
            
            context.Response.Write(serializer.Serialize(data));
        }

        public bool IsReusable { get { return false; } }
    }
}
