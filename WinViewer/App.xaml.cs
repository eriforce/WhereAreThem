using PureLib.WPF;
using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using WhereAreThem.Model;
using WhereAreThem.Model.Persistences;
using WhereAreThem.WinViewer.Model;

namespace WhereAreThem.WinViewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : SingletonApp {
        private static Loader _loader;
        private static Scanner _scanner;

        public static Loader Loader {
            get {
                if (_loader == null)
                    _loader = new Loader(Constant.WatRootPath, PersistenceFactory.Persistence);
                return _loader;
            }
        }
        public static Scanner Scanner {
            get {
                if (_scanner == null)
                    _scanner = new Scanner(Constant.WatRootPath, PersistenceFactory.Persistence);
                return _scanner;
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            if (MainWindow == null)
                MessageBox.Show(e.Exception.GetExceptionText());
            else
                MessageBox.Show(MainWindow, e.Exception.GetExceptionText());
            e.Handled = false;
        }
    }
}
