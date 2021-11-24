using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhereAreThem.Model.Plugins {
    public class PluginManager {

        public PluginManager() {
            Type pluginType = typeof(IPlugin);
            foreach (IPlugin plugin in pluginType.Assembly.GetTypes()
                    .Where(t => pluginType.IsAssignableFrom(t) && t.IsClass)
                    .Select(t => Activator.CreateInstance(t))) {
                if (plugin.Loaded && (plugin.Extensions != null))
                    foreach (string ext in plugin.Extensions) {
                        //_plugins.Add(ext, plugin);
                    }
            }
        }

        public Dictionary<string, string> GetDescriptions(string path, bool hasFileChnaged, Dictionary<string, string> currentDescriptions) {
            string ext = Path.GetExtension(path);

            return null;
        }
    }
}
