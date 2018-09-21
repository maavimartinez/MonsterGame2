using System;
using Entities;

namespace BusinessLogic
{
    
    public class BoardLogic
    {

        private Board board;

        public BoardLogic(Board brd)
        {
            board = brd;
        }

        public void InitializeBoard(){
            for(int i=0; i<board.Width ; i++)
            {
                for(int j=0; j<board.Height ; j++)
                {
                    Cell aux = new Cell();
                    aux.Row = j;
                    aux.Column = i;
                    board.Cells[i,j] = aux;                    
                }
            }
        }


    }

}
