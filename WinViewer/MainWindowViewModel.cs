using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class MainWindowViewModel : ViewModelBase {
        private static Loader _loader = new Loader(ConfigurationManager.AppSettings["path"].WrapPath(),
            Constant.GetPersistence(Type.GetType(ConfigurationManager.AppSettings["persistence"])));

        public string[] Machines {
            get {
                return _loader.MachineNames;
            }
        }
        private string[] _drives;
        public string[] Drives {
            get {
                return _drives;
            }
            set {
                _drives = value;
                RaiseChange("Drives");
            }
        }
        private string _selectedMachine;
        public string SelectedMachine {
            get {
                return _selectedMachine;
            }
            set {
                _selectedMachine = value;
                RaiseChange("SelectedMachine");

                Drives = value.IsNullOrEmpty() ? null :
                    _loader.GetDrives(value).Select(f => f.Name).ToArray();
            }
        }
        private List<Folder> _selectedDrives;
        public List<Folder> SelectedDrives {
            get {
                return _selectedDrives;
            }
            set {
                _selectedDrives = value;
                RaiseChange("SelectedDrives");
            }
        }
        public string SelectedDriveName {
            get {
                if ((SelectedDrives == null) || (SelectedDrives.Count == 0))
                    return null;
                else
                    return _selectedDrives.First().Name;
            }
            set {
                SelectedDrives = new List<Folder>() { _loader.GetDrive(SelectedMachine, value) };
                RaiseChange("SelectedDriveName");
            }
        }
        private ObservableCollection<FileSystemItem> subItems;
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
        private Folder selectedFolder;
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
