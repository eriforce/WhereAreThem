using System;
using System.Collections.Generic;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Event {
    public class ItemsEventArgs : EventArgs {
        public IEnumerable<FileSystemItem> Items { get; private set; }
        public List<Folder> Stack { get; private set; }

        public ItemsEventArgs(IEnumerable<FileSystemItem> items, List<Folder> stack) {
            Items = items;
            Stack = stack;
        }
    }

    public delegate void ItemsEventHandler(object sender, ItemsEventArgs e);
}
