using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace FindFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> _items = new ObservableCollection<string>();
        const string INIT_DIRECTORY = "D:\\3is2\\timofeev\\system programming\\test\\init";

        public MainWindow()
        {
            InitializeComponent();
            FileList.ItemsSource = _items;
            FolderPath.Text = INIT_DIRECTORY;
            MinFileSize.Text = "-1";
            MaxFileSize.Text = "-1";
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.SelectedPath = INIT_DIRECTORY;
                fbd.Description = "Выбор папки";
                fbd.ShowNewFolderButton = true;

                FolderPath.Text = (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) ? fbd.SelectedPath : "";
            }
        }

        private void SearchFiles(object sender, RoutedEventArgs e)
        {
            int minSize = !MinFileSize.Text.Equals("") ? int.Parse(MinFileSize.Text) : -1;
            int maxSize = !MaxFileSize.Text.Equals("") ? int.Parse(MaxFileSize.Text) : -1;
            int filesSize = 0;
            bool isRecursive = (bool)RecursiveSearch.IsChecked;
            bool isHaveIgnoreFiles = IgnoreMask.Text != "";
            List<string> directories = new List<string>() { FolderPath.Text };
            List<string> files = new List<string>();
            List<string> ignoreFiles = new List<string>();

            // Search all of directories
            int dirIndex = 0;
            while (isRecursive && dirIndex < directories.Count)
            {
                List<string> dir = FileOperation.SearchFiles(directories[dirIndex], "*", SearchType.DIRICTORIES, (bool)OnlyHidden.IsChecked, out _, minSize, maxSize);
                directories.AddRange(dir);
                dirIndex++;
            };

            // Search files and ignore files in all directories
            for (int i = 0; i < directories.Count; i++)
            {
                files.AddRange(FileOperation.SearchFiles(directories[i], Mask.Text, (SearchType)ItemsType.SelectedIndex, (bool)OnlyHidden.IsChecked, out int fullSize, minSize, maxSize));
                filesSize += fullSize;

                if (!isHaveIgnoreFiles) continue;
                ignoreFiles.AddRange(FileOperation.SearchFiles(FolderPath.Text, IgnoreMask.Text, (SearchType)ItemsType.SelectedIndex, (bool)OnlyHidden.IsChecked, out int ignoreSize, minSize, maxSize));

                filesSize -= ignoreSize;
            }

            CopyItems(files, ignoreFiles);
            ItemsWeight.Content = $"{filesSize} байт";
        }

        private void CopyItems(List<string> allFiles, List<string> ignoreFiles)
        {
            _items.Clear();

            for (int i = allFiles.Count - 1; i >= 0; i--)
            {
                string file = allFiles[i];
                if (ignoreFiles.IndexOf(file) != -1)
                {
                    allFiles.Remove(file);
                    ignoreFiles.Remove(file);
                }
            }

            for (int k = 0; k < allFiles.Count; ++k)
                _items.Add(allFiles[k]);
        }
    }
}
