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
        private static Loader _loader;
        private static Scanner _scanner;

        public static Loader Loader {
            get {
                if (_loader == null)
                    _loader = new Loader(Constant.Path, Constant.Persistence);
                return _loader;
            }
        }
        public static Scanner Scanner {
            get {
                if (_scanner == null)
                    _scanner = new Scanner(Constant.Path, Constant.Persistence);
                return _scanner;
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            if (MainWindow == null)
                MessageBox.Show(e.Exception.GetTraceText());
            else
                MessageBox.Show(MainWindow, e.Exception.GetTraceText());
            e.Handled = false;
        }
    }
}
