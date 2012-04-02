using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class MainWindowViewModel : ViewModelBase {
        public List<Computer> Computers { get; set; }

        public MainWindowViewModel() {
            Computers = App.Loader.MachineNames.Select(n => new Computer() {
                Name = n,
                Folders = App.Loader.GetDrives(n).Select(
                    d => (Folder)new Drive(n, d.Name, d.CreatedDateUtc)).ToList()
            }).ToList();
        }

        private ObservableCollection<FileSystemItem> subItems;
        private Folder selectedFolder;

        public ObservableCollection<FileSystemItem> SubItems {
            get {
                if (subItems == null)
                    subItems = new ObservableCollection<FileSystemItem>();
                return subItems;
            }
            set {
                subItems = value;
                RaiseChange("SubItems");
            }
        }
        public Folder SelectedFolder {
            get {
                return selectedFolder;
            }
            set {
                selectedFolder = value;
                RaiseChange("SelectedFolder");

                SubItems.Clear();
                if (selectedFolder.Folders != null)
                    foreach (Folder f in selectedFolder.Folders)
                        SubItems.Add(f);
                if (selectedFolder.Files != null)
                    foreach (File f in selectedFolder.Files)
                        SubItems.Add(f);
            }
        }
    }
}
