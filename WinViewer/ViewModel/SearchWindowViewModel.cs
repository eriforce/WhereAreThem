using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using PureLib.WPF.BusyControl;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;
using WhereAreThem.WinViewer.Model;
using IO = System.IO;

namespace WhereAreThem.WinViewer.ViewModel {
    public class SearchWindowViewModel : BusyViewModelBase {
        private SearchResult _selectedSearchResult;
        private ObservableCollection<SearchResult> _results;
        private string _searchPattern;
        private bool _includeFolders = true;
        private bool _includeFiles = true;
        private ICommand _searchCommand;
        private ICommand _locateCommand;
        private ICommand _locateOnDiskCommand;
        private string _location {
            get { return IO.Path.Combine(RootStack.Select(f => f.Name).Union(new string[] { Root.Name }).ToArray()); }
        }

        public Folder Root { get; set; }
        public List<Folder> RootStack { get; set; }
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
        public string WindowTitle {
            get {
                return "Search in {0} of {1}".FormatWith(_location, RootStack.First().Name);
            }
        }
        public ICommand SearchCommand {
            get {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand((p) => {
                        BusyWith("Searching {0} ...".FormatWith(_location), () => {
                            List<SearchResult> results = new List<SearchResult>();
                            SearchInFolder(results, Root, RootStack);
                            Results = new ObservableCollection<SearchResult>(results);
                        });
                    }, (p) => { return !SearchPattern.IsNullOrEmpty() && (IncludeFolders || IncludeFiles); });
                return _searchCommand;
            }
        }
        public ICommand LocateCommand {
            get {
                if (_locateCommand == null)
                    _locateCommand = new RelayCommand((p) => OnLocatingItem());
                return _locateCommand;
            }
        }
        public ICommand LocateOnDiskCommand {
            get {
                if (_locateOnDiskCommand == null)
                    _locateOnDiskCommand = new RelayCommand((p) => {
                        string path = IO.Path.Combine(SelectedSearchResult.ItemPath, SelectedSearchResult.Item.Name);
                        bool itemExists = ((SelectedSearchResult.Item is File) && IO.File.Exists(path))
                            || ((SelectedSearchResult.Item is Folder) && IO.Directory.Exists(path));
                        if (itemExists)
                            Process.Start("explorer.exe", @"/select,{0}".FormatWith(path));
                        else
                            MessageBox.Show(View, "{0} doesn't exist on your disk.".FormatWith(path));
                    }, (p) => { return RootStack.First().Name == Environment.MachineName; });
                return _locateOnDiskCommand;
            }
        }

        public event LocatingItemEventHandler LocatingItem;

        public void RefreshWindowTitle() {
            RaiseChange(() => WindowTitle);
        }

        public void OnLocatingItem() {
            if (LocatingItem != null)
                LocatingItem(this, new LocatingItemEventArgs(SelectedSearchResult.Item, SelectedSearchResult.Stack));
            View.Close();
        }

        private void SearchInFolder(List<SearchResult> results, Folder folder, List<Folder> folderStack) {
            if (IncludeFiles && (folder.Files != null)) {
                List<Folder> stack = new List<Folder>(folderStack);
                stack.Add(folder);
                foreach (File f in folder.Files) {
                    if (Regex.IsMatch(f.Name, SearchPattern.WildcardToRegex(), RegexOptions.IgnoreCase))
                        results.Add(new SearchResult(f, stack));
                }
            }
            if (folder.Folders != null) {
                List<Folder> stack = new List<Folder>(folderStack);
                stack.Add(folder);
                foreach (Folder f in folder.Folders) {
                    if (IncludeFolders && Regex.IsMatch(f.Name, SearchPattern.WildcardToRegex(), RegexOptions.IgnoreCase))
                        results.Add(new SearchResult(f, stack));
                    SearchInFolder(results, f, stack);
                }
            }
        }
    }
}
