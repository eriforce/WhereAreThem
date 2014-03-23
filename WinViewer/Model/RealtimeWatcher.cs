using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhereAreThem.Model;
using WhereAreThem.Model.Models;
using WatFile = WhereAreThem.Model.Models.File;

namespace WhereAreThem.WinViewer.Model {
    public class RealtimeWatcher {
        private List<FileSystemWatcher> _watchers;

        public Dictionary<string, DriveModel> Drives { get; private set; }

        public RealtimeWatcher() {
            _watchers = new List<FileSystemWatcher>();
            Drives = new Dictionary<string, DriveModel>(StringComparer.OrdinalIgnoreCase);
        }

        public void WatchDrive(DriveModel dm) {
            if (Drives.ContainsKey(dm.Name))
                return;

            Drives.Add(dm.Name, dm);
            FileSystemWatcher fileWatcher = new FileSystemWatcher(dm.Name);
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            fileWatcher.Created += FileChanged;
            fileWatcher.Changed += FileChanged;
            fileWatcher.Deleted += FileChanged;
            fileWatcher.Renamed += FileChanged;
            fileWatcher.EnableRaisingEvents = true;
            _watchers.Add(fileWatcher);

            FileSystemWatcher folderWatcher = new FileSystemWatcher(dm.Name);
            folderWatcher.IncludeSubdirectories = true;
            folderWatcher.NotifyFilter = NotifyFilters.DirectoryName;
            folderWatcher.Created += FolderChanged;
            folderWatcher.Changed += FolderChanged;
            folderWatcher.Deleted += FolderChanged;
            folderWatcher.Renamed += FolderChanged;
            folderWatcher.EnableRaisingEvents = true;
            _watchers.Add(folderWatcher);

            // NOTE: The changes of hidden attributes are not handled. 
        }

        public void Close() {
            foreach (FileSystemWatcher watcher in _watchers) {
                watcher.Dispose();
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e) {
            DirectoryInfo parent = new DirectoryInfo(Path.GetDirectoryName(e.FullPath));
            if (IsItemInFilteredFolder(parent))
                return;

            DriveModel drive = Drives[parent.Root.Name];
            lock (drive) {
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                        // There may be folder changed events coming in with LastWrite filter
                        IfParentFolderExists(parent, drive, folder => {
                            FileInfo fi = new FileInfo(e.FullPath);
                            if (fi.Exists) {
                                WatFile file = folder.Files.SingleOrDefault(f => f.NameEquals(fi.Name));
                                if (file != null)
                                    folder.Files.Remove(file);
                                if (fi.ShouldScan()) {
                                    folder.Files.Add(App.Scanner.GetFile(fi, file));
                                    folder.Files.Sort();
                                }
                                RaiseChange(drive);
                            }
                        });
                        break;
                    case WatcherChangeTypes.Deleted:
                        IfParentFolderExists(parent, drive, folder => {
                            int removed = folder.Files.RemoveAll(f => f.NameEquals(Path.GetFileName(e.Name)));
                            if (removed > 0)
                                RaiseChange(drive);
                        });
                        break;
                    case WatcherChangeTypes.Renamed:
                        IfParentFolderExists(parent, drive, folder => {
                            RenamedEventArgs rea = (RenamedEventArgs)e;
                            WatFile oldFile = folder.Files.SingleOrDefault(f => f.NameEquals(Path.GetFileName(rea.OldName)));
                            FileInfo fi = new FileInfo(e.FullPath);
                            if (oldFile != null)
                                folder.Files.Remove(oldFile);
                            if (fi.ShouldScan()) {
                                folder.Files.Add(App.Scanner.GetFile(fi, oldFile));
                                folder.Files.Sort();
                            }
                            RaiseChange(drive);
                        });
                        break;
                }
            }
        }

        private void FolderChanged(object sender, FileSystemEventArgs e) {
            DirectoryInfo parent = new DirectoryInfo(Path.GetDirectoryName(e.FullPath));
            if (IsItemInFilteredFolder(parent))
                return;

            DriveModel drive = Drives[parent.Root.Name];
            lock (drive) {
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Created:
                        IfParentFolderExists(parent, drive, parentFolder => {
                            if (new DirectoryInfo(e.FullPath).ShouldScan()) {
                                App.Scanner.ScanUpdate(e.FullPath, drive);
                                RaiseChange(drive);
                            }
                        });
                        break;
                    case WatcherChangeTypes.Deleted:
                        IfParentFolderExists(parent, drive, parentFolder => {
                            int removed = parentFolder.Folders.RemoveAll(f => f.NameEquals(Path.GetFileName(e.Name)));
                            if (removed > 0)
                                RaiseChange(drive);
                        });
                        break;
                    case WatcherChangeTypes.Renamed:
                        IfParentFolderExists(parent, drive, parentFolder => {
                            RenamedEventArgs rea = (RenamedEventArgs)e;
                            Folder oldFolder = parentFolder.Folders.SingleOrDefault(f => f.NameEquals(Path.GetFileName(rea.OldName)));
                            if (new DirectoryInfo(e.FullPath).ShouldScan()) {
                                if (oldFolder == null)
                                    App.Scanner.ScanUpdate(e.FullPath, drive);
                                else
                                    oldFolder.Name = Path.GetFileName(e.Name);
                                RaiseChange(drive);
                            }
                            else if (oldFolder != null) {
                                parentFolder.Folders.Remove(oldFolder);
                                RaiseChange(drive);
                            }
                        });
                        break;
                }
            }
        }

        private bool IsItemInFilteredFolder(DirectoryInfo di) {
            while (di.Parent != null) {
                if (!di.ShouldScan())
                    return true;
                di = di.Parent;
            }
            return false;
        }

        private void IfParentFolderExists(DirectoryInfo parent, DriveModel drive, Action<Folder> ifExists) {
            Folder folder = GetFolder(parent, drive);
            if (folder == null) {
                App.Scanner.ScanUpdate(parent.FullName, drive);
                RaiseChange(drive);
            }
            else {
                ifExists(folder);
            }
        }

        private Folder GetFolder(DirectoryInfo parent, DriveModel drive) {
            string[] folders = parent.FullName.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            Folder folder = drive;
            for (int i = 1; i < folders.Length; i++) {
                folder = folder.Folders.SingleOrDefault(f => f.NameEquals(folders[i]));
                if (folder == null)
                    break;
            }
            return folder;
        }

        private void RaiseChange(DriveModel drive) {
            drive.Refresh();
            drive.IsChanged = true;
        }
    }
}
