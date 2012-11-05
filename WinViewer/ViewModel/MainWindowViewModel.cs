using System;
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
using IO = System.IO;

namespace WhereAreThem.WinViewer.ViewModel {
    public class MainWindowViewModel : BusyViewModelBase {
        private string _statusBarText;
        private string _location;
        private Folder _selectedFolder;
        private FileSystemItem _selectedItem;
        private ICommand _scanCommand;
        private ICommand _copyCommand;
        private ICommand _openPropertiesCommand;
        private ICommand _goBackCommand;
        private ICommand _goForwardCommand;
        private ICommand _goUpCommand;

        public List<Computer> Computers { get; private set; }
        public ExplorerNavigationService Navigation { get; private set; }

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

                List<string> statusTextParts = new List<string>() {
                    SelectedFolder.Size.ToFriendlyString()
                };
                if (SelectedFolder.Folders != null)
                    statusTextParts.Add("{0} folder(s)".FormatWith(SelectedFolder.Folders.Count));
                if (SelectedFolder.Files != null)
                    statusTextParts.Add("{0} file(s)".FormatWith(SelectedFolder.Files.Count));
                StatusBarText = string.Join(", ", statusTextParts);

                List<Folder> stack = new List<Folder>(SelectedFolderStack);
                stack.Add(SelectedFolder);
                Location = IO.Path.Combine(stack.Select(f => f.Name).ToArray());

                if ((Navigation.CurrentEntry == null) || (SelectedFolder != Navigation.CurrentEntry.Stack.Last()))
                    Navigation.AddBackEntry(new LocatingItemEventArgs(null, stack));
            }
        }
        public List<Folder> SelectedFolderStack { get; set; }
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
                    _scanCommand = new RelayCommand(async (p) => {
                        Folder pFolder = (Folder)p;
                        List<Folder> folders = new List<Folder>(SelectedFolderStack);
                        folders.Add(SelectedFolder);
                        if (pFolder != SelectedFolder)
                            folders.Add(pFolder);

                        await BusyWithAsync("Scanning {0} ...".FormatWith(pFolder.Name), () => {
                            Folder machine = folders[0];
                            DriveModel drive = (DriveModel)folders[1];
                            string path = Path.Combine(folders.Select(f => f.Name).ToArray());
                            if (Directory.Exists(path)) {
                                if (p is DriveModel)
                                    App.Scanner.Scan(path);
                                else
                                    App.Scanner.ScanUpdate(path, drive.DriveType);
                            }
                            else {
                                Folder parent = folders[folders.Count - 2];
                                parent.Folders.Remove(pFolder);
                                App.Scanner.Save(folders.First().Name, drive);
                            }

                            drive.Load();
                        });
                    }, (p) => {
                        if (!(p is Folder) || (p is Computer))
                            return false;
                        if ((p is DriveModel) && (SelectedFolder is Computer))
                            return SelectedFolder.Name == Environment.MachineName;
                        return SelectedFolderStack.Any()
                            && (SelectedFolderStack.First().Name == Environment.MachineName);
                    });
                }
                return _scanCommand;
            }
        }
        public ICommand CopyCommand {
            get {
                if (_copyCommand == null)
                    _copyCommand = new RelayCommand((p) => {
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
                    _openPropertiesCommand = new RelayCommand((p) => {
                        FileSystemItem item = p as FileSystemItem;
                        if (OpeningProperties != null) {
                            List<Folder> stack = new List<Folder>(SelectedFolderStack);
                            if (p != SelectedFolder)
                                stack.Add(SelectedFolder);

                            OpeningProperties(this, new OpeningPropertiesEventArgs(item, stack));
                        }
                    }, (p) => SelectedFolderStack.Any());
                return _openPropertiesCommand;
            }
        }
        public ICommand GoBackCommand {
            get {
                if (_goBackCommand == null)
                    _goBackCommand = new RelayCommand((p) => {
                        Navigation.GoBack();
                        OnNavigatingFolder(Navigation.CurrentEntry);
                    }, (p) => Navigation.CanGoBack);
                return _goBackCommand;
            }
        }
        public ICommand GoForwardCommand {
            get {
                if (_goForwardCommand == null)
                    _goForwardCommand = new RelayCommand((p) => {
                        Navigation.GoForward();
                        OnNavigatingFolder(Navigation.CurrentEntry);
                    }, (p) => Navigation.CanGoForward);
                return _goForwardCommand;
            }
        }
        public ICommand GoUpCommand {
            get {
                if (_goUpCommand == null)
                    _goUpCommand = new RelayCommand((p) => {
                        OnNavigatingFolder(new LocatingItemEventArgs(null, SelectedFolderStack));
                    }, (p) => (SelectedFolderStack != null) && SelectedFolderStack.Any());
                return _goUpCommand;
            }
        }

        public event OpeningPropertiesEventHandler OpeningProperties;
        public event LocatingItemEventHandler NavigatingFolder;

        public MainWindowViewModel() {
            App.Scanner.PrintLine += (s, e) => { StatusBarText = e.String; };
            Computers = App.Loader.MachineNames.Select(n => new Computer() {
                Name = n,
                Folders = App.Loader.GetDrives(n).Select(
                    d => (Folder)new DriveModel(n, d.Name, d.CreatedDateUtc, d.DriveType)).ToList()
            }).ToList();
            InsertLocalComputer();
            Navigation = new ExplorerNavigationService();
        }

        private void InsertLocalComputer() {
            Computer computer = Computers.SingleOrDefault(c => c.Name.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase));
            if (computer == null) {
                computer = new Computer() { Name = Environment.MachineName, Folders = new List<Folder>() };
                Computers.Add(computer);
            }

            DriveType[] driveTypes = ConfigurationManager.AppSettings["driveTypes"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => (DriveType)Enum.Parse(typeof(DriveType), s)).ToArray();
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                if (driveTypes.Contains(drive.DriveType) && !computer.Folders.Any(f => f.Name == drive.Name))
                    computer.Folders.Add(new DriveModel(
                        Environment.MachineName, drive.Name, DateTime.UtcNow, drive.DriveType) { Folders = null });
            }
            computer.Folders.Sort();
        }

        private void OnNavigatingFolder(LocatingItemEventArgs e) {
            if (NavigatingFolder != null)
                NavigatingFolder(this, e);
        }
    }
}
