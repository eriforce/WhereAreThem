using System;
using System.Collections.Generic;
using System.IO;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Model {
    public class DriveModel : Drive {
        public Computer Computer { get; private set; }
        public bool HasLoaded { get; set; }
        public bool IsChanged { get; set; }

        public bool IsNetworkDrive => DriveType is DriveType.Network or NETWORK_SHARE;
        public bool IsLocalDrive => Computer.IsLocal;

        public event EventHandler LocalDriveLoaded;

        public DriveModel(Computer computer, string driveName, DateTime createdDateUtc, DriveType driveType) : base(computer.Name) {
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
                    if (IsLocalDrive && (LocalDriveLoaded != null))
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
