using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class Drive : Folder, INotifyPropertyChanged {
        private static List<Folder> _dummyFolders = new List<Folder>() { new Folder() };
        private string _machineName;

        public Drive(string machineName, string driveName, DateTime createdDateUtc) {
            _machineName = machineName;
            Name = driveName;
            CreatedDateUtc = createdDateUtc;

            Folders = _dummyFolders;
        }

        public void Load() {
            lock (this) {
                if (Folders == _dummyFolders) {
                    Folder drive = App.Loader.GetDrive(_machineName, Name);
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
