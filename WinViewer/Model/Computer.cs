using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class Computer : Folder {
        public IEnumerable<DriveModel> Drives {
            get { return Folders.Select(f => (DriveModel)f); }
        }
    }
}
