using System;
using System.Collections.Generic;
using System.Configuration;
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
        private PluginManager _pluginManager = new PluginManager();

        public event ScanEventHandler Scanning;
        public event ScanEventHandler Scanned;

        public Scanner(string outputPath, IPersistence persistence)
            : base(outputPath, persistence) {
        }

        public Models.File GetFile(FileInfo fi, Models.File file) {
            return new Models.File() {
                Name = fi.Name,
                FileSize = fi.Length,
                CreatedDateUtc = fi.CreationTimeUtc,
                ModifiedDateUtc = fi.LastWriteTimeUtc,
                Description = GetFileDescription(fi, file)
            };
        }

        public Drive Scan(string drivePath) {
            drivePath = Drive.GetDrivePath(Drive.GetDriveLetter(drivePath));
            Folder driveFolder = GetFolder(new DirectoryInfo(drivePath));
            OnScanned(drivePath);
            Drive drive = Drive.FromFolder(driveFolder, new DriveInfo(driveFolder.Name).DriveType, Environment.MachineName);
            drive.CreatedDateUtc = DateTime.UtcNow;
            return drive;
        }

        public Drive ScanShare(string sharePath) {
            string machineName = Drive.GetMachineName(sharePath);
            Folder driveFolder = GetFolder(new DirectoryInfo(sharePath).Root);
            OnScanned(sharePath);
            Drive drive = Drive.FromFolder(driveFolder, Drive.NETWORK_SHARE, machineName);
            drive.CreatedDateUtc = DateTime.UtcNow;
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
                    Folder current = folder.Folders.SingleOrDefault(f => f.NameEquals(pathParts[i]));
                    if (current == null) {
                        current = GetFolder(new DirectoryInfo(Path.Combine(pathParts.Take(i + 1).ToArray())));
                        folder.Folders.Add(current);
                        folder.Folders.Sort();
                        return;
                    }
                    else
                        folder = current;
                }
                GetFolder(new DirectoryInfo(pathToUpdate), folder);
            }
            finally {
                OnScanned(pathToUpdate);
                drive.CreatedDateUtc = DateTime.UtcNow;
            }
        }

        public void Save(Drive drive) {
            Save(GetListPath(drive.MachineName, drive.DriveLetter, drive.DriveType), drive.ToFolder());
        }

        private void Save(string listPath, Folder drive) {
            string directory = Path.GetDirectoryName(listPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            _persistence.Save(drive, listPath);
            drive.CreatedDateUtc = new FileInfo(listPath).LastWriteTimeUtc;
        }

        private Folder GetFolder(DirectoryInfo directory, Folder folder = null) {
            OnScanning(directory.FullName);
            if (folder == null)
                folder = new Folder() {
                    Name = directory.Name,
                };
            folder.CreatedDateUtc = directory.CreationTimeUtc;

            try {
                folder.Files = (from fi in directory.EnumerateFiles()
                                where fi.ShouldScan()
                                join f in folder.Files ?? new List<Models.File>() on fi.Name equals f.Name into files
                                select GetFile(fi, files.SingleOrDefault())).ToList();
                folder.Files.Sort();
            }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }
            catch (IOException) { }
            if (folder.Files == null)
                folder.Files = new List<Models.File>();

            try {
                folder.Folders = (from di in directory.EnumerateDirectories()
                                  where di.ShouldScan()
                                  join f in folder.Folders ?? new List<Folder>() on di.Name equals f.Name into folders
                                  select GetFolder(di, folders.SingleOrDefault())).ToList();
                folder.Folders.Sort();
            }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }
            catch (IOException) { }
            if (folder.Folders == null)
                folder.Folders = new List<Folder>();

            return folder;
        }

        private string GetFileDescription(FileInfo fi, Models.File file) {
            if ((file == null) || (file.ModifiedDateUtc != fi.LastWriteTimeUtc) || file.Description.IsNullOrEmpty())
                return _pluginManager.GetDescription(fi.FullName);
            else
                return file.Description;
        }

        private void OnScanning(string dir) {
            if (Scanning != null)
                Scanning(this, new ScanEventArgs(dir));
        }

        private void OnScanned(string dir) {
            if (Scanned != null)
                Scanned(this, new ScanEventArgs(dir));
        }
    }

    public static class ScannerExtensions {
        private static readonly FileAttributes _filters;

        static ScannerExtensions() {
            try {
                _filters = ConfigurationManager.AppSettings["scannerFilters"].ToEnum<FileAttributes>().Aggregate((r, a) => r | a);
            }
            catch {
                _filters = (FileAttributes)0;
            }
        }

        public static bool ShouldScan(this FileSystemInfo fsi) {
            return (fsi.Attributes & _filters) == 0;
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
