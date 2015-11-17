using Nancy;
using Nancy.Hosting.Self;
using System;
using System.Net;

namespace HttpProcessWrapper
{
    class Program
    {
        static Uri baseUri = new Uri("http://localhost:25564");

        static void Main(string[] args)
        {
            var config = new HostConfiguration();
            //config.UrlReservations.CreateAutomatically = true;
            config.RewriteLocalhost = false;

            var bootstrapper = new Bootstrapper();
            using (var host = new NancyHost(bootstrapper, config, baseUri))
            {
                for (int i = 0; ; i++)
                {
                    try
                    {
                        host.Start();
                        break;
                    }
                    catch (HttpListenerException ex)
                    {
                        // Error 183 happens when the HttpListener fails to listen on the provided Uri
                        if (ex.ErrorCode != 183 || i >= 64) throw;
                        closeOtherInstances();
                    }
                }

                Console.WriteLine("Your application is running on: " + baseUri);
                Console.WriteLine("You can type relative URIs to test routes, or enter an empty line to exit");

                var engine = bootstrapper.GetEngine();

                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == "") break;

                    var req = new Request("GET", new Url(baseUri + line));
                    var res = engine.HandleRequest(req).Response;

                    Console.WriteLine("Response Code: " + res.StatusCode);
                    res.Contents(Console.OpenStandardOutput());
                    Console.WriteLine();
                }

                ProcModule.KillAllProcs();
            }
        }

        static void closeOtherInstances()
        {
            new WebClient().DownloadStringAsync(new Uri(baseUri + "/Close"));
            System.Threading.Thread.Sleep(800);
        }
    }
}
