using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class MainWindowViewModel : ViewModelBase {
        private RelayCommand _copyCommand;
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

        public RelayCommand CopyCommand {
            get {
                if (_copyCommand == null)
                    _copyCommand = new RelayCommand((p) => {
                        Clipboard.SetText(SelectedItem.Name);
                    });
                return _copyCommand;
            }
        }
        public List<Computer> Computers { get; set; }
        public ObservableCollection<FileSystemItem> SubItems {
            get {
                if (_subItems == null)
                    _subItems = new ObservableCollection<FileSystemItem>();
                return _subItems;
            }
            set {
                _subItems = value;
                RaiseChange("SubItems");
            }
        }
        public Folder SelectedFolder {
            get { return _selectedFolder; }
            set {
                _selectedFolder = value;
                RaiseChange("SelectedFolder");

                SubItems.Clear();
                if (_selectedFolder.Folders != null)
                    foreach (Folder f in _selectedFolder.Folders)
                        SubItems.Add(f);
                if (_selectedFolder.Files != null)
                    foreach (File f in _selectedFolder.Files)
                        SubItems.Add(f);
            }
        }
        public FileSystemItem SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                RaiseChange("SelectedItem");
            }
        }
    }
}
