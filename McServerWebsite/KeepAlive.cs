using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Net;

namespace McServerWebsite
{
    public class KeepAlive
    {
        private static KeepAlive instance;
        private static object sync = new object();

        private KeepAlive()
        {
            instance = this;
        }

        public static bool IsKeepingAlive
        {
            get
            {
                lock (sync)
                {
                    return instance != null;
                }
            }
        }

        public static void Start()
        {
            if (IsKeepingAlive) return;

            lock (sync)
            {
                instance = new KeepAlive();
                instance.Insert();
            }
        }

        public static void Stop()
        {
            lock (sync)
            {
                HttpRuntime.Cache.Remove("KeepAlive");
                instance = null;
            }
        }

        private void Callback(string key, object value, CacheItemRemovedReason reason)
        {
            if (reason != CacheItemRemovedReason.Removed)
            {
                new WebClient().DownloadString("http://mc.hassanselim.me/Home/Ping?useless=" + DateTime.Now.Ticks);

                Insert();
            }
        }

        private void Insert()
        {
            HttpRuntime.Cache.Add("KeepAlive", this, null, Cache.NoAbsoluteExpiration,
                TimeSpan.FromMinutes(20), CacheItemPriority.NotRemovable, Callback);
        }
    }
}