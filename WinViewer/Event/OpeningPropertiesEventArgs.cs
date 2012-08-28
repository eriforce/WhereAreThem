using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhereAreThem.Model;
using WhereAreThem.Model.Models;

namespace WhereAreThem.WinViewer.Event {
    public class OpeningPropertiesEventArgs : EventArgs {
        public FileSystemItem Item { get; private set; }
        public List<Folder> FolderStack { get; private set; }

        public OpeningPropertiesEventArgs(FileSystemItem item, List<Folder> folderStack) {
            Item = item;
            FolderStack = folderStack;
        }
    }

    public delegate void OpeningPropertiesEventHandler(object sender, OpeningPropertiesEventArgs e);
}
