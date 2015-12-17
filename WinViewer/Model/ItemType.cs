using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhereAreThem.WinViewer.Model {
    public enum ItemType {
        File,
        Folder,
        Computer,
        DriveModel,
        LocalNetworkComputer,
        LocalNetworkShare = 100,

        Removable,
        Fixed,
        Network,
        CDRom,
        Ram,
    }
}
