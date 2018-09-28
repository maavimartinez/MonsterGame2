using System;

namespace Business.Exceptions
{
    public class RoleNotChosenException : BusinessException
    {
        public RoleNotChosenException() : base("You have to choose a role first")
        {
        }
    }
}