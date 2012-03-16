using System;
using System.Globalization;
using System.Windows.Data;
using PureLib.Common;

namespace WinViewer {
    public class SizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Utility.ToFriendlyString((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
