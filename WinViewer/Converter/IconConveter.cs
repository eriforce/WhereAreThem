using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WhereAreThem.Model;
using WhereAreThem.WinViewer.Properties;
using IO = System.IO;

namespace WhereAreThem.WinViewer {
    public class IconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ItemType type;
            if (!Enum.TryParse<ItemType>(value.GetType().Name, out type))
                return null;

            Icon icon;
            if (type == ItemType.File)
                icon = IconReader.GetFileIcon(IO.Path.GetExtension(((File)value).Name).ToLower(),
                    IconReader.IconSize.Small, false);
            else
                icon = (Icon)Resources.ResourceManager.GetObject(type.ToString());
            return icon.ToImageSource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
