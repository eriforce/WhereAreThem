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

        public bool IsChanged { get; set; }

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
                if (!_hasLoaded) {
                    Drive drive = App.Loader.GetDrive(_machineName, Name);
                    Load(drive);
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

        public void Load(Drive drive) {
            _hasLoaded = true;
            CreatedDateUtc = drive.CreatedDateUtc;
            Files = drive.Files;
            Folders = drive.Folders;
            RaiseChange();
        }

        public void Refresh() {
            _hasLoaded = true;
            Folders = new List<Folder>(Folders);
            RaiseChange();
        }

        private void RaiseChange() {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs("Folders"));
                PropertyChanged(this, new PropertyChangedEventArgs("Items"));
            }
        }
    }
}
