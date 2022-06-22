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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static firstLab.QueueMessage;

namespace firstLab
{
    public partial class MainWindow : Window
    {
        private const int WM_THREAD_FINISHED = User32.WM_USER + 1;
        private const int COLOR_RED = 0x000000FF;
        private const int COLOR_WHITE = 0x00FFFFFF;

        private Thread _firstThread = null;
        private Thread _secondThread = null;

        private ConcurrentQueue<QueueMessage> _messagesForFirstThread;
        private ConcurrentQueue<QueueMessage> _messagesForSecondThread;

        private WMReceiver _receiver;

        public MainWindow()
        {
            InitializeComponent();

            _receiver = new WMReceiver(FirstThreadMessageHandler);

            _messagesForFirstThread = new ConcurrentQueue<QueueMessage>();
            _messagesForSecondThread = new ConcurrentQueue<QueueMessage>();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(SecondThreadMessageHandler);
        }

        private IntPtr SecondThreadMessageHandler(IntPtr hwnd, int message, IntPtr WParam, IntPtr LParam, ref bool handled)
        {
            switch (message)
            {
                case WM_THREAD_FINISHED:
                    SecondButton.Content = "Start second thread";
                    _secondThread = null;
                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        private void FirstThreadMessageHandler(Message message)
        {
            switch (message.Msg)
            {
                case WM_THREAD_FINISHED:
                    FirstButton.Content = "Start first thread";
                    _firstThread = null;
                    break;
            }
        }

        private void ExecuteFirstThread(object args)
        {
            ThreadArguments threadArgs = args as ThreadArguments;
            int x = threadArgs.X;
            int y = threadArgs.Y;
            int size = threadArgs.Size;
            double windowHeight = threadArgs.WindowHeight;
            bool isDirectionDown = true;
            bool isRun = true;

            IntPtr deviceContext = User32.GetDC(threadArgs.Hwnd);
            Gdi32.SelectObject(deviceContext, Gdi32.GetStockObject(Gdi32.StockObjects.DC_BRUSH));
            Gdi32.SelectObject(deviceContext, Gdi32.GetStockObject(Gdi32.StockObjects.DC_PEN));

            while (isRun)
            {
                Gdi32.SetDCBrushColor(deviceContext, COLOR_RED);
                Gdi32.SetDCPenColor(deviceContext, COLOR_RED);
                Gdi32.Ellipse(deviceContext, x, y, x + size, y + size);

                Thread.Sleep(5);

                Gdi32.SetDCBrushColor(deviceContext, COLOR_WHITE);
                Gdi32.SetDCPenColor(deviceContext, COLOR_WHITE);
                Gdi32.Ellipse(deviceContext, x, y, x + size, y + size);

                if (y == 0) isDirectionDown = true;
                else if (y + size >= windowHeight) isDirectionDown = false;

                y += isDirectionDown ? 1 : -1;

                if (_messagesForFirstThread.TryDequeue(out QueueMessage message))
                {
                    switch (message.Type)
                    {
                        case MessageType.CHANGE_WINDOW_SIZES:
                            windowHeight = message.Value;
                            break;
                        case MessageType.CHANGE_THREAD_STATE:
                            isRun = !isRun;
                            break;
                    }
                }
            };

            User32.ReleaseDC(threadArgs.Hwnd, deviceContext);
            User32.PostMessage(threadArgs.ReceiverHandle, WM_THREAD_FINISHED, 0, 0);
        }

        private void ExecuteSecondThread(object args)
        {
            ThreadArguments threadArgs = args as ThreadArguments;
            int x = threadArgs.X;
            int y = threadArgs.Y;
            int size = threadArgs.Size;
            double windowHeight = threadArgs.WindowHeight;

            Graphics g = Graphics.FromHwnd(threadArgs.Hwnd);

            System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.Red);
            System.Drawing.Brush clearBrush = new SolidBrush(System.Drawing.Color.White);

            bool isDirectionDown = true;
            bool isRun = true;

            while (isRun)
            {
                g.FillEllipse(brush, x, y, size, size);
                Thread.Sleep(5);
                g.FillEllipse(clearBrush, x, y, size, size);

                if (y == 0) isDirectionDown = true;
                else if (y + size >= windowHeight) isDirectionDown = false;

                y += isDirectionDown ? 1 : -1;

                if (_messagesForSecondThread.TryDequeue(out QueueMessage message))
                {
                    switch (message.Type)
                    {
                        case MessageType.CHANGE_WINDOW_SIZES:
                            windowHeight = message.Value;
                            //g.Dispose(); maybe not need.

                            // Recreate graphics for draw in changed windows.
                            g = Graphics.FromHwnd(threadArgs.Hwnd);
                            break;
                        case MessageType.CHANGE_THREAD_STATE:
                            isRun = !isRun;
                            break;
                        default:
                            break;
                    }
                }
            };

            g.Dispose();
            User32.PostMessage(threadArgs.Hwnd, WM_THREAD_FINISHED, 0, 0);
        }

        private void ChangeFirstThreadState(object sender, RoutedEventArgs e)
        {
            if (_firstThread == null)
            {
                _firstThread = new Thread(new ParameterizedThreadStart(ExecuteFirstThread));
                _firstThread.Start(
                    new ThreadArguments(
                        hwnd: new WindowInteropHelper(this).Handle,
                        receiverHandle: _receiver.Handle,
                        x: (int)FirstButton.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0)).X + 2 * (int)FirstButton.Width,
                        y: 0,
                        size: 50,
                        windowHeight: (int)(Content as FrameworkElement).ActualHeight
                    )
                );
                FirstButton.Content = "Finish thread";
            }
            else
                // Send a message, that thread changed state (on null, off)
                _messagesForFirstThread.Enqueue(new QueueMessage(MessageType.CHANGE_THREAD_STATE));
        }

        private void ChangeSecondThreadState(object sender, RoutedEventArgs e)
        {
            if (_secondThread == null)
            {
                _secondThread = new Thread(new ParameterizedThreadStart(ExecuteSecondThread));
                _secondThread.Start(
                    new ThreadArguments(
                        hwnd: new WindowInteropHelper(this).Handle,
                        receiverHandle: _receiver.Handle,
                        x: (int)SecondButton.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0)).X + 2 * (int)SecondButton.Width,
                        y: 0,
                        size: 50,
                        windowHeight: (int)(Content as FrameworkElement).ActualHeight
                        )
                    );
                SecondButton.Content = "Finish thread";
            }
            else
                // Send a message, that thread changed state (on null, off).
                _messagesForSecondThread.Enqueue(new QueueMessage(MessageType.CHANGE_THREAD_STATE));
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            QueueMessage message = new QueueMessage(MessageType.CHANGE_WINDOW_SIZES, (int)(Content as FrameworkElement).ActualHeight);

            // Send a message, that window change size.
            _messagesForFirstThread.Enqueue(message);
            _messagesForSecondThread.Enqueue(message);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _firstThread.Abort();
            _secondThread.Abort();
        }
    }
}
