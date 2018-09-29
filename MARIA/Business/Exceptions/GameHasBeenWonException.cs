using System;

namespace Business.Exceptions
{
    public class GameHasBeenWonException : BusinessException
    {
        public GameHasBeenWonException(string message) : base(message)
        {
        }
    }
}