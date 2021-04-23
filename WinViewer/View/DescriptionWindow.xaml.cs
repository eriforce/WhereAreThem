using System.Windows;
using WhereAreThem.Model.Models;
using WhereAreThem.WinViewer.ViewModel;

namespace WhereAreThem.WinViewer.View {
    /// <summary>
    /// Interaction logic for DescriptionWindow.xaml
    /// </summary>
    public partial class DescriptionWindow : Window {
        public DescriptionWindowViewModel VM { get; private set; }

        public DescriptionWindow(File file) {
            InitializeComponent();

            VM = new DescriptionWindowViewModel(file);
            VM.View = this;
            DataContext = VM;
        }
    }
}
