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
    public class Loader : ILoader {
        private string _path;
        private IPersistence _persistence;
        private Dictionary<string, Folder> _machineCache = new Dictionary<string, Folder>();
        private object _listLock = new object();

        public Loader(string path, IPersistence persistence) {
            _path = path;
            _persistence = persistence;
        }

        public string[] MachineNames {
            get {
                return Directory.GetDirectories(_path).Select(p => Path.GetFileName(p)).ToArray();
            }
        }

        public List<Folder> GetDrives(string machineName) {
            return Directory.GetFiles(Path.Combine(_path, machineName), "*.{0}".FormatWith(Constant.ListExt))
                .Select(p => new Folder() {
                    Name = "{0}{1}{2}".FormatWith(Path.GetFileNameWithoutExtension(p), Path.VolumeSeparatorChar, Path.DirectorySeparatorChar),
                    CreatedDateUtc = new FileInfo(p).LastWriteTimeUtc
                }).ToList();
        }

        public Folder GetDrive(string machineName, string drive) {
            string machinePath = Path.Combine(_path, machineName);
            if (!_machineCache.ContainsKey(machineName) && Directory.Exists(machinePath))
                _machineCache.Add(machineName, new Folder() { Name = machineName, Folders = new List<Folder>() });

            string driveLetter = drive.Substring(0, drive.IndexOf(Path.VolumeSeparatorChar));
            string listFileName = Path.ChangeExtension(driveLetter, Constant.ListExt);
            string listPath = Path.Combine(machinePath, listFileName);

            if (!IO.File.Exists(listPath))
                throw new FileNotFoundException("List {0} cannot be found.".FormatWith(listPath));

            Folder machine = _machineCache[machineName];
            Folder driveFolder;
            lock (_listLock) {
                DateTime listTimestamp = new FileInfo(listPath).LastWriteTimeUtc;
                driveFolder = machine.Folders.SingleOrDefault(d => d.Name == "{0}{1}{2}".FormatWith(
                    driveLetter, Path.VolumeSeparatorChar, Path.DirectorySeparatorChar));
                if ((driveFolder == null) || (driveFolder.CreatedDateUtc != listTimestamp)) {
                    machine.Folders.Remove(driveFolder);
                    driveFolder = _persistence.Load(listPath);
                    driveFolder.CreatedDateUtc = listTimestamp;
                    machine.Folders.Add(driveFolder);
                }
            }

            return driveFolder;
        }
    }
}
