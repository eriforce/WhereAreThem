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
using System.Windows.Shapes;
using WhereAreThem.Model;

namespace WhereAreThem.WinViewer {
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window {
        public SearchWindowViewModel VM { get; private set; }

        public SearchWindow() {
            InitializeComponent();

            Activated += (s, e) => {
                VM.RefreshWindowTitle();
                txtSearch.SelectAll();
                Keyboard.Focus(txtSearch);
            };

            VM = new SearchWindowViewModel();
            VM.View = this;
            DataContext = VM;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            VM.OnLocatingItem();
        }
    }
}
