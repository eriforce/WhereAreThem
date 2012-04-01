using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using PureLib.Common;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static Loader Loader = new Loader(ConfigurationManager.AppSettings["path"].WrapPath(),
            Constant.GetPersistence(Type.GetType(ConfigurationManager.AppSettings["persistence"])));
    }
}
