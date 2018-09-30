using System;

namespace Business.Exceptions
{
    public class NoMonstersInGameException : BusinessException
    {
        public NoMonstersInGameException() : base("There has to be at least one Monster per game. To proceed please choose Monster.")
        {
        }
    }
}