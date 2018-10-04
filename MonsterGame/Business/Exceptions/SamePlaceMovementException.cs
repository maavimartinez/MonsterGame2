using System;

namespace Business.Exceptions
{
    public class SamePlaceMovementException : ActionException
    {
        public SamePlaceMovementException() : base("You are already there")
        {
        }
    }
}