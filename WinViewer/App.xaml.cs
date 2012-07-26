using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using PureLib.Common;
using PureLib.WPF;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : SingleInstanceApp {
        private static ILoader _loader;
        public static ILoader Loader {
            get {
                if (_loader == null)
                    _loader = new Loader(
                        ConfigurationManager.AppSettings["path"].WrapPath(), Constant.Persistence);
                return _loader;
            }
        }

        public App()
            : base(new Guid("{09B64957-2D83-410E-8430-6FC63E11D735}")) {
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            MessageBox.Show(MainWindow, e.Exception.GetTraceText());
        }
    }
}
