using System;
using System.Collections.Generic;
using System.Linq;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class Computer : Folder {
        public bool IsLocal => NameEquals(Environment.MachineName);
        public IEnumerable<DriveModel> Drives => Folders.Select(f => (DriveModel)f);
    }
}
