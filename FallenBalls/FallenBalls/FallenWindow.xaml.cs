using SysLab6;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FallenBalls
{
    public partial class FallenWindow : Window
    {
        private const int WM_THREAD_FINISHED = User32.WM_USER + 1;
        private const int WM_CLEAR_THREAD_FINISHED = User32.WM_USER + 2;

        private Thread _drawingThread = null;
        private Thread _clearThread = null;

        private static object _locker;

        private Color[,] _ballsMatrix;

        private readonly WMReceiver _receiver;
        private readonly Random _random = new Random();
        private readonly int _windowWidthInBalls;
        private readonly int _windowHeightInBalls;
        private readonly int _ballSize;
        private readonly int _ballSpeed;
        private bool isClearThreadRun = true;

        private enum Colors
        {
            RED,
            GREEN
            //BLUE,
            //YELLOW,
            //PURPLE,
            //BROWN,
        }

        public FallenWindow(int widthInBalls, int heightInBalls, int ballSize, int ballSpeed)
        {
            InitializeComponent();
            _receiver = new WMReceiver(ThreadMessageHandler);

            _windowWidthInBalls = widthInBalls;
            _windowHeightInBalls = heightInBalls;
            _ballSize = ballSize;
            _ballSpeed = ballSpeed;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _ballsMatrix = new Color[_windowWidthInBalls, _windowHeightInBalls];
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            _drawingThread = new Thread(new ParameterizedThreadStart(ExecuteDrawingThread));
            _drawingThread.Start(
                new ThreadArguments(
                    hwnd: hwnd,
                    receiverHandle: _receiver.Handle
            ));

            _clearThread = new Thread(new ParameterizedThreadStart(ExecuteClearThread));
            _clearThread.Start(new ThreadArguments(hwnd, _receiver.Handle));
            _locker = new object();
        }

        private void ExecuteDrawingThread(object args)
        {
            Color GetBall(int column, int row)
            {
                Color ball;
                lock (_locker)
                    ball = _ballsMatrix[column, row];

                return ball;
            }

            void SetBall(int column, int row, Color ball)
            {
                lock (_locker)
                    _ballsMatrix[column, row] = ball;
            }

            ThreadArguments threadArgs = args as ThreadArguments;
            IntPtr hwnd = threadArgs.Hwnd;
            IntPtr receiverHandle = threadArgs.ReceiverHandle;

            bool isRun = true;

            Graphics g = Graphics.FromHwnd(hwnd);
            Brush clearBrush = new SolidBrush(Color.White);

            g.FillRectangle(clearBrush, 0, 0, _windowWidthInBalls * _ballSize, _windowHeightInBalls * _ballSize);
            while (isRun)
            {
                // Draw exist balls.
                for (int x = 0; x < _windowWidthInBalls; x++)
                {
                    for (int y = 0; y < _windowHeightInBalls; y++)
                    {
                        Color ball = GetBall(x, y);
                        if (ball == Color.Empty)
                        {
                            g.FillEllipse(new SolidBrush(Color.White), x * _ballSize, y * _ballSize, _ballSize, _ballSize);
                            continue;
                        }

                        g.FillEllipse(new SolidBrush(ball), x * _ballSize, y * _ballSize, _ballSize, _ballSize);
                    }
                }

                // Draw new ball.
                int ballColumn = _random.Next(_windowWidthInBalls);
                Color ballColor = GetColor(_random.Next(Enum.GetNames(typeof(Colors)).Length));
                Brush brush = new SolidBrush(ballColor);
                int ballRow = _windowHeightInBalls;
                for (int y = 0; y < _windowHeightInBalls; y++)
                {
                    if (GetBall(ballColumn, y) != Color.Empty) break;

                    g.FillEllipse(brush, ballColumn * _ballSize, y * _ballSize, _ballSize, _ballSize);
                    Thread.Sleep(405 - _ballSpeed * 4);
                    g.FillEllipse(clearBrush, ballColumn * _ballSize, y * _ballSize, _ballSize, _ballSize);

                    ballRow = y;
                }
                g.FillEllipse(brush, ballColumn * _ballSize, ballRow * _ballSize, _ballSize, _ballSize);
                SetBall(ballColumn, ballRow, ballColor);
                brush.Dispose();

                isRun = ballRow != 0;
            }

            g.Dispose();
            User32.PostMessage(receiverHandle, WM_THREAD_FINISHED, 0, 0);
            lock (_locker)
                isClearThreadRun = false;
        }

        private void ExecuteClearThread(object args)
        {
            Color GetBall(int column, int row)
            {
                Color ball;
                lock (_locker)
                    ball = _ballsMatrix[column, row];

                return ball;
            }

            void SetBall(int column, int row, Color ball)
            {
                lock (_locker)
                    _ballsMatrix[column, row] = ball;
            }

            (bool result, int count, int lastIdx) IsHaveHorizontalCombo(int row)
            {
                int counter = 0;
                int totalCounter = -1;
                int lastIdx = -1;
                Color previousColor = Color.Empty;

                for (int x = 0; x < _windowWidthInBalls; ++x)
                {
                    Color currentBall = GetBall(x, row);
                    if (currentBall == Color.Empty)
                    {
                        counter = 0;
                        continue;
                    }

                    if (currentBall != previousColor)
                    {
                        counter = 0;
                        previousColor = currentBall;
                    }

                    counter++;
                    if (counter >= 3)
                    {
                        totalCounter = counter;
                        lastIdx = x;
                    }
                }

                return (totalCounter >= 3, totalCounter, lastIdx);
            }

            (bool result, int count, int lastIdx) IsHaveVerticalCombo(int col)
            {
                int counter = 0;
                int totalCounter = -1;
                int lastIdx = -1;
                Color previousColor = Color.Empty;

                for (int y = 1; y < _windowHeightInBalls; ++y)
                {
                    Color currentBall = GetBall(col, y);
                    if (currentBall == Color.Empty)
                    {
                        counter = 0;
                        continue;
                    }

                    if (currentBall != previousColor)
                    {
                        counter = 0;
                        previousColor = currentBall;
                    }

                    counter++;
                    if (counter >= 3)
                    {
                        totalCounter = counter;
                        lastIdx = y;
                    }
                }

                return (totalCounter >= 3, totalCounter, lastIdx);
            }

            void ClearHorizontalCombo(int counter, int idx, int row)
            {
                for (int x = idx - counter + 1; x <= idx; x++)
                {
                    for (int y = row; y > 0; y--)
                    {
                        SetBall(x, y, GetBall(x, y - 1));
                    }
                }
            }

            void ClearVerticalCombo(int counter, int idx, int col)
            {
                for (int y = idx - counter + 1; y <= idx; y++)
                {
                    SetBall(col, y, GetBall(col, y - 1));
                }
            }

            ThreadArguments threadArgs = args as ThreadArguments;
            IntPtr receiverHandle = threadArgs.ReceiverHandle;

            bool isRun = true;
            while (isRun)
            {
                for (int y = _windowHeightInBalls - 1; y > 0; y--)
                {
                    (bool resultHorizontal, int countHorizontal, int lastIdxHorizontal) = IsHaveHorizontalCombo(y);
                    if (resultHorizontal) ClearHorizontalCombo(countHorizontal, lastIdxHorizontal, y);
                }

                for (int x = 0; x < _windowWidthInBalls; ++x)
                {
                    (bool resultVertical, int countVertical, int lastIdxVertical) = IsHaveVerticalCombo(x);
                    if (resultVertical) ClearVerticalCombo(countVertical, lastIdxVertical, x);
                }

                lock (_locker)
                    isRun = isClearThreadRun;
            }

            User32.PostMessage(receiverHandle, WM_CLEAR_THREAD_FINISHED, 0, 0);
        }

        private void ThreadMessageHandler(Message message)
        {
            switch (message.Msg)
            {
                case WM_THREAD_FINISHED:
                    _drawingThread = null;
                    SetLockerNull();
                    break;
                case WM_CLEAR_THREAD_FINISHED:
                    _clearThread = null;
                    SetLockerNull();
                    break;
            }

            void SetLockerNull()
            {
                if (_drawingThread == null && _clearThread == null)
                    _locker = null;
            }
        }

        private Color GetColor(int colorId)
        {
            Color color = Color.Silver;
            switch ((Colors)colorId)
            {
                case Colors.RED:
                    color = Color.Red;
                    break;
                case Colors.GREEN:
                    color = Color.Green;
                    break;
                    //case Colors.BLUE:
                    //    color = Color.Blue;
                    //    break;
                    //case Colors.YELLOW:
                    //    color = Color.Yellow;
                    //    break;
                    //case Colors.PURPLE:
                    //    color = Color.Purple;
                    //    break;
                    //case Colors.BROWN:
                    //    color = Color.Brown;
                    //    break;
            }

            return color;
        }

        private void FormClosing(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = _drawingThread != null;
    }
}
