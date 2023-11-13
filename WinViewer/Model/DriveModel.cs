using System;
using System.Collections.Generic;
using System.IO;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class DriveModel : Drive {
        private readonly bool _isLocalDrive;

        public Computer Computer { get; private set; }
        public bool HasLoaded { get; set; }
        public bool IsChanged { get; set; }

        public bool IsNetworkDrive {
            get { return DriveType == DriveType.Network || DriveType == Drive.NETWORK_SHARE; }
        }

        public event EventHandler LocalDriveLoaded;

        public DriveModel(Computer computer, string driveName, DateTime createdDateUtc, DriveType driveType) : base(computer.Name) {
            _isLocalDrive = computer.NameEquals(Environment.MachineName);
            Computer = computer;
            Name = driveName;
            CreatedDateUtc = createdDateUtc;
            DriveType = driveType;

            Folders = new List<Folder> { new Folder { Name = "Loading ..." } };
        }

        public bool Load() {
            try {
                if (!HasLoaded) {
                    Drive drive = App.Loader.GetDrive(Computer.Name, Name, DriveType);
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
            HasLoaded = true;
            CreatedDateUtc = drive.CreatedDateUtc;
            Files = drive.Files;
            Folders = drive.Folders;
            RaiseItemChanges();
        }
    }
}
