using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SysLab6
{
    /// <summary>
    /// Класс, реализующий пользовательскую обработку оконных сообщений операционной системы Windows.
    /// </summary>
    public class WMReceiver : NativeWindow
    {
        /// <summary>
        /// Объявление типа пользовательского метода, ответственного за обработку сообщения в форме.
        /// </summary>
        /// <param name="message">Some message</param>
        public delegate void MessageHandler(Message message);

        /// <summary>
        /// Ссылка на пользовательский метод-обработчик сообщений.
        /// </summary>
        private MessageHandler MessageHandlerMethod;

        /// <summary>
        /// Конструктор класса обработчика сообщений.
        /// </summary>
        /// <param name="MessageHandlerMethod">Handler method</param>
        public WMReceiver(MessageHandler MessageHandlerMethod)
        {
            this.MessageHandlerMethod = MessageHandlerMethod;
            // Создание дескриптора (this.Handle), его нужно указать в вызове 
            // системной функции PostMessage, чтобы именно данный объект получил сообщение
            CreateHandle(new CreateParams());
        }

        // Метод класса, ответственный за получение сообщений
        protected override void WndProc(ref Message Msg)
        {
            // Вызов пользовательского обработчика сообщения
            MessageHandlerMethod(Msg);
            // Вызов обработчика сообщения родительского класса
            base.WndProc(ref Msg);
        }
    }
}