using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using WhereAreThem.Model;
using WhereAreThem.WinViewer.Properties;

namespace WhereAreThem.WinViewer {
    public class IconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            ItemType type;
            if (!Enum.TryParse<ItemType>(value.GetType().Name, out type))
                return null;

            Icon icon;
            if (type == ItemType.File) {
                string ext = Path.GetExtension(((WhereAreThem.Model.File)value).Name).ToLower();
                icon = IconReader.GetFileIcon(ext, IconReader.IconSize.Small, false);
            }
            else {
                if (((Folder)value).Name.Contains(Path.VolumeSeparatorChar))
                    type = ItemType.Drive;
                icon = (Icon)Resources.ResourceManager.GetObject(type.ToString());
            }
            return icon.ToImageSource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
