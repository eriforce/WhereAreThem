using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model.Models;
using IO = System.IO;

namespace WhereAreThem.WinViewer.ViewModel {
    public class PropertiesWindowViewModel : ViewModelBase {
        private List<Folder> _itemStack;
        private PropertyInfo _propertyInfo;
        private ICommand _okCommand;

        public FileSystemItem Item { get; private set; }
        public string FileSystemType {
            get { return Item.GetType().Name; }
        }
        public string Location {
            get { return IO.Path.Combine(_itemStack.Select(f => f.Name).ToArray()); }
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
            get { return Item is File; }
        }
        public ICommand OkCommand {
            get {
                if (_okCommand == null)
                    _okCommand = new RelayCommand((p) => View.Close());
                return _okCommand;
            }
        }

        public PropertiesWindowViewModel(FileSystemItem item, List<Folder> itemStack) {
            Item = item;
            _itemStack = itemStack;
            _propertyInfo = new PropertyInfo(_itemStack.Last(), new string[] { Item.Name });
        }
    }
}
