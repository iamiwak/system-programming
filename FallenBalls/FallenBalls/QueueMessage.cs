using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallenBalls
{
    public class QueueMessage
    {
        private MessageType type;
        private int value;

        public enum MessageType
        {
            ROW,
            FINISH_THREAD
        }

        public QueueMessage(MessageType type) => this.type = type;

        public QueueMessage(MessageType type, int value)
        {
            this.type = type;
            this.value = value;
        }

        public MessageType Type { get; set; }

        public int Value { get; set; }
    }
}
