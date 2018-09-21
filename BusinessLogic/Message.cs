using System;

namespace Business
{
    public class Message
    {
        public Message()
        {

        }

        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
    }
}