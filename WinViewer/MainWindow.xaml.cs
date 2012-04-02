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

namespace WhereAreThem.WinViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private TreeViewItem _selectedTreeViewItem;

        public MainWindowViewModel VM { get; private set; }

        public MainWindow() {
            InitializeComponent();

            VM = new MainWindowViewModel();
            VM.View = this;
            DataContext = VM;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGrid dataGrid = (DataGrid)sender;
            FileSystemItem item = (FileSystemItem)dataGrid.SelectedItem;

            if (item is Folder) {
                VM.SelectedFolder = (Folder)item;

                _selectedTreeViewItem.IsExpanded = true;
                _selectedTreeViewItem.UpdateLayout();
                _selectedTreeViewItem = (TreeViewItem)_selectedTreeViewItem.ItemContainerGenerator.ContainerFromItem(item);
                _selectedTreeViewItem.IsSelected = true; ;
            }
        }

        private void FolderTree_Selected(object sender, RoutedEventArgs e) {
            _selectedTreeViewItem = (TreeViewItem)e.OriginalSource;

            TreeView treeView = (TreeView)sender;
            LoadDrive(treeView.SelectedItem);
            VM.SelectedFolder = (Folder)treeView.SelectedItem;
        }

        private void FolderTree_Expanded(object sender, RoutedEventArgs e) {
            TreeViewItem treeViewItem = (TreeViewItem)e.OriginalSource;
            LoadDrive(treeViewItem.Header);
        }

        private void LoadDrive(object item) {
            if (item is Drive)
                ((Drive)item).Load();
        }
    }
}
