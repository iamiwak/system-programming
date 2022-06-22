using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace firstLab
{
    public class ThreadArguments
    {
        private int x;
        private int y;
        private int size;
        private int windowHeight;
        private IntPtr hwnd;
        private IntPtr receiverHandle;

        public ThreadArguments() { }

        public ThreadArguments(IntPtr hwnd, IntPtr receiverHandle, int x, int y, int size, int windowHeight)
        {
            this.hwnd = hwnd;
            this.receiverHandle = receiverHandle;
            this.x = x;
            this.y = y;
            this.size = size;
            this.windowHeight = windowHeight;
        }

        public int X
        {
            get => x;
            set => x = value;
        }

        public int Y
        {
            get => y;
            set => y = value;
        }

        public int Size
        {
            get => size;
            set => size = value;
        }

        public int WindowHeight
        {
            get => windowHeight;
            set => windowHeight = value;
        }

        public IntPtr Hwnd
        {
            get => hwnd;
            set => hwnd = value;
        }
        public IntPtr ReceiverHandle
        {
            get => receiverHandle;
            set => receiverHandle = value;
        }
    }
}
