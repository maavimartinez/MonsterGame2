using System;

namespace Business.Exceptions
{
    public class CellAlreadyTakenException : ActionException
    {
        public CellAlreadyTakenException() : base("You can't make that move, there is another player there")
        {
        }
    }
}