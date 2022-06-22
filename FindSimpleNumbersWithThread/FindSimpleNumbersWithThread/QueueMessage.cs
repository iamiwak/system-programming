using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindSimpleNumbersWithThread
{
    public class QueueMessage
    {
        private MessageType type;
        private int value;

        public enum MessageType
        {
            CHANGE_THREAD_STATE = 0
        }

        public QueueMessage(MessageType type)
        {
            this.type = type;
        }

        public QueueMessage(MessageType type, int value)
        {
            this.type = type;
            this.value = value;
        }

        public MessageType Type { get => type; set => type = value; }

        public int Value { get => value; set => this.value = value; }
    }
}
