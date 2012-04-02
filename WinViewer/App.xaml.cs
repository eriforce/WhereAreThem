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
        private static Loader _loader;
        public static Loader Loader {
            get {
                if (_loader == null)
                    _loader = new Loader(ConfigurationManager.AppSettings["path"].WrapPath(),
                        Constant.GetPersistence(Type.GetType(ConfigurationManager.AppSettings["persistence"])));
                return _loader;
            }
        }
    }
}
