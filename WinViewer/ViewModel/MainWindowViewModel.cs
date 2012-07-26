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
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class MainWindowViewModel : ViewModelBase {
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
                        const int tryTimesLimit = 3;
                        int tryTimes = 0;
                        while (tryTimes < tryTimesLimit) {
                            try {
                                if (tryTimes > 0)
                                    Thread.Sleep(1000);
                                Clipboard.SetText(SelectedItem.Name);
                                break;
                            }
                            catch (COMException) {
                                tryTimes++;
                                if (tryTimes == tryTimesLimit)
                                    MessageBox.Show(View, "Cannot access the clipboard.");
                            }
                        }
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

        public MainWindowViewModel() {
            Computers = App.Loader.MachineNames.Select(n => new Computer() {
                Name = n,
                Folders = App.Loader.GetDrives(n).Select(
                    d => (Folder)new Drive(n, d.Name, d.CreatedDateUtc)).ToList()
            }).ToList();
        }

        public event OpeningPropertiesEventHandler OpeningProperties;
    }
}
