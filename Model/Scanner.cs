using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PureLib.Common;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;
using WhereAreThem.Model.Plugins;
using IO = System.IO;

namespace WhereAreThem.Model {
    public class Scanner : ListBase {
        private const FileAttributes filter = FileAttributes.Hidden | FileAttributes.System;
        private readonly string driveSuffix = "{0}{1}".FormatWith(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar);
        private PluginManager _pluginManager = new PluginManager();

        public event ScanEventHandler Scaning;

        public Scanner(string outputPath, IPersistence persistence)
            : base(outputPath, persistence) {
        }

        public Drive Scan(string drivePath) {
            string driveLetter = Drive.GetDriveLetter(drivePath);
            Folder driveFolder = GetFolder(new DirectoryInfo("{0}{1}".FormatWith(driveLetter, driveSuffix)));
            Drive drive = Drive.FromFolder(driveFolder, new DriveInfo(driveFolder.Name).DriveType);
            drive.CreatedDateUtc = DateTime.UtcNow;
            return drive;
        }

        public Drive ScanUpdate(string pathToUpdate, DriveType driveType) {
            if (pathToUpdate.IsNullOrEmpty())
                throw new ArgumentNullException("Parameter pathToUpdate is null.");
            if (!Directory.Exists(pathToUpdate))
                throw new DirectoryNotFoundException("Path '{0}' cannot be found.".FormatWith(pathToUpdate));

            string listPath = GetListPath(Environment.MachineName, Drive.GetDriveLetter(pathToUpdate), driveType);
            if (!IO.File.Exists(listPath))
                throw new FileNotFoundException("List '{0}' cannot be found.".FormatWith(listPath));

            Drive drive = Drive.FromFolder(_persistence.Load(listPath), driveType);
            ScanUpdate(pathToUpdate, drive);
            return drive;
        }

        public void ScanUpdate(string pathToUpdate, Drive drive) {
            try {
                string[] pathParts = pathToUpdate.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                pathParts[0] += Path.DirectorySeparatorChar;
                Folder folder = drive;
                for (int i = 1; i < pathParts.Length; i++) {
                    if (folder.Folders == null)
                        folder.Folders = new List<Folder>();
                    Folder current = folder.Folders.SingleOrDefault(f => f.Name.Equals(pathParts[i], StringComparison.OrdinalIgnoreCase));
                    if (current == null) {
                        current = GetFolder(new DirectoryInfo(Path.Combine(pathParts.Take(i + 1).ToArray())));
                        folder.Folders.Add(current);
                        folder.Folders.Sort();
                        return;
                    }
                    else
                        folder = current;
                }
                Folder newFolder = GetFolder(new DirectoryInfo(Path.Combine(pathParts)));
                folder.CreatedDateUtc = newFolder.CreatedDateUtc;
                folder.Folders = newFolder.Folders;
                folder.Files = newFolder.Files;
            }
            finally {
                drive.CreatedDateUtc = DateTime.UtcNow;
            }
        }

        public void Save(string machineName, Drive drive) {
            Save(GetListPath(machineName, drive.DriveLetter, drive.DriveType), drive.ToFolder());
        }

        private void Save(string listPath, Folder drive) {
            string directory = Path.GetDirectoryName(listPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            _persistence.Save(drive, listPath);
            drive.CreatedDateUtc = new FileInfo(listPath).LastWriteTimeUtc;
        }

        private Folder GetFolder(DirectoryInfo directory) {
            OnScaning(directory.FullName);
            Folder folder = new Folder() {
                Name = directory.Name,
                CreatedDateUtc = directory.CreationTimeUtc,
            };
            try {
                folder.Files = (from fi in directory.EnumerateFiles()
                                where !fi.Attributes.HasFlag(filter)
                                join f in folder.Files ?? new List<Models.File>() on fi.Name equals f.Name into files
                                select new Models.File() {
                                    Name = fi.Name,
                                    FileSize = fi.Length,
                                    CreatedDateUtc = fi.CreationTimeUtc,
                                    ModifiedDateUtc = fi.LastWriteTimeUtc,
                                    Description = GetFileDescription(fi, files.SingleOrDefault())
                                }).ToList();
                folder.Folders = (from di in directory.EnumerateDirectories()
                                  where !di.Attributes.HasFlag(filter)
                                  select GetFolder(di)).ToList();
                folder.Files.Sort();
                folder.Folders.Sort();
            }
            catch (UnauthorizedAccessException) { } // no permission
            catch (DirectoryNotFoundException) { }  // broken junction
            return folder;
        }

        private string GetFileDescription(FileInfo fi, Models.File file) {
            if ((file == null) || (file.ModifiedDateUtc != fi.LastWriteTimeUtc))
                return _pluginManager.GetDescription(fi.FullName);
            else
                return file.Description;
        }

        private void OnScaning(string dir) {
            if (Scaning != null)
                Scaning(this, new ScanEventArgs(dir));
        }
    }

    public class ScanEventArgs : EventArgs {
        public string CurrentDirectory { get; private set; }

        public ScanEventArgs(string currentDirectory) {
            CurrentDirectory = currentDirectory;
        }
    }

    public delegate void ScanEventHandler(object sender, ScanEventArgs e);
}
