using System;

namespace Business.Exceptions
{
    public class CellAlreadyContainsAPlayerException : ActionException
    {
        public CellAlreadyContainsAPlayerException() : base("Cell is taken by another player")
        {
        }
    }
}