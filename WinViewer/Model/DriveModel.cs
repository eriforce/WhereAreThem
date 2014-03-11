using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;

namespace WhereAreThem.WinViewer.Model {
    public class DriveModel : Drive, INotifyPropertyChanged {
        private bool _hasLoaded;
        private bool _isLocalDrive;

        public Computer Computer { get; private set; }
        public bool IsChanged { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler LocalDriveLoaded;

        public DriveModel(Computer computer, string driveName, DateTime createdDateUtc, DriveType driveType) {
            _isLocalDrive = computer.NameEquals(Environment.MachineName);
            Computer = computer;
            Name = driveName;
            CreatedDateUtc = createdDateUtc;
            DriveType = driveType;

            Folders = new List<Folder>() { new Folder() { Name = "Loading ..." } };
        }

        public bool Load() {
            try {
                if (!_hasLoaded) {
                    Drive drive = App.Loader.GetDrive(Computer.Name, Name);
                    Load(drive);
                    if (_isLocalDrive && (LocalDriveLoaded != null))
                        LocalDriveLoaded(this, EventArgs.Empty);
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
