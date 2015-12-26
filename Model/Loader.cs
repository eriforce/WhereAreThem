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
        private Folder _networkMachine = new Folder() { Folders = new List<Folder>() };
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
            IEnumerable<string> lists = Directory.GetFiles(Path.Combine(_outputPath, machineName), "*.*.{0}".FormatWith(Constant.ListExt))
                .Concat(Directory.GetFiles(_outputPath, "*.*.{0}".FormatWith(Constant.ListExt)));
            return (from file in lists
                    let nameParts = Path.GetFileNameWithoutExtension(file).Split('.')
                    let driveLetter = nameParts.First()
                    let driveType = (DriveType)Enum.Parse(typeof(DriveType), nameParts.Last())
                    select new Drive(machineName) {
                        Name = Drive.GetDriveName(driveLetter, driveType),
                        DriveType = driveType,
                        CreatedDateUtc = new FileInfo(file).LastWriteTimeUtc
                    }).ToList();
        }

        public Drive GetDrive(string machineName, string path) {
            string machinePath = Path.Combine(_outputPath, machineName);
            string driveLetter = Drive.GetDriveLetter(path);
            string listPath = Directory.GetFiles(_outputPath, "{0}.*.{1}".FormatWith(driveLetter, Constant.ListExt)).SingleOrDefault();
            bool isNetwork = !listPath.IsNullOrEmpty();
            if (!isNetwork) {
                listPath = Directory.GetFiles(machinePath, "{0}.*.{1}".FormatWith(driveLetter, Constant.ListExt)).SingleOrDefault();
                if (listPath.IsNullOrEmpty())
                    throw new FileNotFoundException("Drive {0} of {1} cannot be found.".FormatWith(driveLetter, machineName));

                if (!_machineCache.ContainsKey(machineName))
                    _machineCache.Add(machineName, new Folder() { Name = machineName, Folders = new List<Folder>() });
            }

            DriveType driveType = (DriveType)Enum.Parse(typeof(DriveType), listPath.Split('.')[1]);
            Folder machine = isNetwork ? _networkMachine : _machineCache[machineName];
            Folder driveFolder;
            lock (_listLock) {
                DateTime listTimestamp = new FileInfo(listPath).LastWriteTimeUtc;
                string drivePath = Drive.GetDrivePath(driveLetter, driveType, machineName);
                driveFolder = machine.Folders.SingleOrDefault(d => d.NameEquals(drivePath));
                if ((driveFolder == null) || (driveFolder.CreatedDateUtc != listTimestamp)) {
                    if (driveFolder != null)
                        machine.Folders.Remove(driveFolder);
                    driveFolder = Drive.FromFolder(_persistence.Load(listPath), driveType, machineName);
                    driveFolder.Name = Drive.GetDriveName(driveLetter, driveType);
                    driveFolder.CreatedDateUtc = listTimestamp;
                    machine.Folders.Add(driveFolder);
                }
            }
            return (Drive)driveFolder;
        }
    }
}
