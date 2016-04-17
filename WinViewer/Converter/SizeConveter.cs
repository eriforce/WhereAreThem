using System;
using System.Globalization;
using System.Windows.Data;
using PureLib.Common;

namespace WhereAreThem.WinViewer.Converter {
    public class SizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((long)value).ToFriendlyString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
