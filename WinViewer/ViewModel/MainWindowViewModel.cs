using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using PureLib.WPF.BusyControl;
using PureLib.WPF.Command;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;
using WhereAreThem.WinViewer.Model;
using WatFile = WhereAreThem.Model.Models.File;

namespace WhereAreThem.WinViewer.ViewModel {
    public class MainWindowViewModel : BusyViewModelBase {
        private string _statusBarText;
        private string _location;
        private Folder _selectedFolder;
        private ObservableCollection<FileSystemItem> _selectedItems;
        private RelayCommand _scanCommand;
        private RelayCommand _copyCommand;
        private RelayCommand _openPropertiesCommand;
        private RelayCommand _openDescriptionCommand;
        private RelayCommand _goBackCommand;
        private RelayCommand _goForwardCommand;
        private RelayCommand _goUpCommand;
        private RelayCommand _locateOnDiskCommand;
        private RelayCommand _excludeCommand;

        public ObservableCollection<Computer> Computers { get; private set; }
        public ObservableCollection<FileSystemItem> ItemsInSelectedFolder { get; private set; }
        public ExplorerNavigationService Navigation { get; private set; }
        public RealtimeWatcher Watcher { get; private set; }
        public List<Folder> SelectedFolderStack { get; set; }
        private Computer LocalComputer {
            get { return Computers.SingleOrDefault(c => c.NameEquals(Environment.MachineName)); }
        }

        public string StatusBarText {
            get { return _statusBarText; }
            set {
                _statusBarText = value;
                RaiseChange(() => StatusBarText);
            }
        }
        public string Location {
            get { return _location; }
            set {
                _location = value;
                RaiseChange(() => Location);
            }
        }
        public Folder SelectedFolder {
            get { return _selectedFolder; }
            set {
                _selectedFolder = value;
                RaiseChange(() => SelectedFolder);

                RefreshItemsInSelectedFolder();
                SetStatusBarOnSelectedFolderChanged();
                Location = Path.Combine(SelectedFolderStack.Select(f => f.Name).ToArray());

                if ((Navigation.CurrentEntry == null) || (SelectedFolder != Navigation.CurrentEntry.Stack.Last()))
                    Navigation.AddEntry(new ItemEventArgs(null, SelectedFolderStack));
            }
        }

        public ObservableCollection<FileSystemItem> SelectedItems {
            get { return _selectedItems; }
            set {
                _selectedItems = value;
                RaiseChange(() => SelectedItems);
            }
        }
        public RelayCommand ScanCommand {
            get {
                _scanCommand ??= new(p => {
                    Folder pFolder = (Folder)p;
                    bool isFromTree = IsTreeItemSelected(pFolder);
                    List<Folder> folderStack = isFromTree ?
                        SelectedFolderStack : SelectedFolderStack.Concat(new[] { pFolder }).ToList();

                    string path = Path.Combine(folderStack.Select(f => f.Name).ToArray());
                    DriveModel drive = folderStack.GetDrive();
                    if (Directory.Exists(path)) {
                        Scan(path, p is DriveModel, drive);
                        if (isFromTree)
                            RefreshItemsInSelectedFolder();
                    }
                    else {
                        Folder parent = folderStack.GetParent();
                        parent.Folders.Remove(pFolder);
                        parent.RaiseItemChanges();
                        if (!isFromTree)
                            RefreshItemsInSelectedFolder();
                        drive.IsChanged = true;
                    }
                }, p => {
                    if (p is not Folder)
                        return false;
                    else if (p is Computer)
                        return false;
                    else if (p is DriveModel && ((DriveModel)p).IsNetworkDrive)
                        return true;
                    else
                        return SelectedFolderStack.GetComputer().NameEquals(Environment.MachineName)
                            || (SelectedFolderStack.GetDrive() != null && SelectedFolderStack.GetDrive().IsNetworkDrive);
                });
                return _scanCommand;
            }
        }
        public RelayCommand CopyCommand {
            get {
                _copyCommand ??= new(p => {
                    FileSystemItem item = p as FileSystemItem;
                    try {
                        Clipboard.SetText(item.Name);
                    }
                    catch (COMException) {
                        MessageBox.Show(View, "Cannot access the clipboard.");
                    }
                });
                return _copyCommand;
            }
        }
        public RelayCommand OpenPropertiesCommand {
            get {
                _openPropertiesCommand ??= new(p => {
                    if (OpeningProperties != null) {
                        IEnumerable<FileSystemItem> selectedItems = IsTreeItemSelected(p) ?
                            new[] { (FileSystemItem)p } : SelectedItems.AsEnumerable();
                        OpeningProperties(this, new ItemsEventArgs(selectedItems, GetSelectedItemStack(p)));
                    }
                }, p => p is not Computer);
                return _openPropertiesCommand;
            }
        }

        public RelayCommand OpenDescriptionCommand {
            get {
                _openDescriptionCommand ??= new(
                        p => OpeningDescription(this, new EventArgs<WatFile>((WatFile)p)),
                        p => (p is WatFile file) && file.Data != null);
                return _openDescriptionCommand;
            }
        }
        public RelayCommand GoBackCommand {
            get {
                _goBackCommand ??= new(p => {
                    List<Folder> current = Navigation.CurrentEntry.Stack;
                    Navigation.GoBack();
                    while (!Navigation.CurrentEntry.Stack.Exists()) {
                        Navigation.RemoveCurrentEntry();
                    }
                    List<Folder> prev = Navigation.CurrentEntry.Stack;
                    OnLocatingItem(new ItemEventArgs(
                        prev.SequenceEqual(current.GetParentStack()) ? current.Last() : null, prev));
                }, p => Navigation.CanGoBack);
                return _goBackCommand;
            }
        }
        public RelayCommand GoForwardCommand {
            get {
                _goForwardCommand ??= new(p => {
                    Navigation.GoForward();
                    OnLocatingItem(Navigation.CurrentEntry);
                }, p => Navigation.CanGoForward);
                return _goForwardCommand;
            }
        }
        public RelayCommand GoUpCommand {
            get {
                _goUpCommand ??= new(p => {
                    OnLocatingItem(new ItemEventArgs(SelectedFolder,
                        SelectedFolderStack.GetParentStack().ToList()));
                }, p => (SelectedFolderStack != null) && SelectedFolder is not Computer);
                return _goUpCommand;
            }
        }
        public RelayCommand LocateOnDiskCommand {
            get {
                _locateOnDiskCommand ??= new(
                        p => ((FileSystemItem)p).LocateOnDisk(GetSelectedItemStack(p), View),
                        p => SelectedFolderStack.GetComputer().NameEquals(Environment.MachineName));
                return _locateOnDiskCommand;
            }
        }
        public RelayCommand ExcludeCommand {
            get {
                _excludeCommand ??= new(p => {
                    foreach (FileSystemItem item in SelectedItems) {
                        if (item is Folder)
                            SelectedFolder.Folders.Remove((Folder)item);
                        else
                            SelectedFolder.Files.Remove((WatFile)item);
                    }

                    SelectedFolder.RaiseItemChanges();
                    SelectedFolderStack.GetDrive().IsChanged = true;

                    RefreshItemsInSelectedFolder();
                }, p => SelectedFolder is not Computer);
                return _excludeCommand;
            }
        }

        public event ItemEventHandler LocatingItem;
        public event ItemsEventHandler OpeningProperties;
        public event EventHandler<EventArgs<WatFile>> OpeningDescription;

        public MainWindowViewModel() {
            ItemsInSelectedFolder = new ObservableCollection<FileSystemItem>();
            SelectedItems = new ObservableCollection<FileSystemItem>();
            Navigation = new ExplorerNavigationService();
            if (bool.Parse(ConfigurationManager.AppSettings["enableWatcher"]))
                Watcher = new RealtimeWatcher();

            App.Scanner.Scanning += (s, e) => {
                StatusBarText = $"[Scanning] {e.CurrentDirectory}";
            };
            App.Scanner.Scanned += (s, e) => {
                StatusBarText = $"[Scanned] {e.CurrentDirectory}";
            };
            Computers = new ObservableCollection<Computer>(App.Loader.MachineNames.Select(n => new Computer() { Name = n }));
            foreach (Computer c in Computers) {
                c.Folders = App.Loader.GetDrives(c.Name).Select(
                    d => (Folder)new DriveModel(c, d.Name, d.CreatedDateUtc, d.DriveType)).ToList();
            }
            InsertLocalDrives();
        }

        public bool Scan(string[] folders) {
            if (folders == null)
                return false;

            foreach (string path in folders) {
                Computer computer;
                bool isNetworkShare = Drive.IsNetworkPath(path);
                if (isNetworkShare) {
                    string machineName = Drive.GetMachineName(path);
                    computer = Computers.SingleOrDefault(c => c.NameEquals(machineName));
                    if (computer == null) {
                        computer = new Computer {
                            Name = machineName,
                            Folders = new List<Folder>()
                        };
                        Computers.Add(computer);
                    }
                }
                else
                    computer = LocalComputer;

                DirectoryInfo root = new DirectoryInfo(path).Root;
                DriveModel drive = computer.Drives.SingleOrDefault(f => f.NameEquals(root.Name));
                bool isNew = drive == null;
                if (isNew) {
                    string name;
                    DriveType driveType;
                    if (isNetworkShare) {
                        name = Drive.GetDriveLetter(path);
                        driveType = Drive.NETWORK_SHARE;
                    }
                    else {
                        name = root.Name;
                        driveType = DriveType.Network;
                    }
                    drive = new DriveModel(computer, name, DateTime.UtcNow, driveType);
                    drive.Folders.Clear();
                    computer.Folders.Add(drive);
                    computer.Folders = new List<Folder>(computer.Folders);
                    computer.RaiseItemChanges();
                }
                bool isDrive = root.FullName.Equals(path, StringComparison.OrdinalIgnoreCase);
                Scan(path, isDrive, drive);
            }
            return true;
        }

        public void Save() {
            BusyWith("Saving ...", Task.Run(() => {
                foreach (Computer computer in Computers) {
                    foreach (DriveModel drive in computer.Drives) {
                        if (drive.IsChanged) {
                            StatusBarText = $"Saving {drive.Name} of {computer.Name} ...";
                            App.Scanner.Save(drive);
                            drive.IsChanged = false;
                        }
                    }
                }
                return true;
            }));
        }

        private void InsertLocalDrives() {
            Computer localComputer = LocalComputer;
            DriveType[] driveTypes = ConfigurationManager.AppSettings["driveTypes"].ToEnum<DriveType>();
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (driveTypes.Contains(drive.DriveType) && !localComputer.Folders.Any(f => f.NameEquals(drive.Name)))
                    localComputer.Folders.Add(new DriveModel(
                        localComputer, drive.Name, DateTime.UtcNow, drive.DriveType) { Folders = null });
            }
            localComputer.Folders.Sort();
            if (Watcher != null)
                foreach (DriveModel dm in localComputer.Drives) {
                    dm.LocalDriveLoaded += (s, e) => {
                        Watcher.WatchDrive((DriveModel)s);
                    };
                }
        }

        private void Scan(string path, bool scanDrive, DriveModel drive) {
            string folderName = scanDrive ? path : Path.GetFileName(path);
            BusyWith($"Scanning {folderName} ...", Task.Run(() => {
                if (scanDrive) {
                    Drive d = App.Scanner.Scan(path);
                    drive.Load(d);
                }
                else {
                    drive.Load();
                    App.Scanner.ScanUpdate(path, drive);
                    drive.HasLoaded = true;
                }
                drive.IsChanged = true;
                return true;
            }));
        }

        private void SetStatusBarOnSelectedFolderChanged() {
            List<string> statusTextParts = new() { SelectedFolder.Size.ToFriendlyString() };
            if (SelectedFolder.Folders != null)
                statusTextParts.Add($"{SelectedFolder.Folders.Count} folder(s)");
            if (SelectedFolder.Files != null)
                statusTextParts.Add($"{SelectedFolder.Files.Count} file(s)");
            StatusBarText = string.Join(", ", statusTextParts);
        }

        private bool IsTreeItemSelected(object p) {
            return p == SelectedFolder;
        }

        private List<Folder> GetSelectedItemStack(object p) {
            return IsTreeItemSelected(p) ? SelectedFolderStack.GetParentStack().ToList() : SelectedFolderStack;
        }

        private void RefreshItemsInSelectedFolder() {
            ItemsInSelectedFolder.Clear();
            if (SelectedFolder.Items != null)
                ItemsInSelectedFolder.AddRange(SelectedFolder.Items);
        }

        private void OnLocatingItem(ItemEventArgs e) {
            LocatingItem?.Invoke(this, e);
        }
    }
}
