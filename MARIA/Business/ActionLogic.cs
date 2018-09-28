using System;
using Entities;
using Business.Exceptions;
using System.Runtime.Serialization;

namespace Business
{
    public class ActionLogic
    {
        const int RADIUS = 1;
        const int WIDTH = 8;
        const int HEIGHT = 8;

        private Board board;

        public ActionLogic(Board brd)
        {
            board = brd;
        }

        


    }
}