using System;

namespace Business.Exceptions
{
    public class FullGameException : BusinessException
    {
        public FullGameException(string msg) : base(msg)
        {
        }
    }
}