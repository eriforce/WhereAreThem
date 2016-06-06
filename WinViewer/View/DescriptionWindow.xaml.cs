using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WhereAreThem.WinViewer.View {
    /// <summary>
    /// Interaction logic for DescriptionWindow.xaml
    /// </summary>
    public partial class DescriptionWindow : Window {
        public DescriptionWindow(Dictionary<string, string> descriptions) {
            InitializeComponent();

            Content = new TextBox() {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                // TODO: rewrite description window
                Text = null,
                FontFamily = new FontFamily("Consolas"),
            };
        }
    }
}
