using System;

namespace Business.Exceptions
{
    public class LastPlayerAbandonedGameException : BusinessException
    {
        public LastPlayerAbandonedGameException() : base("")
        {
        }
    }
}