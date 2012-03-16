using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WhereAreThem.Model;

namespace WinViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindowViewModel VM { get; private set; }

        public MainWindow() {
            InitializeComponent();

            VM = new MainWindowViewModel();
            VM.View = this;
            DataContext = VM;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Folder folder = (Folder)e.NewValue;
            VM.SubItems = new List<FileSystemItem>();
            if (folder.Folders != null)
                VM.SubItems.AddRange(folder.Folders);
            if (folder.Files != null)
                VM.SubItems.AddRange(folder.Files);
        }
    }
}
