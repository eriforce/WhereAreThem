using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Model;

namespace WhereAreThem.WinViewer.Converter {
    public class IconConverter : IValueConverter {
        private Dictionary<string, ImageSource> _folderIconCache = new Dictionary<string, ImageSource>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ItemType type;
            if (!Enum.TryParse<ItemType>(value.GetType().Name, out type))
                return null;

            IconSize size;
            if (!Enum.TryParse<IconSize>((string)parameter, out size))
                size = IconSize.Small;

            if (type == ItemType.File) {
                Icon icon = IconReader.GetFileIcon(((File)value).Extension.ToLower(), size, false);
                return icon.ToImageSource();
            }
            else {
                if ((type == ItemType.DriveModel) && !Enum.TryParse<ItemType>(((DriveModel)value).DriveType.ToString(), out type))
                    return null;
                string key = type.ToString() + size.ToString();
                if (!_folderIconCache.ContainsKey(key))
                    _folderIconCache.Add(key, IconReader.GetIcon(type, size).ToImageSource());
                return _folderIconCache[key];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
