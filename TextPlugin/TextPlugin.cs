using PureLib.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model.Plugins;

namespace TextPlugin
{
    [Export(typeof(IPlugin))]
    [ExportMetadata("Extensions", new string[] { ".txt" })]
    class TextPlugin : IPlugin
    {
        public string[] Extensions { get; }

        public bool Loaded
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string GetDescription(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
