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

namespace myapp
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        private string _path;

        public RenameWindow(string filePath)
        {
            InitializeComponent();

            _path = filePath;
            FileName.Text = filePath.Substring(filePath.LastIndexOf('\\') + 1);
        }

        private void CancelClick(object sender, RoutedEventArgs e) => Close();

        private void RenameClick(object sender, RoutedEventArgs e)
        {
            if (FileName.Text == "")
                return;

            FileOperation.RenameFile(_path, FileName.Text);

            Close();
        }
    }
}
