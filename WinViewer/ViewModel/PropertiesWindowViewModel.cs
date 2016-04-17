using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Model;
using WatFile = WhereAreThem.Model.Models.File;

namespace WhereAreThem.WinViewer.ViewModel {
    public class PropertiesWindowViewModel : ViewModelBase {
        private List<Folder> _parentStack;
        private PropertyInfo _propertyInfo;

        public FileSystemItem Item { get; private set; }
        public string FileSystemType {
            get { return Item.GetType().Name; }
        }
        public string Location {
            get { return Path.Combine(_parentStack.Select(f => f.Name).ToArray()); }
        }
        public string Size {
            get { return string.Format("{0} ({1} bytes)", _propertyInfo.TotalSizeFriendlyString, _propertyInfo.TotalSizeString); }
        }
        public string Contains {
            get {
                string strFilesCount = string.Empty;
                if (IsFolder) {
                    Folder folder = (Folder)Item;
                    strFilesCount = string.Format("{0} Files, {1} Folders", _propertyInfo.FileCountString, _propertyInfo.FolderCountString);
                }
                return strFilesCount;
            }
        }
        public bool IsFolder {
            get { return !IsFile; }
        }
        public bool IsFile {
            get { return Item is WatFile; }
        }

        public PropertiesWindowViewModel(FileSystemItem item, List<Folder> parentStack) {
            Item = item;
            _parentStack = parentStack;
            _propertyInfo = new PropertyInfo(parentStack.Last(), new string[] { Item.Name });
        }
    }
}
