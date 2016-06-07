using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Models;

namespace WhereAreThem.Model.Plugins {
    public class PluginManager {
        [ImportMany]
        IEnumerable<Lazy<IPlugin, IPluginMetaData>> _imported_plugins;

        private Dictionary<string, List<Lazy<IPlugin, IPluginMetaData>>> _plugins = new Dictionary<string, List<Lazy<IPlugin, IPluginMetaData>>>(StringComparer.OrdinalIgnoreCase);

        public PluginManager() {
            CompositionContainer container = new CompositionContainer(new DirectoryCatalog("."));
            container.ComposeParts(this);

            foreach (var p in _imported_plugins) {
                foreach (string ext in p.Metadata.Extensions) {
                    if (_plugins.ContainsKey(ext))
                        _plugins[ext].Add(p);
                    else
                        _plugins.Add(ext, new List<Lazy<IPlugin, IPluginMetaData>>() { p });
                }
            }
        }

        public Dictionary<string, string> GetDescriptions(string path, bool hasChanged, Dictionary<string, string> descriptions) {
            string ext = Path.GetExtension(path);

            if (!_plugins.ContainsKey(ext))
                return null;

            Dictionary<string, string> new_descriptions = new Dictionary<string, string>();

            foreach (var p in _plugins[ext]) {
                if (hasChanged || !descriptions.ContainsKey(p.Metadata.ID))
                    new_descriptions.Add(p.Metadata.ID, p.Value.GetDescription(path));
                else
                    new_descriptions.Add(p.Metadata.ID, descriptions[p.Metadata.ID]);
            }

            return new_descriptions;
        }
    }
}
