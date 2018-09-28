using System;

namespace Business.Exceptions
{
    public class ActionException : Exception
    {
        public ActionException(string message) : base(message)
        {
        }
    }
}