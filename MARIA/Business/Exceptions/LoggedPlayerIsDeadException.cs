﻿using System;

namespace Business.Exceptions
{
    public class LoggedPlayerIsDeadException : BusinessException
    {
        public LoggedPlayerIsDeadException() : base("You are dead")
        {
        }
    }
}