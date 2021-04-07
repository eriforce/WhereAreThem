using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using PureLib.WPF.BusyControl;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;
using WhereAreThem.WinViewer.Model;

namespace WhereAreThem.WinViewer.ViewModel {
    public class SearchWindowViewModel : BusyViewModelBase {
        private Folder _root;
        private List<Folder> _rootStack;
        private SearchResult _selectedSearchResult;
        private ObservableCollection<SearchResult> _results;
        private string _searchPattern;
        private string _statusBarText;
        private bool _includeFolders = true;
        private bool _includeFiles = true;
        private string _location;
        private string _machineName;
        private ICommand _searchCommand;
        private ICommand _locateCommand;
        private ICommand _locateOnDiskCommand;
        private ICommand _openPropertiesCommand;

        public Folder Root {
            get { return _root; }
            set {
                _root = value;
                RaiseChange(() => Root);
            }
        }
        public List<Folder> RootStack {
            get { return _rootStack; }
            set {
                _rootStack = value;
                RaiseChange(() => RootStack);

                MachineName = RootStack.First().Name;
            }
        }
        public SearchResult SelectedSearchResult {
            get { return _selectedSearchResult; }
            set {
                _selectedSearchResult = value;
                RaiseChange(() => SelectedSearchResult);
            }
        }
        public ObservableCollection<SearchResult> Results {
            get { return _results; }
            set {
                _results = value;
                RaiseChange(() => Results);
            }
        }
        public string SearchPattern {
            get { return _searchPattern; }
            set {
                _searchPattern = value;
                RaiseChange(() => SearchPattern);
            }
        }
        public string StatusBarText {
            get { return _statusBarText; }
            set {
                _statusBarText = value;
                RaiseChange(() => StatusBarText);
            }
        }
        public bool IncludeFolders {
            get { return _includeFolders; }
            set {
                _includeFolders = value;
                RaiseChange(() => IncludeFolders);
            }
        }
        public bool IncludeFiles {
            get { return _includeFiles; }
            set {
                _includeFiles = value;
                RaiseChange(() => IncludeFiles);
            }
        }
        public string Location {
            get { return _location; }
            set {
                _location = value;
                RaiseChange(() => Location);
            }
        }
        public string MachineName {
            get { return _machineName; }
            set {
                _machineName = value;
                RaiseChange(() => MachineName);
            }
        }
        public ICommand SearchCommand {
            get {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand(p => {
                        BusyWith("Searching {0} ...".FormatWith(Location), Task.Run(() => {
                            Results = new ObservableCollection<SearchResult>(
                                Root.Search(RootStack.GetParentStack().ToList(), SearchPattern, IncludeFiles, IncludeFolders));

                            List<string> statusTextParts = new List<string>();
                            if (Results.Any(r => r.Item is Folder))
                                statusTextParts.Add("{0} folder(s)".FormatWith(Results.Count(r => r.Item is Folder)));
                            if (Results.Any(r => r.Item is File)) {
                                statusTextParts.Add("{0} file(s)".FormatWith(Results.Count(r => r.Item is File)));
                                statusTextParts.Add(Results.Sum(r => r.Item.Size).ToFriendlyString());
                            }
                            StatusBarText = string.Join(", ", statusTextParts);
                            return true;
                        }));
                    }, p => !SearchPattern.IsNullOrEmpty() && (IncludeFolders || IncludeFiles));
                return _searchCommand;
            }
        }
        public ICommand LocateCommand {
            get {
                if (_locateCommand == null)
                    _locateCommand = new RelayCommand(p => OnLocatingItem());
                return _locateCommand;
            }
        }
        public ICommand LocateOnDiskCommand {
            get {
                if (_locateOnDiskCommand == null)
                    _locateOnDiskCommand = new RelayCommand(
                        p => SelectedSearchResult.Item.LocateOnDisk(SelectedSearchResult.Stack, View),
                        p => RootStack.GetComputer().NameEquals(Environment.MachineName));
                return _locateOnDiskCommand;
            }
        }
        public ICommand OpenPropertiesCommand {
            get {
                if (_openPropertiesCommand == null)
                    _openPropertiesCommand = new RelayCommand(p => OnOpeningProperties());
                return _openPropertiesCommand;
            }
        }

        public event ItemEventHandler LocatingItem;
        public event ItemsEventHandler OpeningProperties;

        public void OnLocatingItem() {
            if (LocatingItem != null)
                LocatingItem(this, new ItemEventArgs(SelectedSearchResult.Item, SelectedSearchResult.Stack));
            View.Close();
        }

        public void OnOpeningProperties() {
            if (OpeningProperties != null)
                OpeningProperties(this, new ItemsEventArgs(new[] { SelectedSearchResult.Item }, SelectedSearchResult.Stack));
        }
    }
}
