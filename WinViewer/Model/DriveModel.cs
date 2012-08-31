using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class DriveModel : Drive, INotifyPropertyChanged {
        private string _machineName;

        public bool IsFake { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DriveModel(string machineName, string driveName, DateTime createdDateUtc, DriveType driveType) {
            _machineName = machineName;
            Name = driveName;
            CreatedDateUtc = createdDateUtc;
            DriveType = driveType;

            Folders = new List<Folder>() { new Folder() };
        }

        public void Load() {
            Folder drive = App.Loader.GetDrive(_machineName, Name);
            CreatedDateUtc = drive.CreatedDateUtc;
            Files = drive.Files;
            Folders = drive.Folders;
            IsFake = false;
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("Folders"));
                PropertyChanged(this, new PropertyChangedEventArgs("Items"));
            }
        }
    }
}
