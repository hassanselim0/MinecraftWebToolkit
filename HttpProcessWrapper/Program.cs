using Nancy;
using Nancy.Hosting.Self;
using System;

namespace HttpProcessWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseUri = new Uri("http://localhost:25564");

            var config = new HostConfiguration();
            //config.UrlReservations.CreateAutomatically = true;
            config.RewriteLocalhost = false;

            var bootstrapper = new Bootstrapper();
            using (var host = new NancyHost(bootstrapper, config, baseUri))
            {
                host.Start();

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
    }
}
