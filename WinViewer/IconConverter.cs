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

            if (type == ItemType.File) {
                string ext = Path.GetExtension(((WhereAreThem.Model.File)value).Name).ToLower();
                switch (ext) {
                    case ".txt":
                        type = ItemType.Txt;
                        break;
                    case ".jpg":
                    case ".jp2":
                    case ".png":
                    case ".bmp":
                    case ".gif":
                        type = ItemType.Pic;
                        break;
                    case ".exe":
                        type = ItemType.App;
                        break;
                    case ".dll":
                        type = ItemType.Dll;
                        break;
                    case ".bat":
                    case ".cmd":
                        type = ItemType.Bat;
                        break;
                    case ".ini":
                        type = ItemType.Ini;
                        break;
                }
            }
            else if (((Folder)value).Name.Contains(Path.VolumeSeparatorChar))
                type = ItemType.Drive;

            return GetBitmap(type);
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
