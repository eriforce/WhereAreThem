using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using PureLib.Common;
using WhereAreThem.Model;

namespace WhereAreThem.Viewer.Models {
    public static class List {
        private static string _path {
            get {
                return ConfigurationManager.AppSettings["path"].WrapPath();
            }
        }
        private static IPersistence _persistence {
            get {
                return Constant.GetPersistence(Type.GetType(ConfigurationManager.AppSettings["persistence"]));
            }
        }
        private static Dictionary<string, Folder> _machineCache = new Dictionary<string, Folder>();

        public static string[] MachineNames {
            get {
                return Directory.GetDirectories(_path)
                    .Select(p => Path.GetFileName(p)).ToArray();
            }
        }

        public static List<Folder> GetDrives(string machineName) {
            return Directory.GetFiles(Path.Combine(_path, machineName), "*.{0}".FormatWith(Constant.ListExt))
                .Select(p => new Folder() {
                    Name = "{0}:\\".FormatWith(Path.GetFileNameWithoutExtension(p)),
                    CreatedDateUtc = new FileInfo(p).LastWriteTimeUtc
                }).ToList();
        }

        public static Folder GetDrive(string machineName, string drive) {
            string machinePath = Path.Combine(_path, machineName);
            if (!_machineCache.ContainsKey(machineName) && Directory.Exists(machinePath))
                _machineCache.Add(machineName, new Folder() { Name = machineName, Folders = new List<Folder>() });

            string listFileName = Path.ChangeExtension(drive.Substring(0, drive.IndexOf(':')), Constant.ListExt);
            string listPath = Path.Combine(machinePath, listFileName);
            Folder machine = _machineCache[machineName];
            if (!machine.Folders.Any(d => d.Name == drive) && System.IO.File.Exists(listPath))
                machine.Folders.Add(_persistence.Load(listPath));

            Folder driveFolder = machine.Folders.SingleOrDefault(d => d.Name == drive);
            if (driveFolder == null)
                throw new FileLoadException("File name of '{0}' is incorrect.".FormatWith(listFileName));
            return driveFolder;
        }

        public static Folder GetFolder(string machineName, string path, out List<Folder> stack) {
            stack = null;
            if (path.IsNullOrEmpty())
                return new Folder() {
                    Name = machineName,
                    Folders = List.GetDrives(machineName)
                };
            else {
                string[] parts = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                Folder drive = List.GetDrive(machineName, "{0}\\".FormatWith(parts.First()));
                if (drive == null)
                    return null;

                stack = new List<Folder>();
                stack.Add(drive);
                for (int i = 1; i < parts.Length; i++) {
                    stack.Add(stack.Last().Folders.Single(f => f.Name == parts[i]));
                }
                return stack.Last();
            }
        }
    }
}