namespace firstLab
{
    public class QueueMessage
    {
        private MessageType type;
        private int value;

        public enum MessageType
        {
            CHANGE_WINDOW_SIZES = 0,
            CHANGE_THREAD_STATE
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

        public MessageType Type
        {
            get => type;
            set => type = value;
        }
        public int Value
        {
            get => value;
            set => this.value = value;
        }
    }
}