using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer.Model {
    public class Drive : Folder, INotifyPropertyChanged {
        private static List<Folder> _dummyFolders = new List<Folder>() { new Folder() };
        private string machineName;

        public Drive(string machineName, string driveName, DateTime createdDateUtc) {
            this.machineName = machineName;
            this.Name = driveName;
            this.CreatedDateUtc = createdDateUtc;

            Folders = _dummyFolders;
        }

        public void Load() {
            lock (this) {
                if (Folders == _dummyFolders) {
                    Folder drive = App.Loader.GetDrive(machineName, Name);
                    CreatedDateUtc = drive.CreatedDateUtc;
                    Files = drive.Files;
                    Folders = drive.Folders;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Folders"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
