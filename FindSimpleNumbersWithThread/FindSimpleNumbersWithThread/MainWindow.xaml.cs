using SysLab6;
using System;
using System.Collections.Concurrent;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FindSimpleNumbersWithThread.QueueMessage;

namespace FindSimpleNumbersWithThread
{
    public partial class MainWindow : Window
    {
        private const int WM_THREAD_FINISHED = User32.WM_USER + 1;
        private const int WM_THREAD_FOUND_PRIMARY_NUMBER = User32.WM_USER + 2;

        private bool _isThreadsRun = false;
        private Dictionary<int, ConcurrentQueue<QueueMessage>> _messages;
        private QueueMessage _changeThreadStateMessage = new QueueMessage(MessageType.CHANGE_THREAD_STATE);

        public MainWindow()
        {
            InitializeComponent();

            _messages = new Dictionary<int, ConcurrentQueue<QueueMessage>>();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(ThreadMessagesHandler);
        }

        private IntPtr ThreadMessagesHandler(IntPtr hwnd, int message, IntPtr WParam, IntPtr LParam, ref bool handled)
        {
            switch (message)
            {
                case WM_THREAD_FOUND_PRIMARY_NUMBER:
                    SendText(WParam.ToString());
                    ResultsTextBox.ScrollToEnd();
                    
                    handled = true;
                    break;
                case WM_THREAD_FINISHED:
                    _messages.Remove((int)WParam);
                    if (_messages.Count == 0)
                    {
                        ChangeStateThreadButtons.Content = "Start find prime numbers";
                        ChangeStateThreadButtons.IsEnabled = true;
                    }
                    LabelRunThreads.Content = $"Threads run: {_messages.Count}";

                    handled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        private void SendText(string message)
        {
            ResultsTextBox.AppendText(message);
            ResultsTextBox.AppendText(Environment.NewLine);
        }

        private void ExecuteThread(object args)
        {
            ThreadArguments threadArgs = args as ThreadArguments;
            int start = threadArgs.Start;
            int end = threadArgs.End;
            int threadId = Thread.CurrentThread.ManagedThreadId;
            ConcurrentQueue<QueueMessage> messages = threadArgs.Messages;
            bool isRun = true;
            IntPtr hwnd = threadArgs.Hwnd;

            for (int i = start; i < end && isRun; ++i)
            {
                if (IsNumberPrime(i))
                    User32.PostMessage(hwnd, WM_THREAD_FOUND_PRIMARY_NUMBER, i, 0);

                if (messages.TryDequeue(out QueueMessage message))
                {
                    switch (message.Type)
                    {
                        case MessageType.CHANGE_THREAD_STATE:
                            isRun = false;
                            break;
                    }
                }
            }

            User32.PostMessage(hwnd, WM_THREAD_FINISHED, threadId, 0);
        }

        private void ChangeThreadsState(object sender, RoutedEventArgs e)
        {
            if (_isThreadsRun)
            {
                _isThreadsRun = !_isThreadsRun;
                ChangeStateThreadButtons.IsEnabled = false;

                foreach (KeyValuePair<int, ConcurrentQueue<QueueMessage>> pair in _messages)
                    pair.Value.Enqueue(new QueueMessage(MessageType.CHANGE_THREAD_STATE));

                return;
            }

            ChangeStateThreadButtons.Content = "Stop find prime numbers";
            _isThreadsRun = true;
            ResultsTextBox.Clear();

            int start = int.Parse(StartRange.Text);
            int end = int.Parse(EndRange.Text);
            int threadsCount = int.Parse(ThreadsCount.Text);

            if (end < start || threadsCount <= 0)
                return;

            int deltaRange = (end - start) / threadsCount;

            for (int i = 0; i < threadsCount; ++i)
            {
                Thread thread = new Thread(ExecuteThread);
                _messages.Add(thread.ManagedThreadId, new ConcurrentQueue<QueueMessage>());

                thread.Start(
                    new ThreadArguments(
                        hwnd: new WindowInteropHelper(this).Handle,
                        start: start + deltaRange * i,
                        end: start + deltaRange * (i + 1) - 1,
                        messages: _messages[thread.ManagedThreadId]
                    )
                );
            }

            LabelRunThreads.Content = $"Threads run: {_messages.Count}";
        }

        private bool IsNumberPrime(int num)
        {
            //if (number == 2 || number == 3)
            //    return true;

            //if (number <= 1 || number % 2 == 0 || number % 3 == 0)
            //    return false;

            //for (int i = 5; i * i <= number; i += 6)
            //{
            //    if (number % i == 0 || number % (i + 2) == 0)
            //        return false;
            //}

            //return true;

            for (int i = 2; i < num; i++)
            {
                if (num % i == 0) return false;
            }
            return num != 2;
        }

        private void FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (KeyValuePair<int, ConcurrentQueue<QueueMessage>> pair in _messages)
                pair.Value.Enqueue(new QueueMessage(MessageType.CHANGE_THREAD_STATE));
        }
    }
}
