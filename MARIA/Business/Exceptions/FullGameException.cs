using System;

namespace Business.Exceptions
{
    public class FullGameException : BusinessException
    {
        public FullGameException() : base("The game is full")
        {
        }
    }
}