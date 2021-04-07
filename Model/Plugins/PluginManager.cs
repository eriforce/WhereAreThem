using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WhereAreThem.Model.Plugins {
    public class PluginManager {
        private Dictionary<string, IPlugin> _plugins = new Dictionary<string, IPlugin>(StringComparer.OrdinalIgnoreCase);

        public PluginManager() {
            Type pluginType = typeof(IPlugin);
            foreach (IPlugin plugin in pluginType.Assembly.GetTypes()
                    .Where(t => pluginType.IsAssignableFrom(t) && t.IsClass)
                    .Select(t => Activator.CreateInstance(t))) {
                if (plugin.Loaded && (plugin.Extensions != null))
                    foreach (string ext in plugin.Extensions) {
                        _plugins.Add(ext, plugin);
                    }
            }
        }

        public Dictionary<string, string> GetDescriptions(string path, bool hasChanged, Dictionary<string, string> descriptions) {
            string ext = Path.GetExtension(path);
            if (!_plugins.ContainsKey(ext))
                return null;

            // TODO: implement plugin invocations
            throw new NotImplementedException();
        }
    }
}
