using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestAPI
{
    class RestfullServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:5000/";
        public static int requestCount = 0;

        public static string ControllerHandler(string method, string path, string auth, string body)
        {
            Console.WriteLine("Controller: " + method + " " + path + " " + auth + " " + body);

            try
            {
                // TODO Code here
                
                return "{status:\"Method not supported\"}";
            }
            catch(Exception exc)
            {
                Console.Error.WriteLine(exc);
                return "{status:\"Exception: "+exc.ToString()+"\"}";
            }            
        }

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine(req.Url.AbsolutePath);
                var auth = req.Headers["authorization"];
                Console.WriteLine(auth);
                var content = "";
                if (req.HttpMethod == "POST" || req.HttpMethod == "PUT")
                {
                    byte[] buffer = new byte[req.ContentLength64];
                    var real_len = req.InputStream.ReadAsync(buffer, 0, buffer.Length);
                    content = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine(content);
                }
                Console.WriteLine();

                resp.AddHeader("Access-Control-Allow-Origin", (string)req.Headers["Origin"]);

                if (req.HttpMethod == "OPTIONS")
                {
                    resp.StatusCode = 204;
                    resp.AddHeader("Allow", "OPTIONS, GET, POST, PUT, DELETE");
                    resp.AddHeader("Access-Control-Allow-Headers","Origin, X-Requested-With, Content-Type, Accept, Authorization");
                    resp.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    resp.AddHeader("Access-Control-Allow-Credentials", "true");
                }
                else
                {
                    var respData = ControllerHandler(req.HttpMethod, req.RawUrl, auth, content);

                    Console.WriteLine("ResponseData: " + respData);
                    Console.WriteLine();
                    Console.WriteLine();

                    // Write the response info
                    byte[] data = Encoding.UTF8.GetBytes(respData);
                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }
                resp.Close();
            }
        }

        public static void Main(string[] args)
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}
