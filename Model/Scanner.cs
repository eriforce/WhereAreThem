﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using PureLib.Common;
using WhereAreThem.Model.Models;
using WhereAreThem.Model.Persistences;

namespace WhereAreThem.Model {
    public sealed class Scanner : ListBase {
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
                Data = GetFileDescription(fi, file),
            };
        }

        public Drive Scan(string path) {
            Folder driveFolder;
            DriveType driveType;
            string machineName;

            if (Drive.IsNetworkPath(path)) {
                driveFolder = GetFolder(new DirectoryInfo(path).Root);
                driveType = Drive.NETWORK_SHARE;
                machineName = Drive.GetMachineName(path);
            }
            else {
                string drivePath = Drive.GetDrivePath(Drive.GetDriveLetter(path));
                driveFolder = GetFolder(new DirectoryInfo(drivePath));
                driveType = new DriveInfo(driveFolder.Name).DriveType;
                machineName = Environment.MachineName;
            }

            OnScanned(path);
            Drive drive = Drive.FromFolder(driveFolder, driveType, machineName);
            drive.CreatedDateUtc = DateTime.UtcNow;
            return drive;
        }

        public void ScanUpdate(string pathToUpdate, Drive drive) {
            try {
                string[] pathParts = pathToUpdate.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                bool isNetworkShare = drive.DriveType == Drive.NETWORK_SHARE;
                int firstFolderIndex;
                if (isNetworkShare)
                    firstFolderIndex = 2;
                else {
                    firstFolderIndex = 1;
                    pathParts[0] += Path.DirectorySeparatorChar;
                }
                Folder parent = drive;
                for (int i = firstFolderIndex; i < pathParts.Length; i++) {
                    parent.Folders ??= new List<Folder>();
                    Folder current = parent.Folders.SingleOrDefault(f => f.NameEquals(pathParts[i]));
                    if (current == null) {
                        string path = Path.Combine(pathParts.Take(i + 1).ToArray());
                        if (isNetworkShare)
                            path = Drive.NETWORK_COMPUTER_PREFIX + path;
                        current = GetFolder(new DirectoryInfo(path));
                        parent.Folders.Add(current);
                        parent.Folders.Sort();
                        parent.RaiseItemChanges();
                        return;
                    }
                    else
                        parent = current;
                }
                GetFolder(new DirectoryInfo(pathToUpdate), parent);
                parent.RaiseItemChanges();
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
            Persistence.Save(drive, listPath);
            drive.CreatedDateUtc = new FileInfo(listPath).LastWriteTimeUtc;
        }

        private Folder GetFolder(DirectoryInfo directory, Folder folder = null) {
            OnScanning(directory.FullName);
            folder ??= new Folder() {
                Name = directory.Name,
            };
            folder.CreatedDateUtc = directory.CreationTimeUtc;

            try {
                folder.Files ??= new List<Models.File>();
                folder.Files = (from fi in directory.EnumerateFiles()
                                where fi.ShouldScan()
                                join f in folder.Files on fi.Name equals f.Name into files
                                select GetFile(fi, files.SingleOrDefault())).ToList();
                folder.Files.Sort();
            }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }
            catch (IOException) { }

            try {
                folder.Folders ??= new List<Folder>();
                folder.Folders = (from di in directory.EnumerateDirectories()
                                  where di.ShouldScan()
                                  join f in folder.Folders on di.Name equals f.Name into folders
                                  select GetFolder(di, folders.SingleOrDefault())).ToList();
                folder.Folders.Sort();
            }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }
            catch (IOException) { }

            return folder;
        }

        private Dictionary<string, string> GetFileDescription(FileInfo fi, Models.File file) {
            return null;
        }

        private void OnScanning(string dir) {
            Scanning?.Invoke(this, new ScanEventArgs(dir));
        }

        private void OnScanned(string dir) {
            Scanned?.Invoke(this, new ScanEventArgs(dir));
        }
    }

    public static class ScannerExtensions {
        private static readonly FileAttributes _filters;

        static ScannerExtensions() {
            try {
                _filters = ConfigurationManager.AppSettings["scannerFilters"].ToEnum<FileAttributes>().Aggregate((r, a) => r | a);
            }
            catch {
                _filters = 0;
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
