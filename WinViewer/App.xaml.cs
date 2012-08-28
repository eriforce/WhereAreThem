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

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            if (MainWindow == null)
                MessageBox.Show(e.Exception.GetTraceText());
            else
                MessageBox.Show(MainWindow, e.Exception.GetTraceText());
        }
    }
}
