using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhereAreThem.Model.Plugins {
    public interface IPlugin {
        bool Loaded { get; }
        string[] Extensions { get; }
        string GetDescription(string path);
    }
}
