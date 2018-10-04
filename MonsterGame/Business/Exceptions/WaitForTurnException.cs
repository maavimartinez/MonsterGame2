using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Exceptions
{
    public class WaitForTurnException : ActionException
    {
        public WaitForTurnException() : base("Wait until everyone completes their turn")
        {
        }
    }
}
