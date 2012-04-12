﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PureLib.WPF;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    public class PropertiesWindowViewModel : ViewModelBase {
        private List<Folder> _itemStack;

        public FileSystemItem Item { get; private set; }

        public string Location {
            get {
                return System.IO.Path.GetFullPath(Item.Name).ToLower();
            }
        }

        public PropertiesWindowViewModel(FileSystemItem item, List<Folder> itemStack) {
            Item = item;
            _itemStack = itemStack;
        }
    }
}