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
        private Dictionary<string, IPlugin> _plugins = new Dictionary<string, IPlugin>(StringComparer.OrdinalIgnoreCase);

        [ImportMany]
        private IEnumerable<IPlugin> plugins;

        public PluginManager() {
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(@"."));
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            foreach (var p in plugins)
            {
                if(p.Extensions != null)
                {
                    foreach (string ext in p.Extensions)
                    {
                        _plugins.Add(ext, p);
                    }
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
