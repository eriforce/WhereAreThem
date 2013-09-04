using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Model;

namespace WhereAreThem.WinViewer.Converter {
    public class IconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ItemType type;
            if (!Enum.TryParse<ItemType>(value.GetType().Name, out type))
                return null;

            IconSize size;
            if(!Enum.TryParse<IconSize>((string)parameter, out size))
                size = IconSize.Small;

            Icon icon;
            if (type == ItemType.File)
                icon = IconReader.GetFileIcon(((File)value).Extension.ToLower(), size, false);
            else {
                if ((type == ItemType.DriveModel) && !Enum.TryParse<ItemType>(((DriveModel)value).DriveType.ToString(), out type))
                    return null;
                icon = IconReader.GetIcon(type, size);
            }
            return icon.ToImageSource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
