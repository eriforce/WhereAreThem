using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PureLib.Common;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;

namespace WhereAreThem.Model {
    public class Loader : ListBase {
        private readonly ConcurrentDictionary<(string MachineName, string DrivePath), Drive> _driveCache = new ConcurrentDictionary<(string, string), Drive>();

        public Loader(string outputPath, IPersistence persistence)
            : base(outputPath, persistence) {
        }

        public SortedSet<string> MachineNames {
            get {
                SortedSet<string> machines = new SortedSet<string>(
                    Directory.GetDirectories(OutputPath).Select(p => Path.GetFileName(p)),
                    StringComparer.OrdinalIgnoreCase);

                machines.Remove(SharedMachineName);
                if (!machines.Contains(Environment.MachineName, StringComparer.OrdinalIgnoreCase))
                    machines.Add(Environment.MachineName);

                return machines;
            }
        }

        public List<Drive> GetDrives(string machineName) {
            IEnumerable<string> lists = Directory.GetFiles(SharedPath, $"*.*.{Constant.ListExt}");

            string machinePath = Path.Combine(OutputPath, machineName);
            if (Directory.Exists(machinePath))
                lists = lists.Concat(Directory.GetFiles(machinePath, $"*.*.{Constant.ListExt}"));

            List<Drive> drives = (from file in lists
                                  let nameParts = Path.GetFileNameWithoutExtension(file).Split('.')
                                  let driveLetter = nameParts.First()
                                  let driveType = (DriveType)Enum.Parse(typeof(DriveType), nameParts.Last())
                                  select new Drive(machineName) {
                                      Name = Drive.GetDriveName(driveLetter, driveType),
                                      DriveType = driveType,
                                      CreatedDateUtc = new FileInfo(file).LastWriteTimeUtc
                                  }).ToList();
            drives.Sort();
            return drives;
        }

        public Drive GetDrive(string machineName, string path) {
            string driveLetter = Drive.GetDriveLetter(path);
            string listPath = Directory.GetFiles(SharedPath, $"{driveLetter}.*.{Constant.ListExt}").SingleOrDefault();
            bool isShared = !listPath.IsNullOrEmpty();
            if (!isShared) {
                string machinePath = Path.Combine(OutputPath, machineName);
                listPath = Directory.GetFiles(machinePath, $"{driveLetter}.*.{Constant.ListExt}").SingleOrDefault();
                if (listPath.IsNullOrEmpty())
                    throw new FileNotFoundException($"Drive {driveLetter} of {machineName} cannot be found.");
            }

            DriveType driveType = (DriveType)Enum.Parse(typeof(DriveType), Path.GetFileName(listPath).Split('.')[1]);
            string drivePath = Drive.GetDrivePath(driveLetter, driveType, machineName);
            DateTime listTimestamp = new FileInfo(listPath).LastWriteTimeUtc;

            var cacheKey = (isShared ? SharedMachineName : machineName, drivePath);
            if (!_driveCache.TryGetValue(cacheKey, out Drive drive) || drive.CreatedDateUtc != listTimestamp) {
                drive = Drive.FromFolder(Persistence.Load(listPath), driveType, machineName);
                drive.Name = Drive.GetDriveName(driveLetter, driveType);
                drive.CreatedDateUtc = listTimestamp;
                _driveCache[cacheKey] = drive;
            }
            return drive;
        }
    }
}
