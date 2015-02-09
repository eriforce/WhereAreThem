using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaInfoWrapper;
using PureLib.Common;

namespace WhereAreThem.Model.Plugins {
    public sealed class MediaInfoPlugin : IPlugin, IDisposable {
        private MediaInfo _mi;

        public string[] Extensions { get; private set; }

        public bool Loaded {
            get { return _mi.Loaded; }
        }

        public MediaInfoPlugin() {
            _mi = new MediaInfo();
            string exts = ConfigurationManager.AppSettings[this.GetType().Name];
            if (!exts.IsNullOrEmpty())
                Extensions = exts.Split('|');
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
