using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WhereAreThem.Model;
using WinViewer.Properties;

namespace WinViewer {
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
            return GetBitmap(icon);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private BitmapFrame GetBitmap(Icon icon) {
            MemoryStream stream = new MemoryStream();
            icon.Save(stream);
            icon.Dispose();
            IconBitmapDecoder decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            return decoder.Frames[0];
        }
    }
}
