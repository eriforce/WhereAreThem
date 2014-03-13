﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using PureLib.WPF.BusyControl;
using WhereAreThem.Model;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;
using WhereAreThem.WinViewer.Model;
using WatFile = WhereAreThem.Model.Models.File;

namespace WhereAreThem.WinViewer.ViewModel {
    public class MainWindowViewModel : BusyViewModelBase {
        private string _statusBarText;
        private string _location;
        private Folder _selectedFolder;
        private FileSystemItem _selectedItem;
        private ICommand _scanCommand;
        private ICommand _copyCommand;
        private ICommand _openPropertiesCommand;
        private ICommand _openDescriptionCommand;
        private ICommand _goBackCommand;
        private ICommand _goForwardCommand;
        private ICommand _goUpCommand;
        private Computer _localComputer {
            get { return Computers.SingleOrDefault(c => c.NameEquals(Environment.MachineName)); }
        }

        public List<Computer> Computers { get; private set; }
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

                SetStatusBar();
                Location = Path.Combine(SelectedFolderStack.Select(f => f.Name).ToArray());

                if ((Navigation.CurrentEntry == null) || (SelectedFolder != Navigation.CurrentEntry.Stack.Last()))
                    Navigation.AddBackEntry(new ItemEventArgs(null, SelectedFolderStack));
            }
        }
        public FileSystemItem SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                RaiseChange(() => SelectedItem);
            }
        }
        public ICommand ScanCommand {
            get {
                if (_scanCommand == null) {
                    _scanCommand = new RelayCommand(p => {
                        Folder pFolder = (Folder)p;
                        List<Folder> folders = SelectedFolderStack;
                        if (pFolder != SelectedFolder) {
                            folders = new List<Folder>(SelectedFolderStack);
                            folders.Add(pFolder);
                        }

                        string path = Path.Combine(folders.Select(f => f.Name).ToArray());
                        DriveModel drive = folders.GetDrive();
                        if (Directory.Exists(path)) {
                            Scan(path, p is DriveModel, drive);
                        }
                        else {
                            Folder parent = folders.GetParent();
                            parent.Folders.Remove(pFolder);
                            drive.Refresh();
                            drive.IsChanged = true;
                        }
                    }, p => {
                        // p is ensured under the selected folder stack
                        if (p is Computer)
                            return false;
                        else
                            return SelectedFolderStack.GetComputer().NameEquals(Environment.MachineName);
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
                            List<Folder> parentStack = SelectedFolderStack;
                            if (p == SelectedFolder)
                                parentStack = SelectedFolderStack.GetParentStack().ToList();
                            OpeningProperties(this, new ItemEventArgs((FileSystemItem)p, parentStack));
                        }
                    }, p => !(p is Computer));
                return _openPropertiesCommand;
            }
        }
        public ICommand OpenDescriptionCommand {
            get {
                if (_openDescriptionCommand == null)
                    _openDescriptionCommand = new RelayCommand(p => {
                        OpeningDescription(this, new EventArgs<string>(((WatFile)p).Description));
                    }, p => (p is WatFile) && !((WatFile)p).Description.IsNullOrEmpty());
                return _openDescriptionCommand;
            }
        }
        public ICommand GoBackCommand {
            get {
                if (_goBackCommand == null)
                    _goBackCommand = new RelayCommand(p => {
                        List<Folder> next = Navigation.CurrentEntry.Stack;
                        Navigation.GoBack();
                        List<Folder> prev = Navigation.CurrentEntry.Stack;
                        OnLocatingItem(new ItemEventArgs(
                            prev.SequenceEqual(next.GetParentStack()) ? next.Last() : null, prev));
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

        public event ItemEventHandler LocatingItem;
        public event ItemEventHandler OpeningProperties;
        public event EventHandler<EventArgs<string>> OpeningDescription;

        public MainWindowViewModel() {
            Navigation = new ExplorerNavigationService();
            Watcher = new RealtimeWatcher();

            App.Scanner.Scanning += (s, e) => { StatusBarText = e.CurrentDirectory; };
            Computers = App.Loader.MachineNames.Select(n => new Computer() { Name = n }).ToList();
            foreach (Computer c in Computers) {
                c.Folders = App.Loader.GetDrives(c.Name).Select(
                    d => (Folder)new DriveModel(c, d.Name, d.CreatedDateUtc, d.DriveType)).ToList();
            }
            InsertLocalComputer();
        }

        public void Scan(string[] folders) {
            if (folders != null)
                foreach (string path in folders) {
                    string root = Directory.GetDirectoryRoot(path);
                    DriveModel drive = _localComputer.Drives.Single(f => f.NameEquals(root));
                    bool isDrive = root.Equals(path, StringComparison.OrdinalIgnoreCase);
                    if (!isDrive)
                        drive.Load();
                    Scan(path, isDrive, drive);
                }
        }

        public void Save() {
            BusyWith("Saving ...", () => {
                foreach (DriveModel drive in _localComputer.Drives) {
                    if (drive.IsChanged) {
                        StatusBarText = "Saving {0} of {1} ...".FormatWith(drive.Name, Environment.MachineName);
                        App.Scanner.Save(Environment.MachineName, drive);
                    }
                }
            });
        }

        private void InsertLocalComputer() {
            Computer localComputer = _localComputer;
            if (localComputer == null) {
                localComputer = new Computer() { Name = Environment.MachineName, Folders = new List<Folder>() };
                Computers.Add(localComputer);
            }

            DriveType[] driveTypes = ConfigurationManager.AppSettings["driveTypes"]
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => (DriveType)Enum.Parse(typeof(DriveType), s)).ToArray();
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (driveTypes.Contains(drive.DriveType) && !localComputer.Folders.Any(f => f.NameEquals(drive.Name)))
                    localComputer.Folders.Add(new DriveModel(
                        localComputer, drive.Name, DateTime.UtcNow, drive.DriveType) { Folders = null });
            }
            localComputer.Folders.Sort();
            foreach (DriveModel dm in localComputer.Drives) {
                dm.LocalDriveLoaded += (s, e) => {
                    Watcher.WatchDrive((DriveModel)s);
                };
            }
        }

        private void Scan(string path, bool scanDrive, DriveModel drive) {
            string folderName = scanDrive ? path : Path.GetFileName(path);
            BusyWith("Scanning {0} ...".FormatWith(folderName), () => {
                if (scanDrive) {
                    Drive d = App.Scanner.Scan(path);
                    drive.Load(d);
                }
                else {
                    App.Scanner.ScanUpdate(path, drive);
                    drive.Refresh();
                }
                drive.IsChanged = true;
                StatusBarText = "Scanning of {0} has completed.".FormatWith(path);
            });
        }

        private void SetStatusBar() {
            List<string> statusTextParts = new List<string> { SelectedFolder.Size.ToFriendlyString() };
            if (SelectedFolder.Folders != null)
                statusTextParts.Add("{0} folder(s)".FormatWith(SelectedFolder.Folders.Count));
            if (SelectedFolder.Files != null)
                statusTextParts.Add("{0} file(s)".FormatWith(SelectedFolder.Files.Count));
            StatusBarText = string.Join(", ", statusTextParts);
        }

        private void OnLocatingItem(ItemEventArgs e) {
            if (LocatingItem != null)
                LocatingItem(this, e);
        }
    }
}
