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
            if (Enum.TryParse<ItemType>(value.GetType().Name, out type)) {
                if ((type == ItemType.Folder) && ((Folder)value).Name.Contains(Path.VolumeSeparatorChar))
                    type = ItemType.Drive;
                return GetBitmap(type);
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private BitmapFrame GetBitmap(ItemType type) {
            MemoryStream stream = new MemoryStream();
            Icon icon = (Icon)Resources.ResourceManager.GetObject(type.ToString());
            icon.Save(stream);
            IconBitmapDecoder decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
            return decoder.Frames[0];
        }
    }
}
