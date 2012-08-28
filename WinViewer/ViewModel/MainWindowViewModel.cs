using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;
using WhereAreThem.WinViewer.Model;

namespace WhereAreThem.WinViewer.ViewModel {
    public class MainWindowViewModel : BusyViewModelBase {
        private string _statusBarText;
        private Folder _selectedFolder;
        private FileSystemItem _selectedItem;
        private ICommand _copyCommand;
        private ICommand _openPropertiesCommand;

        public string StatusBarText {
            get { return _statusBarText; }
            set {
                _statusBarText = value;
                RaiseChange(() => StatusBarText);
            }
        }
        public List<Computer> Computers { get; set; }
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

        public event OpeningPropertiesEventHandler OpeningProperties;

        public MainWindowViewModel() {
            Computers = App.Loader.MachineNames.Select(n => new Computer() {
                Name = n,
                Folders = App.Loader.GetDrives(n).Select(
                    d => (Folder)new Drive(n, d.Name, d.CreatedDateUtc)).ToList()
            }).ToList();
        }
    }
}
