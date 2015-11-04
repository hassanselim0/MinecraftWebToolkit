using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MinecraftWebToolkit.Controllers
{
    [NoCache]
    [Authorize(Roles = "Admin")]
    public class ConnectionController : Controller
    {
        //public ActionResult ListUpnp()
        //{
        //    var upnp = new NATUPNPLib.UPnPNATClass();
        //    if (upnp.StaticPortMappingCollection == null) return Content("UPnP Disabled");

        //    var result = upnp.StaticPortMappingCollection.Cast<NATUPNPLib.IStaticPortMapping>()
        //        .Aggregate("", (s, m) => s + m.Description + "<br/>");

        //    return Content(result);
        //}

        //public ActionResult EnableUpnp()
        //{
        //    var upnp = new NATUPNPLib.UPnPNATClass();
        //    if (upnp.StaticPortMappingCollection == null) return Content("UPnP Disabled");

        //    var localIP = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())
        //        .First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        //    upnp.StaticPortMappingCollection.Add(25565, "TCP", 25565, localIP.ToString(), true, "Minecraft");

        //    return Redirect("/Home/ListUpnp");
        //}

        //public ActionResult DisableUpnp()
        //{
        //    var upnp = new NATUPNPLib.UPnPNATClass();
        //    if (upnp.StaticPortMappingCollection == null) return Content("UPnP Disabled");

        //    upnp.StaticPortMappingCollection.Remove(25565, "TCP");

        //    return Redirect("/Home/ListUpnp");
        //}

        public ActionResult Test()
        {
            var client = new System.Net.Sockets.TcpClient();
            try
            {
                client.Connect("localhost", 25565);
                client.Close();
                return Content("Connection OK " + DateTime.Now.Ticks);
            }
            catch (Exception e)
            {
                return Content("Error:<br />" + e.Message);
            }
        }
    }
}
