using System;
using Entities;

namespace UI
{
    public class UIBoard
    {

        public void DrawBoard(Board board){
            int a = 64;
            char myChar;
            Console.WriteLine("   1   2   3   4   5   6   7   8 ");
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            for (int i = 0; i < board.Width; i++) {
                a++;
                myChar = (char) a;
                Console.Write(myChar + "|");
                for (int j = 0; j < board.Height; j++) {
                    Console.Write(board.Cells[i,j] + "|");
                }
                Console.WriteLine("");
                if (i < board.Width - 1) {
                    Console.WriteLine(" +---+---+---+---+---+---+---+---+");
                }
            }
            Console.WriteLine(" +---+---+---+---+---+---+---+---+");
        } 

    }
}
