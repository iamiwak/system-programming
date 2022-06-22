using System;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using SysLab1;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace myapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> _items = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            Files.ItemsSource = _items;
        }

        private string SelectFolder()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Выбор папки";
            fbd.ShowNewFolderButton = true;

            return (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) ? fbd.SelectedPath : "";
        }

        private bool IsFileExist()
        {
            if (!File.Exists(FilePath.Text))
            {
                MessageBox.Show("По данному пути нет файла.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsDirectoryExist()
        {
            if (!Directory.Exists(SourceFolderPath.Text))
            {
                MessageBox.Show("По данному пути нет папки.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt|Executable files (*.exe)|*.exe";
            ofd.InitialDirectory = @"D:\3is2\timofeev\system programming\test\source";

            if (ofd.ShowDialog() == true)
                FilePath.Text = ofd.FileName;
        }

        private void SelectSourceFolder(object sender, RoutedEventArgs e) => SourceFolderPath.Text = SelectFolder();

        private void SelectDestinationFolder(object sender, RoutedEventArgs e) => DestinationFolderPath.Text = SelectFolder();

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            if (!IsFileExist())
                return;

            ShellExecute shellExecute = new ShellExecute();
            shellExecute.Verb = ShellExecute.OpenFile;
            shellExecute.Path = FilePath.Text;
            shellExecute.Execute();
        }

        private void StartFile(object sender, RoutedEventArgs e)
        {
            if (!IsFileExist())
                return;

            ShellExecute shellExecute = new ShellExecute();
            shellExecute.Verb = ShellExecute.EditFile;
            shellExecute.Path = FilePath.Text;
            shellExecute.Execute();
        }

        private void PrintFile(object sender, RoutedEventArgs e)
        {
            if (!IsFileExist())
                return;

            ShellExecute shellExecute = new ShellExecute();
            shellExecute.Verb = ShellExecute.PrintFile;
            shellExecute.Path = FilePath.Text;
            shellExecute.Execute();
        }

        private void AddToListFiles(object sender, RoutedEventArgs e)
        {
            if (!IsFileExist())
                return;

            if (_items.Contains(FilePath.Text))
                return;

            _items.Add(FilePath.Text);
        }

        private void RenameFile(object sender, RoutedEventArgs e)
        {
            if (!IsFileExist())
                return;

            new RenameWindow(FilePath.Text).Show();

            FilePath.Text = "";
        }

        private void DeleteFromList(object sender, RoutedEventArgs e)
        {
            int selectedIndex = Files.SelectedIndex;
            if (!(selectedIndex >= 0 && selectedIndex < _items.Count))
            {
                MessageBox.Show("Вы не выбрали файл.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _items.RemoveAt(selectedIndex);
        }

        private void ClearList(object sender, RoutedEventArgs e) => _items.Clear();

        private void MoveFilesFromList(object sender, RoutedEventArgs e) => FileOperation.MoveFiles(_items, DestinationFolderPath.Text);

        private void CopyFilesFromList(object sender, RoutedEventArgs e) => FileOperation.CopyFiles(_items, DestinationFolderPath.Text);

        private void DeleteFilesFromList(object sender, RoutedEventArgs e) => FileOperation.DeleteFiles(_items);

        private void MoveFilesWithMask(object sender, RoutedEventArgs e) => FileOperation.MoveFiles(SourceFolderPath.Text, DestinationFolderPath.Text, FileMask.Text);

        private void CopyFilesWithMask(object sender, RoutedEventArgs e) => FileOperation.CopyFiles(SourceFolderPath.Text, DestinationFolderPath.Text, FileMask.Text);

        private void DeleteFilesWithMask(object sender, RoutedEventArgs e) => FileOperation.DeleteFiles(SourceFolderPath.Text, FileMask.Text);

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            if (!IsDirectoryExist())
                return;

            Process.Start("explorer.exe", SourceFolderPath.Text);
        }

        private void OpenFolderWithSearch(object sender, RoutedEventArgs e)
        {
            if (!IsDirectoryExist())
                return;

            ShellExecute shellExecute = new ShellExecute();
            shellExecute.Verb = ShellExecute.FindInFolder;
            shellExecute.Path = SourceFolderPath.Text;
            shellExecute.Execute();
        }

        private void Files_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
