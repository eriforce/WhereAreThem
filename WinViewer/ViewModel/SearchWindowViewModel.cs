using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;
using IO = System.IO;

namespace WhereAreThem.WinViewer {
    public class SearchWindowViewModel : ViewModelBase {
        private RelayCommand _searchCommand;
        private ObservableCollection<SearchResult> _results;
        private string _searchPattern;

        public SearchWindowViewModel() {
            Results = new ObservableCollection<SearchResult>();
        }

        public Folder Root { get; set; }
        public List<Folder> RootStack { get; set; }
        public string WindowTitle {
            get {
                return "Search {0} in {1}".FormatWith(Root.Name, IO.Path.Combine(RootStack.Select(f => f.Name).ToArray()));
            }
        }
        public RelayCommand SearchCommand {
            get {
                if (_searchCommand == null)
                    _searchCommand = new RelayCommand((p) => {
                        Results.Clear();
                        SearchInFolder(Root, RootStack);
                    }, (p) => { return !SearchPattern.IsNullOrEmpty(); });
                return _searchCommand;
            }
        }
        public ObservableCollection<SearchResult> Results {
            get { return _results; }
            set {
                _results = value;
                RaiseChange("Results");
            }
        }
        public string SearchPattern {
            get { return _searchPattern; }
            set {
                _searchPattern = value;
                RaiseChange("SearchPattern");
            }
        }

        public void RefreshWindowTitle() {
            RaiseChange("WindowTitle");
        }

        private void SearchInFolder(Folder folder, List<Folder> folderStack) {
            if (folder.Files != null) {
                List<Folder> stack = new List<Folder>(folderStack);
                stack.Add(folder);
                foreach (File f in folder.Files) {
                    if (Regex.IsMatch(f.Name, SearchPattern.WildcardToRegex(), RegexOptions.IgnoreCase))
                        Results.Add(new SearchResult(f, stack));
                }
            }
            if (folder.Folders != null) {
                List<Folder> stack = new List<Folder>(folderStack);
                stack.Add(folder);
                foreach (Folder f in folder.Folders) {
                    if (Regex.IsMatch(f.Name, SearchPattern.WildcardToRegex(), RegexOptions.IgnoreCase))
                        Results.Add(new SearchResult(f, stack));
                    SearchInFolder(f, stack);
                }
            }
        }
    }
}
