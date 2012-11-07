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
        private bool _hasLoaded;

        public event PropertyChangedEventHandler PropertyChanged;

        public DriveModel(string machineName, string driveName, DateTime createdDateUtc, DriveType driveType) {
            _machineName = machineName;
            Name = driveName;
            CreatedDateUtc = createdDateUtc;
            DriveType = driveType;

            Folders = new List<Folder>() { new Folder() { Name = "Loading ..." } };
        }

        public bool Load() {
            try {
                Folder drive = App.Loader.GetDrive(_machineName, Name);
                if (!_hasLoaded || (CreatedDateUtc != drive.CreatedDateUtc)) {
                    _hasLoaded = true;
                    CreatedDateUtc = drive.CreatedDateUtc;
                    Files = drive.Files;
                    Folders = drive.Folders;
                    if (PropertyChanged != null) {
                        PropertyChanged(this, new PropertyChangedEventArgs("Folders"));
                        PropertyChanged(this, new PropertyChangedEventArgs("Items"));
                    }
                }
                return true;
            }
            catch (DirectoryNotFoundException) {
                return false;
            }
            catch (FileNotFoundException) {
                return false;
            }
        }
    }
}
