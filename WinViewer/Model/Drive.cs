using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class Drive : Folder, INotifyPropertyChanged {
        private string _machineName;

        public bool IsFake { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Drive(string machineName, string driveName, DateTime createdDateUtc) {
            _machineName = machineName;
            Name = driveName;
            CreatedDateUtc = createdDateUtc;

            Folders = new List<Folder>() { new Folder() };
        }

        public void Load() {
            Folder drive = App.Loader.GetDrive(_machineName, Name);
            CreatedDateUtc = drive.CreatedDateUtc;
            Files = drive.Files;
            Folders = drive.Folders;
            IsFake = false;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Folders"));
        }
    }
}
