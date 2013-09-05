using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PureLib.Common;

namespace WhereAreThem.Model.Plugins {
    public sealed class MediaInfoPlugin : IPlugin, IDisposable {
        private MediaInfo _mi = new MediaInfo();

        public string[] Extensions { get; private set; }

        public MediaInfoPlugin() {
            string exts = ConfigurationManager.AppSettings[this.GetType().Name];
            Extensions = exts.IsNullOrEmpty() ? null : exts.Split('|');
        }

        public string GetDescription(string path) {
            _mi.Open(path);
            _mi.Option("Complete");
            return _mi.Inform();
        }

        public void Dispose() {
            _mi.Dispose();
        }
    }
}
