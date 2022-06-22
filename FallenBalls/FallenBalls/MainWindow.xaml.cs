using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace FallenBalls
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartThreads(object sender, RoutedEventArgs e)
        {
            int.TryParse(FormsCount.Text, out int formCounts);
            int.TryParse(WidthBalls.Text, out int widthInBalls);
            int.TryParse(HeightBalls.Text, out int heightInBalls);
            int.TryParse(FormDistances.Text, out int formDistances);
            int.TryParse(BallSize.Text, out int ballSize);
            int.TryParse(BallSpeed.Text, out int ballSpeed);

            if (!(formCounts > 0 && formCounts <= 5) ||
                !(widthInBalls > 0 && widthInBalls <= 10) ||
                !(heightInBalls > 0 && heightInBalls <= 20) ||
                !(formDistances > 0 && formDistances <= 100) ||
                !(ballSize > 0 && ballSize <= 50) ||
                !(ballSpeed > 0 && ballSpeed <= 100))
            {
                ShowMessage("Данные введены неверно!\n\n" +
                    "Максимальное количество окно: 5;\n" +
                    "Максимальные высота в шариках: 20;\n" +
                    "Максимальная ширина в шариках: 10;\n" +
                    "Максимальный размер шариков: 50;\n" +
                    "Максимальная дистацния между формами: 100;\n" +
                    "Максимальная скорость шарика: 100.");

                return;
            }

            double left = (SystemParameters.PrimaryScreenWidth - formCounts * widthInBalls * ballSize - (formCounts - 1) * formDistances) / 2;
            double top = SystemParameters.PrimaryScreenHeight / 4;

            for (int i = 0; i < formCounts; ++i)
            {
                FallenWindow win = new FallenWindow(widthInBalls, heightInBalls, ballSize, ballSpeed)
                {
                    Left = left,
                    Top = top,
                    Width = ballSize * widthInBalls + Width - (Content as FrameworkElement).ActualWidth,
                    Height = ballSize * heightInBalls + Height - (Content as FrameworkElement).ActualHeight,
                };
                win.Show();

                left += widthInBalls * ballSize + formDistances;
            }
        }

        private void ShowMessage(string text) => MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
