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
                    size += Files.Sum(f => f.Size);
                if (Folders != null)
                    size += Folders.Sum(f => f.Size);
                return size;
            }
        }

        public List<SearchResult> Search(List<Folder> folderStack, string pattern, bool includeFile, bool includeFolder) {
            if (!includeFile && !includeFolder)
                return null;

            string regexPattern = pattern.WildcardToRegex();
            if (pattern.Contains("."))
                regexPattern = "^{0}$".FormatWith(regexPattern);

            List<SearchResult> results = new List<SearchResult>();
            Search(results, folderStack, regexPattern, includeFile, includeFolder);
            return results;
        }

        private void Search(List<SearchResult> results, List<Folder> folderStack, string regexPattern, bool includeFile, bool includeFolder) {
            List<Folder> stack = new List<Folder>(folderStack);
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
