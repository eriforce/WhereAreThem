using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhereAreThem.Model.Plugins {
    public interface IPluginMetaData {
        string ID { get; }
        string[] Extensions { get; }
    }
}
