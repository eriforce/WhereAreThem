using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Event {
    public class LocatingItemEventArgs : EventArgs {
        public FileSystemItem Item { get; private set; }
        public List<Folder> Stack { get; private set; }

        public LocatingItemEventArgs(FileSystemItem item, List<Folder> stack) {
            Item = item;
            Stack = stack;
        }
    }

    public delegate void LocatingItemEventHandler(object sender, LocatingItemEventArgs e);
}
