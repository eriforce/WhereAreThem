using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using PureLib.Common;

namespace WhereAreThem.Model.Models {
    [DataContract]
    public class Folder : FileSystemItem {
        [DataMember]
        public List<Folder> Folders { get; set; }
        [DataMember]
        public List<File> Files { get; set; }

        public List<FileSystemItem> Items {
            get {
                List<FileSystemItem> items = new List<FileSystemItem>();
                if (Folders != null)
                    items.AddRange(Folders);
                if (Files != null)
                    items.AddRange(Files);
                return items;
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

        public List<SearchResult> Search(List<Folder> parentStack, string pattern, bool includeFile, bool includeFolder) {
            if (!includeFile && !includeFolder)
                return null;

            string regexPattern = pattern.WildcardToRegex();
            if (pattern.Contains("."))
                regexPattern = "^{0}$".FormatWith(regexPattern);

            List<SearchResult> results = new List<SearchResult>();
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
