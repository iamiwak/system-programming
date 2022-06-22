using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallenBalls
{
    public class ThreadArguments
    {
        private IntPtr hwnd;
        private IntPtr receiverHandle;
        private int width;
        private int height;
        private int row;

        public ThreadArguments(IntPtr hwnd, IntPtr receiverHandle)
        {
            this.hwnd = hwnd;
            this.receiverHandle = receiverHandle;
            //this.width = width;
            //this.height = height;
        }

        public ThreadArguments(IntPtr hwnd, IntPtr receiverHandle, int row)
        {
            this.hwnd = hwnd;
            this.receiverHandle = receiverHandle;
            this.row = row;
            //this.width = width;
            //this.height = height;
        }

        public IntPtr Hwnd
        {
            get => hwnd;
            set
            {
                hwnd = value;
            }
        }

        public IntPtr ReceiverHandle
        {
            get => receiverHandle;
            set
            {
                receiverHandle = value;
            }
        }

        public int Width
        {
            get => width;
            set
            {
                width = value;
            }
        }

        public int Height
        {
            get => height;
            set
            {
                height = value;
            }
        }

        public int Row
        {
            get => row;
            set
            {
                row = value;
            }
        }
    }
}
