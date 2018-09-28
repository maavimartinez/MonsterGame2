using System;
using System.Collections.Generic;

namespace Entities
{
    public class Board
    {
        
        public int     Width  { get; set; }

        public int     Height { get; set; }

        public Cell[,] Cells { get; set; }
        
        public Board()
        {
            Width = 8;
            Height = 8;
            Cells = new Cell[Width,Height];
        }

        public void InitializeBoard(){
            for(int i=0; i<Width ; i++)
            {
                for(int j=0; j<Height ; j++)
                {
                    Cell aux = new Cell();
                    aux.Y = j;
                    aux.X = i;
                    Cells[i,j] = aux;                    
                }
            }
        }

    }
    
}
