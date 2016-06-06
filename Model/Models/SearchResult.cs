using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace WhereAreThem.Model.Models {
    public class SearchResult {
        public FileSystemItem Item { get; private set; }
        public List<Folder> Stack { get; private set; }
        public string ItemPath
            => Path.Combine(Stack.Select(f => f.Name).ToArray());

        public SearchResult(FileSystemItem item, List<Folder> stack) {
            Item = item;
            Stack = stack;
        }
    }
}
