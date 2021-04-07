using System.Collections.Generic;
using System.Windows;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.ViewModel;

namespace WhereAreThem.WinViewer.View {
    /// <summary>
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window {
        public PropertiesWindowViewModel VM { get; private set; }

        public PropertiesWindow(IEnumerable<FileSystemItem> items, List<Folder> parentStack) {
            InitializeComponent();

            VM = new PropertiesWindowViewModel(items, parentStack);
            VM.View = this;
            DataContext = VM;
        }
    }
}