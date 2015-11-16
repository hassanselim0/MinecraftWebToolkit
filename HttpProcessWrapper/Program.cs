using System;
using Nancy.Hosting.Self;

namespace HttpProcessWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://localhost:25564");
            
            var config = new HostConfiguration();
            //config.UrlReservations.CreateAutomatically = true;
            config.RewriteLocalhost = false;

            using (var host = new NancyHost(config, uri))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press [Enter] to close the host.");
                while (Console.ReadKey().Key != ConsoleKey.Enter) ;

                ProcModule.KillAllProcs();
            }
        }
    }
}
