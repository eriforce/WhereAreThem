using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class OpeningPropertiesEventArgs : EventArgs {
        public FileSystemItem Item { get; private set; }

        public OpeningPropertiesEventArgs(FileSystemItem item) {
            Item = item;
        }
    }
    public delegate void OpeningPropertiesEventHandler(object sender, OpeningPropertiesEventArgs e);
    
    public class MainWindowViewModel : ViewModelBase {
        private ICommand _copyCommand;
        private ICommand _openPropertiesCommand;
        private string _statusBarText;
        private ObservableCollection<FileSystemItem> _subItems;
        private Folder _selectedFolder;
        private FileSystemItem _selectedItem;

        public MainWindowViewModel() {
            Computers = App.Loader.MachineNames.Select(n => new Computer() {
                Name = n,
                Folders = App.Loader.GetDrives(n).Select(
                    d => (Folder)new Drive(n, d.Name, d.CreatedDateUtc)).ToList()
            }).ToList();
        }

        public event OpeningPropertiesEventHandler OpeningProperties;

        public ICommand CopyCommand {
            get {
                if (_copyCommand == null)
                    _copyCommand = new RelayCommand((p) => {
                        Clipboard.SetText(SelectedItem.Name);
                    }, (p) => { return SelectedItem != null; });
                return _copyCommand;
            }
        }
        public ICommand OpenPropertiesCommand {
            get {
                if (_openPropertiesCommand == null)
                    _openPropertiesCommand = new RelayCommand((p) => {
                        if (OpeningProperties != null)
                            OpeningProperties(this, new OpeningPropertiesEventArgs(SelectedItem));
                    }, (p) => { return SelectedItem != null; });
                return _openPropertiesCommand;
            }
        }
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

                SubItems.Clear();
                List<string> statusTextParts = new List<string>() {
                    Utility.ToFriendlyString(SelectedFolder.Size)
                };
                if (SelectedFolder.Folders != null) {
                    SubItems.AddRange(SelectedFolder.Folders);
                    statusTextParts.Add("{0} folder(s)".FormatWith(SelectedFolder.Folders.Count));
                }
                if (SelectedFolder.Files != null) {
                    SubItems.AddRange(SelectedFolder.Files);
                    statusTextParts.Add("{0} file(s)".FormatWith(SelectedFolder.Files.Count));
                }
                StatusBarText = string.Join(", ", statusTextParts);
            }
        }
        public List<Folder> SelectedFolderStack { get; set; }
        public ObservableCollection<FileSystemItem> SubItems {
            get {
                if (_subItems == null)
                    _subItems = new ObservableCollection<FileSystemItem>();
                return _subItems;
            }
            set {
                _subItems = value;
                RaiseChange(() => SubItems);
            }
        }
        public FileSystemItem SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                RaiseChange(() => SelectedItem);
            }
        }
    }
}
