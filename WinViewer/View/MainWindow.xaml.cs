using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PureLib.Common;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.Event;
using WhereAreThem.WinViewer.Model;
using WhereAreThem.WinViewer.ViewModel;

namespace WhereAreThem.WinViewer.View {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private TreeViewItem _selectedTreeViewItem;
        private SearchWindow _searchWindow;

        public MainWindowViewModel VM { get; private set; }
        public SearchWindow SearchWindow {
            get {
                if (_searchWindow == null) {
                    _searchWindow = new SearchWindow();
                    _searchWindow.Owner = this;
                    _searchWindow.VM.LocatingItem += OnLocatingItem;
                    _searchWindow.Closing += (s, e) => {
                        _searchWindow.Hide();
                        e.Cancel = true;
                    };
                }
                return _searchWindow;
            }
        }

        public MainWindow() {
            InitializeComponent();

            AllowDrop = true;
            Drop += OnWindowDrop;
            KeyDown += OnWindowKeyDown;
            Closing += OnWindowClosing;

            VM = new MainWindowViewModel();
            VM.View = this;
            VM.LocatingItem += OnLocatingItem;
            VM.OpeningProperties += OnOpeningProperties;
            VM.OpeningDescription += OnOpeningDescription;
            DataContext = VM;
        }

        private void OnLocatingItem(object sender, ItemEventArgs e) {
            TreeViewItem treeViewItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(e.Stack[0]);
            for (int i = 1; i < e.Stack.Count; i++) {
                treeViewItem.IsExpanded = true;
                treeViewItem.UpdateLayout();
                treeViewItem = (TreeViewItem)treeViewItem.ItemContainerGenerator.ContainerFromItem(e.Stack[i]);
            }
            treeViewItem.IsSelected = true;
            VM.SelectedItem = e.Item;
        }

        private void OnOpeningProperties(object sender, ItemEventArgs e) {
            PropertiesWindow window = new PropertiesWindow(e.Item, e.Stack);
            window.Owner = this;
            window.Show();
        }

        private void OnOpeningDescription(object sender, EventArgs<File> e) {
            DescriptionWindow window = new DescriptionWindow(e.Data);
            window.Owner = this;
            window.Show();
        }

        private void OnWindowDrop(object sender, DragEventArgs e) {
            Dispatcher.InvokeAsync(async () => {
                if (!VM.IsBusy && e.Data.GetDataPresent(DataFormats.FileDrop))
                    await VM.ScanAsync(e.Data.GetData(DataFormats.FileDrop, true) as string[]);
            });
            e.Handled = true;
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e) {
            if ((e.Key == Key.F) && (Keyboard.Modifiers == ModifierKeys.Control)) {
                if ((VM.SelectedFolder != null) && !(VM.SelectedFolder is Computer)) {
                    if ((SearchWindow.VM.RootStack != null) && !SearchWindow.VM.RootStack.SequenceEqual(VM.SelectedFolderStack))
                        SearchWindow.VM.Results = null;
                    SearchWindow.VM.RootStack = VM.SelectedFolderStack;
                    SearchWindow.VM.Root = VM.SelectedFolder;
                    SearchWindow.VM.Location = VM.Location;
                    SearchWindow.Show();
                }
                e.Handled = true;
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e) {
            VM.Save();
            if (VM.Watcher != null)
                VM.Watcher.Close();
        }

        private void FolderTreeMouseRightClick(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = GetParent<TreeViewItem>((DependencyObject)e.OriginalSource);
            item.Focus();
            e.Handled = true;
        }

        private void FolderTreeSelected(object sender, RoutedEventArgs e) {
            _selectedTreeViewItem = (TreeViewItem)e.OriginalSource;

            Folder selectedFolder = (Folder)_selectedTreeViewItem.Header;
            LoadIfDrive(selectedFolder);
            List<Folder> stack = new List<Folder>();
            GetFolderStack(_selectedTreeViewItem, stack);
            stack.Add(selectedFolder);
            VM.SelectedFolderStack = stack;
            VM.SelectedFolder = selectedFolder;
        }

        private void FolderTreeExpanded(object sender, RoutedEventArgs e) {
            TreeViewItem treeViewItem = (TreeViewItem)e.OriginalSource;
            LoadIfDrive(treeViewItem.Header);
        }

        private void DataGridMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGridRow row = MainWindow.GetParent<DataGridRow>((DependencyObject)e.OriginalSource);
            if (row != null) {
                DataGrid dataGrid = (DataGrid)sender;
                FileSystemItem item = (FileSystemItem)dataGrid.SelectedItem;

                if (item is Folder) {
                    _selectedTreeViewItem.IsExpanded = true;
                    _selectedTreeViewItem.UpdateLayout();
                    _selectedTreeViewItem = (TreeViewItem)_selectedTreeViewItem.ItemContainerGenerator.ContainerFromItem(item);
                    _selectedTreeViewItem.IsSelected = true;
                    _selectedTreeViewItem.BringIntoView();
                }
            }
        }

        private void DataGridSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (VM.SelectedItem != null)
                ((DataGrid)sender).ScrollIntoView(VM.SelectedItem);
        }

        private void DataGridCopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e) {
            DataGridClipboardCellContent content = new DataGridClipboardCellContent(
                e.Item, e.ClipboardRowContent[0].Column, ((FileSystemItem)e.Item).Name);
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(content);
        }

        private void LoadIfDrive(object item) {
            if (item is DriveModel) {
                DriveModel drive = (DriveModel)item;
                bool loaded = VM.BusyWith("Loading {0} ...".FormatWith(drive.Name), Task.Run(() => drive.Load()));
                if (!loaded)
                    MessageBox.Show(this, "You need to scan this drive first.");
            }
        }

        private void GetFolderStack(TreeViewItem item, List<Folder> stack) {
            TreeViewItem parent = GetParent<TreeViewItem>(item);
            if (parent != null) {
                stack.Insert(0, (Folder)parent.Header);
                GetFolderStack(parent, stack);
            }
        }

        public static T GetParent<T>(DependencyObject item) where T : DependencyObject {
            DependencyObject parent = item;
            do {
                parent = VisualTreeHelper.GetParent(parent);
            }
            while ((parent != null) && !(parent is T));
            return parent as T;
        }
    }
}
