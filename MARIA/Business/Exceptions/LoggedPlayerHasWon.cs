using System;

namespace Business.Exceptions
{
    public class LoggedPlayerHasWon : BusinessException
    {
        public LoggedPlayerHasWon() : base("You have won the game")
        {
        }
    }
}