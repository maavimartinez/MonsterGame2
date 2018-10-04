using System;

namespace Business.Exceptions
{
    public class MovementOutOfBoundsException : ActionException
    {
        public MovementOutOfBoundsException() : base("Movement out of bounds")
        {
        }
    }
}