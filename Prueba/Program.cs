using System;
using Entities;
using UI;
using BusinessLogic;

namespace Prueba
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board = new Board();

            BoardLogic bl = new BoardLogic(board);
            bl.InitializeBoard();

            UIBoard ui = new UIBoard();
            ui.DrawBoard(board);
        }
        
    }
}
