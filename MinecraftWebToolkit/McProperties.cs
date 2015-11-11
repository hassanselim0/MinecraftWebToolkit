using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WebConfig = System.Web.Configuration.WebConfigurationManager;

namespace MinecraftWebToolkit
{
    // Please excuse my static-ness, I don't like unnecessary Singletons
    public static class McProperties
    {
        private static List<McProperty> properties;
        private static DateTime lastModified;

        private static void load()
        {
            var path = Path.Combine(WebConfig.AppSettings["McServerPath"], "server.properties");

            if (properties != null && lastModified == File.GetLastWriteTimeUtc(path))
                return; // We already loaded the current version

            var lines = File.ReadAllLines(path);

            properties = lines
                .Where(l => l.TrimStart()[0] != '#') // Ignore Comments
                .Select(l =>
                {
                    var p = l.Split('='); // Split the "Key=Value" pair
                    return new McProperty(p[0].Trim(), p[1].Trim());
                }).ToList();

            lastModified = File.GetLastWriteTimeUtc(path);
        }

        private static void save()
        {
            var lines = properties.Select(p => string.Format("{0}={1}", p.Name, p.Value));

            var path = Path.Combine(WebConfig.AppSettings["McServerPath"], "server.properties");
            File.Copy(path, path + "-backup", true);
            File.WriteAllLines(path, lines);

            lastModified = File.GetLastWriteTimeUtc(path);
        }

        public static List<McProperty> GetAll()
        {
            load(); // Ensure the properties are loaded

            return properties.ToList(); // Clone the List
        }

        public static void SetAll(List<McProperty> props)
        {
            properties = props.ToList(); // Clone the List

            save();
        }

        public static string GetValue(string propName)
        {
            load(); // Ensure the properties are loaded

            return properties.Find(p => p.Name == propName).Value; // O(n) ... but it doesn't matter!
        }

        public static void SetValue(string name, string value)
        {
            var prop = properties.Find(p => p.Name == name);

            if (prop == null)
                properties.Add(new McProperty(name, value));
            else
                prop.Value = value;

            save();
        }

        public static void RestoreBackup()
        {
            var path = Path.Combine(WebConfig.AppSettings["McServerPath"], "server.properties");
            File.Copy(path + "-backup", path, true);
        }
    }

    public class McProperty
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public McProperty() { }

        public McProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}