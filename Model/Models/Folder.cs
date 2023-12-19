using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using PureLib.Common;

namespace WhereAreThem.Model.Models {
    [DataContract]
    public class Folder : FileSystemItem, INotifyPropertyChanged {
        private IEnumerable<FileSystemItem> _items;

        public List<Folder> Folders { get; set; }
        public List<File> Files { get; set; }

        public IEnumerable<FileSystemItem> Items {
            get {
                if (_items == null) {
                    if ((Folders != null) && (Files != null))
                        _items = Folders.Concat<FileSystemItem>(Files);
                    else if (Folders != null)
                        _items = Folders;
                    else if (Files != null)
                        _items = Files;
                }
                return _items;
            }
        }
        public override long Size {
            get {
                long size = 0;
                if (Files != null)
                    try {
                        size += Files.Sum(f => f.Size);
                    }
                    catch (InvalidOperationException) { }
                if (Folders != null)
                    try {
                        size += Folders.Sum(f => f.Size);
                    }
                    catch (InvalidOperationException) { }
                return size;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseItemChanges() {
            if (Folders != null)
                Folders = new List<Folder>(Folders);
            if (Files != null)
                Files = new List<File>(Files);
            _items = null;

            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Folders)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Files)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Items)));
            }
        }

        public List<SearchResult> Search(List<Folder> parentStack, string pattern, bool includeFile, bool includeFolder) {
            if (!includeFile && !includeFolder)
                return null;

            string regexPattern = pattern.WildcardToRegex();
            if (pattern.Contains("."))
                regexPattern = $"^{regexPattern}$";

            List<SearchResult> results = new();
            Search(results, parentStack, regexPattern, includeFile, includeFolder);
            return results;
        }

        private void Search(List<SearchResult> results, List<Folder> parentStack, string regexPattern, bool includeFile, bool includeFolder) {
            List<Folder> stack = new List<Folder>(parentStack);
            stack.Add(this);

            if (includeFile && (Files != null))
                foreach (File f in Files) {
                    if (Regex.IsMatch(f.Name, regexPattern, RegexOptions.IgnoreCase))
                        results.Add(new SearchResult(f, stack));
                }

            if (Folders != null)
                foreach (Folder f in Folders) {
                    if (includeFolder && Regex.IsMatch(f.Name, regexPattern, RegexOptions.IgnoreCase))
                        results.Add(new SearchResult(f, stack));
                    f.Search(results, stack, regexPattern, includeFile, includeFolder);
                }
        }
    }
}
