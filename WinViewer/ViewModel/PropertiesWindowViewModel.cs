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
        private PropertyInfo _propertyInfo;

        public bool ContainsFolder { get; private set; }
        public bool IsSingleItem { get; private set; }
        public bool IsSingleFile { get; private set; }
        public FileSystemItem Item { get; private set; }
        public string Title { get; private set; }
        public string FileSystemType { get; private set; }
        public string Location { get; private set; }
        public string Size { get; private set; }
        public string Contains { get; private set; }

        public PropertiesWindowViewModel(IEnumerable<FileSystemItem> items, List<Folder> parentStack) {
            _propertyInfo = new PropertyInfo(items);

            ContainsFolder = _propertyInfo.Folders.Any();
            IsSingleItem = items.Count() == 1;
            IsSingleFile = _propertyInfo.Files.Count == 1 && !ContainsFolder;
            Title = IsSingleItem ? items.Single().Name :
                string.Format("{0} Files, {1} Folders", _propertyInfo.FileCountString, _propertyInfo.FolderCountString);
            FileSystemType = IsSingleItem ? items.Single().GetType().Name : "Multiple Types";
            Location = Path.Combine(parentStack.Select(f => f.Name).ToArray());
            Size = string.Format("{0} ({1} bytes)", _propertyInfo.TotalSizeFriendlyString, _propertyInfo.TotalSizeString);
            if (ContainsFolder)
                Contains = string.Format("{0} Files, {1} Folders", _propertyInfo.FileCountString, _propertyInfo.FolderCountString);
            if (IsSingleItem)
                Item = items.Single();
        }
    }
}
