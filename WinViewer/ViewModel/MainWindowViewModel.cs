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
        private ICommand _scanCommand;
        private ICommand _copyCommand;
        private ICommand _openPropertiesCommand;
        private ICommand _openDescriptionCommand;
        private ICommand _goBackCommand;
        private ICommand _goForwardCommand;
        private ICommand _goUpCommand;
        private ICommand _locateOnDiskCommand;
        private Computer _localComputer {
            get { return Computers.SingleOrDefault(c => c.NameEquals(Environment.MachineName)); }
        }

        public ObservableCollection<Computer> Computers { get; private set; }
        public ObservableCollection<FileSystemItem> SelectedFolderItems { get; private set; }
        public ExplorerNavigationService Navigation { get; private set; }
        public RealtimeWatcher Watcher { get; private set; }
        public List<Folder> SelectedFolderStack { get; set; }

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

                RefreshSelectedFolderItems();
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
        public ICommand ScanCommand {
            get {
                if (_scanCommand == null) {
                    _scanCommand = new RelayCommand(async p => {
                        Folder pFolder = (Folder)p;
                        bool isFromTree = IsTreeItemSelected(pFolder);
                        List<Folder> folders = isFromTree ?
                            SelectedFolderStack : SelectedFolderStack.Concat(new[] { pFolder }).ToList();

                        string path = Path.Combine(folders.Select(f => f.Name).ToArray());
                        DriveModel drive = folders.GetDrive();
                        if (Directory.Exists(path)) {
                            await ScanAsync(path, p is DriveModel, drive, folders.GetComputer());
                            if (isFromTree)
                                RefreshSelectedFolderItems();
                        }
                        else {
                            Folder parent = folders.GetParent();
                            parent.Folders.Remove(pFolder);
                            parent.RaiseItemChanges();
                            if (!isFromTree)
                                RefreshSelectedFolderItems();
                            drive.IsChanged = true;
                        }
                    }, p => {
                        if (!(p is Folder))
                            return false;
                        else if (p is Computer)
                            return false;
                        else if (p is DriveModel && ((DriveModel)p).IsNetworkDrive)
                            return true;
                        else
                            return SelectedFolderStack.GetComputer().NameEquals(Environment.MachineName)
                                || (SelectedFolderStack.GetDrive() != null && SelectedFolderStack.GetDrive().IsNetworkDrive);
                    });
                }
                return _scanCommand;
            }
        }
        public ICommand CopyCommand {
            get {
                if (_copyCommand == null)
                    _copyCommand = new RelayCommand(p => {
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
        public ICommand OpenPropertiesCommand {
            get {
                if (_openPropertiesCommand == null)
                    _openPropertiesCommand = new RelayCommand(p => {
                        if (OpeningProperties != null) {
                            IEnumerable<FileSystemItem> selectedItems = IsTreeItemSelected(p) ?
                                new[] { (FileSystemItem)p } : SelectedItems.AsEnumerable();
                            OpeningProperties(this, new ItemsEventArgs(selectedItems, GetSelectedItemStack(p)));
                        }
                    }, p => !(p is Computer));
                return _openPropertiesCommand;
            }
        }

        public ICommand OpenDescriptionCommand {
            get {
                if (_openDescriptionCommand == null)
                    _openDescriptionCommand = new RelayCommand(
                        p => OpeningDescription(this, new EventArgs<WatFile>((WatFile)p)),
                        p => (p is WatFile) && ((WatFile)p).Data != null);
                return _openDescriptionCommand;
            }
        }
        public ICommand GoBackCommand {
            get {
                if (_goBackCommand == null)
                    _goBackCommand = new RelayCommand(p => {
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
        public ICommand GoForwardCommand {
            get {
                if (_goForwardCommand == null)
                    _goForwardCommand = new RelayCommand(p => {
                        Navigation.GoForward();
                        OnLocatingItem(Navigation.CurrentEntry);
                    }, p => Navigation.CanGoForward);
                return _goForwardCommand;
            }
        }
        public ICommand GoUpCommand {
            get {
                if (_goUpCommand == null)
                    _goUpCommand = new RelayCommand(p => {
                        OnLocatingItem(new ItemEventArgs(SelectedFolder,
                            SelectedFolderStack.GetParentStack().ToList()));
                    }, p => (SelectedFolderStack != null) && !(SelectedFolder is Computer));
                return _goUpCommand;
            }
        }
        public ICommand LocateOnDiskCommand {
            get {
                if (_locateOnDiskCommand == null)
                    _locateOnDiskCommand = new RelayCommand(
                        p => ((FileSystemItem)p).LocateOnDisk(GetSelectedItemStack(p), View),
                        p => SelectedFolderStack.GetComputer().NameEquals(Environment.MachineName));
                return _locateOnDiskCommand;
            }
        }

        public event ItemEventHandler LocatingItem;
        public event ItemsEventHandler OpeningProperties;
        public event EventHandler<EventArgs<WatFile>> OpeningDescription;

        public MainWindowViewModel() {
            SelectedFolderItems = new ObservableCollection<FileSystemItem>();
            SelectedItems = new ObservableCollection<FileSystemItem>();
            Navigation = new ExplorerNavigationService();
            if (bool.Parse(ConfigurationManager.AppSettings["enableWatcher"]))
                Watcher = new RealtimeWatcher();

            App.Scanner.Scanning += (s, e) => {
                StatusBarText = "[Scanning] {0}".FormatWith(e.CurrentDirectory);
            };
            App.Scanner.Scanned += (s, e) => {
                StatusBarText = "[Scanned] {0}".FormatWith(e.CurrentDirectory);
            };
            Computers = new ObservableCollection<Computer>(App.Loader.MachineNames.Select(n => new Computer() { Name = n }));
            foreach (Computer c in Computers) {
                c.Folders = App.Loader.GetDrives(c.Name).Select(
                    d => (Folder)new DriveModel(c, d.Name, d.CreatedDateUtc, d.DriveType)).ToList();
            }
            InsertLocalDrives();
        }

        public async Task ScanAsync(string[] folders) {
            if (folders != null)
                foreach (string path in folders) {
                    DirectoryInfo root = new DirectoryInfo(path).Root;
                    Computer computer;
                    bool isNetworkShare = Drive.IsNetworkPath(path);
                    if (isNetworkShare) {
                        string machineName = Drive.GetMachineName(path);
                        computer = Computers.SingleOrDefault(c => c.NameEquals(machineName));
                        if (computer == null) {
                            computer = new Computer() { Name = machineName };
                            computer.Folders = new List<Folder>();
                            Computers.Add(computer);
                        }
                    }
                    else
                        computer = _localComputer;

                    bool isDrive = root.FullName.Equals(path, StringComparison.OrdinalIgnoreCase);
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
                    if (!isDrive)
                        drive.Load();
                    await ScanAsync(path, isDrive, drive, computer);
                }
        }

        public void Save() {
            BusyWith("Saving ...", Task.Run(() => {
                foreach (Computer computer in Computers) {
                    foreach (DriveModel drive in computer.Drives) {
                        if (drive.IsChanged) {
                            StatusBarText = "Saving {0} of {1} ...".FormatWith(drive.Name, computer.Name);
                            App.Scanner.Save(drive);
                            drive.IsChanged = false;
                        }
                    }
                }
                return true;
            }));
        }

        private void InsertLocalDrives() {
            Computer localComputer = _localComputer;
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

        private async Task ScanAsync(string path, bool scanDrive, DriveModel drive, Computer computer) {
            string folderName = scanDrive ? path : Path.GetFileName(path);
            await BusyWithAsync("Scanning {0} ...".FormatWith(folderName), Task.Run(() => {
                if (scanDrive) {
                    Drive d = App.Scanner.Scan(path);
                    drive.Load(d);
                }
                else {
                    Folder parent = drive.GetDrive(path);
                    App.Scanner.ScanUpdate(path, drive);
                    drive.HasLoaded = true;
                    parent.RaiseItemChanges();
                }
                drive.IsChanged = true;
                return true;
            }));
        }

        private void SetStatusBarOnSelectedFolderChanged() {
            List<string> statusTextParts = new List<string> { SelectedFolder.Size.ToFriendlyString() };
            if (SelectedFolder.Folders != null)
                statusTextParts.Add("{0} folder(s)".FormatWith(SelectedFolder.Folders.Count));
            if (SelectedFolder.Files != null)
                statusTextParts.Add("{0} file(s)".FormatWith(SelectedFolder.Files.Count));
            StatusBarText = string.Join(", ", statusTextParts);
        }

        private bool IsTreeItemSelected(object p) {
            return p == SelectedFolder;
        }

        private List<Folder> GetSelectedItemStack(object p) {
            return IsTreeItemSelected(p) ? SelectedFolderStack.GetParentStack().ToList() : SelectedFolderStack;
        }

        private void RefreshSelectedFolderItems() {
            SelectedFolderItems.Clear();
            if (SelectedFolder.Items != null)
                SelectedFolderItems.AddRange(SelectedFolder.Items);
        }

        private void OnLocatingItem(ItemEventArgs e) {
            if (LocatingItem != null)
                LocatingItem(this, e);
        }
    }
}
