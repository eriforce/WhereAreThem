using System;
using System.Collections.Generic;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Event {
    public class ItemEventArgs : EventArgs {
        public FileSystemItem Item { get; private set; }
        public List<Folder> Stack { get; private set; }

        public ItemEventArgs(FileSystemItem item, List<Folder> stack) {
            Item = item;
            Stack = stack;
        }
    }

    public delegate void ItemEventHandler(object sender, ItemEventArgs e);
}
