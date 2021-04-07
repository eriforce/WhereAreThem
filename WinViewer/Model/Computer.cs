using System.Collections.Generic;
using System.Linq;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class Computer : Folder {
        public IEnumerable<DriveModel> Drives {
            get { return Folders.Select(f => (DriveModel)f); }
        }
    }
}
