using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;
using System.Collections.ObjectModel;

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
        private List<FileSystemItem> subItems;
        public List<FileSystemItem> SubItems {
            get {
                return subItems;
            }
            set {
                subItems = value;
                RaiseChange("SubItems");
            }
        }
    }
}
