using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;
using IO = System.IO;

namespace WhereAreThem.Model {
    public class Loader : ListBase {
        private Dictionary<string, Folder> _machineCache = new Dictionary<string, Folder>();
        private object _listLock = new object();

        public Loader(string outputPath, IPersistence persistence)
            : base(outputPath, persistence) {
        }

        public string[] MachineNames {
            get {
                return !Directory.Exists(_outputPath) ? new string[] { } :
                    Directory.GetDirectories(_outputPath).Select(p => Path.GetFileName(p)).ToArray();
            }
        }

        public List<Drive> GetDrives(string machineName) {
            return (from file in Directory.GetFiles(Path.Combine(_outputPath, machineName), "*.*.{0}".FormatWith(Constant.ListExt))
                    let nameParts = Path.GetFileNameWithoutExtension(file).Split('.')
                    let driveLetter = nameParts.First()
                    let driveType = (DriveType)Enum.Parse(typeof(DriveType), nameParts.Last())
                    select new Drive() {
                        Name = Drive.GetDrivePath(driveLetter),
                        DriveType = driveType,
                        CreatedDateUtc = new FileInfo(file).LastWriteTimeUtc
                    }).ToList();
        }

        public Drive GetDrive(string machineName, string path) {
            string machinePath = Path.Combine(_outputPath, machineName);
            if (!_machineCache.ContainsKey(machineName) && Directory.Exists(machinePath))
                _machineCache.Add(machineName, new Folder() { Name = machineName, Folders = new List<Folder>() });

            string driveLetter = Drive.GetDriveLetter(path);
            string listPath = Directory.GetFiles(machinePath, "{0}.*.{1}".FormatWith(driveLetter, Constant.ListExt)).SingleOrDefault();
            if (listPath.IsNullOrEmpty())
                throw new FileNotFoundException("Drive {0} of {1} cannot be found.".FormatWith(driveLetter, machineName));
            DriveType driveType = (DriveType)Enum.Parse(typeof(DriveType), listPath.Split('.')[1]);

            Folder machine = _machineCache[machineName];
            Folder driveFolder;
            lock (_listLock) {
                DateTime listTimestamp = new FileInfo(listPath).LastWriteTimeUtc;
                driveFolder = machine.Folders.SingleOrDefault(d => d.Name == Drive.GetDrivePath(driveLetter));
                if ((driveFolder == null) || (driveFolder.CreatedDateUtc != listTimestamp)) {
                    machine.Folders.Remove(driveFolder);
                    driveFolder = Drive.FromFolder(_persistence.Load(listPath), driveType);
                    driveFolder.CreatedDateUtc = listTimestamp;
                    machine.Folders.Add(driveFolder);
                }
            }
            return (Drive)driveFolder;
        }
    }
}
