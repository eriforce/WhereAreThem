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
        private IEnumerable<Lazy<IPlugin, IPluginDescription>> _plugins;

        public PluginManager() {
            var container = new CompositionContainer(new DirectoryCatalog(@"."));
            container.ComposeParts(this);
        }

        public string GetDescription(string path) {
            string ext = Path.GetExtension(path);
            var plugin = _plugins.Where(p => p.Metadata.Extensions.Contains(ext)).SingleOrDefault();
            if (plugin != null)
                return plugin.Value.GetDescription(path);
            else
                return null;
        }
    }
}
