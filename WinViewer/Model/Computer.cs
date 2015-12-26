using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class Computer : Folder, INotifyPropertyChanged {
        public IEnumerable<DriveModel> Drives {
            get { return Folders.Select(f => (DriveModel)f); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseFolderChanges() {
            Folders = new List<Folder>(Folders);

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Folders"));
        }
    }
}
