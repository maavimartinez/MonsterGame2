using System;
using System.Text.RegularExpressions;
using Entities;
using Business.Exceptions;

namespace Business
{
    public class GameLogic
    {
        private Board Board;

        private Game Game;

        private ActionLogic ActionLogic;

        public GameLogic(Board brd, Game gm)
        {
            Board = brd;
            ActionLogic = new ActionLogic(brd);
            Game = gm;
        }

        

        //end game, show winner
    }

}