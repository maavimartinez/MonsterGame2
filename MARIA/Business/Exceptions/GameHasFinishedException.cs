using System;

namespace Business.Exceptions
{
    public class GameHasFinishedException : BusinessException
    {
        public GameHasFinishedException(string message) : base(message)
        {
        }
    }
}