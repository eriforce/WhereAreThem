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
    public class Scanner : ListBase {
        private const FileAttributes filter = FileAttributes.Hidden | FileAttributes.System;
        private readonly string driveSuffix = "{0}{1}".FormatWith(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar);

        public Scanner(string outputPath, IPersistence persistence)
            : base(outputPath, persistence) {
        }

        public Drive Scan(string path) {
            string driveLetter = path.EndsWith(driveSuffix) ? Drive.GetDriveLetter(path) : path;
            Folder driveFolder = GetFolder(new DirectoryInfo("{0}{1}".FormatWith(driveLetter, driveSuffix)));
            Drive drive = Drive.FromFolder(driveFolder, new DriveInfo(driveFolder.Name).DriveType);
            Save(Environment.MachineName, drive);
            return drive;
        }

        public Drive ScanUpdate(string path, DriveType driveType) {
            if (path.IsNullOrEmpty())
                throw new ArgumentNullException("ScanUpdate path is null.");
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException("Path '{0}' cannot be found.".FormatWith(path));
            
            string[] parts = path.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            parts[0] += Path.DirectorySeparatorChar;
            string listPath = GetListPath(Environment.MachineName, Drive.GetDriveLetter(parts[0]), driveType);
            if (!IO.File.Exists(listPath))
                throw new FileNotFoundException("List '{0}' cannot be found.".FormatWith(listPath));

            Drive drive = Drive.FromFolder(_persistence.Load(listPath), driveType);
            UpdateFolder(drive, parts);
            Save(Environment.MachineName, drive);
            return drive;
        }

        public void Save(string machineName, Drive drive) {
            Save(GetListPath(machineName, drive.DriveLetter, drive.DriveType), drive.ToFolder());
        }

        private void Save(string listPath, Folder drive) {
            OnPrintLine("Saving {0} ...".FormatWith(drive.Name));
            string directory = Path.GetDirectoryName(listPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            drive.CreatedDateUtc = DateTime.UtcNow;
            _persistence.Save(drive, listPath);
            OnPrintLine("Scanning of {0} has completed.".FormatWith(drive.Name));
        }

        private void UpdateFolder(Folder root, string[] pathParts) {
            Folder folder = root;
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

        private Folder GetFolder(DirectoryInfo directory) {
            OnPrintLine(directory.FullName);
            Folder folder = new Folder() {
                Name = directory.Name,
                CreatedDateUtc = directory.CreationTimeUtc,
            };
            try {
                folder.Files = directory.GetFiles()
                     .Where(f => !f.Attributes.HasFlag(filter))
                     .Select(f => new Models.File() {
                         Name = f.Name,
                         FileSize = f.Length,
                         CreatedDateUtc = f.CreationTimeUtc,
                         ModifiedDateUtc = f.LastWriteTimeUtc
                     }).ToList();
                folder.Folders = directory.GetDirectories()
                        .Where(d => !d.Attributes.HasFlag(filter))
                        .Select(d => GetFolder(d)).ToList();
                folder.Files.Sort();
                folder.Folders.Sort();
            }
            catch (UnauthorizedAccessException) { }
            return folder;
        }
    }

    public class StringEventArgs : EventArgs {
        public string String { get; set; }
    }

    public delegate void StringEventHandler(object sender, StringEventArgs e);
}
