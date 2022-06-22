using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindSimpleNumbersWithThread
{
    public class ThreadArguments
    {
        private int _start;
        private int _end;
        private IntPtr _hwnd;
        private ConcurrentQueue<QueueMessage> _messages;

        public ThreadArguments(IntPtr hwnd, int start, int end, ConcurrentQueue<QueueMessage> messages)
        {
            _hwnd = hwnd;
            _start = start;
            _end = end;
            Messages = messages;
        }

        public int Start { get => _start; set => _start = value; }

        public int End { get => _end; set => _end = value; }

        public IntPtr Hwnd { get => _hwnd; set => _hwnd = value; }

        public ConcurrentQueue<QueueMessage> Messages { get => _messages; set => _messages = value; }
    }
}
