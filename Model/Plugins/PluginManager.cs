using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Plugins {
    public class PluginManager {
        private Dictionary<string, IPlugin> _plugins = new Dictionary<string, IPlugin>(StringComparer.OrdinalIgnoreCase);

        public PluginManager() {
            Type pluginType = typeof(IPlugin);
            foreach (IPlugin plugin in pluginType.Assembly.GetTypes()
                    .Where(t => pluginType.IsAssignableFrom(t) && t != pluginType)
                    .Select(t => Activator.CreateInstance(t))) {
                foreach (string ext in plugin.Extensions) {
                    _plugins.Add(ext, plugin);
                }
            }
        }

        public string GetDescription(string path) {
            string ext = Path.GetExtension(path);
            if (!_plugins.ContainsKey(ext))
                return null;

            return _plugins[ext].GetDescription(path);
        }
    }
}
