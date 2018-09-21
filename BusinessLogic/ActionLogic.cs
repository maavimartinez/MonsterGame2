using System;
using Entities;

namespace BusinessLogic
{
    public class ActionLogic
    {
        const int RADIUS = 1;
        const int WIDTH  = 8;
        const int HEIGHT = 8;

        private Board board;

        public ActionLogic(Board brd){
            board = brd;
        }

        public void Move(Player player, string pos)
        {       
            int row = Convert.ToInt32(pos[0]) - 65; 
            int column = pos[1]-1;  
            CheckValidMovement(player, row, column);
            RemovePlayerFromCell(player);
            LocatePlayerOnCell(player, row, column);
            CheckIfSomeoneIsNear(player);
        }

        private void CheckValidMovement(Player player, int row, int column)
        {
            var lowerX = Math.Max(0, player.Position.Column - RADIUS);
            var upperX = Math.Min(WIDTH - 1, player.Position.Column + RADIUS);
            var lowerY = Math.Max(0, player.Position.Row - RADIUS);
            var upperY = Math.Min(HEIGHT - 1, player.Position.Row + RADIUS);     
            if(column<lowerX || column>upperX || row<lowerY || row>upperY) 
                throw new Exception("Movement out of bounds");
            if(player.Position.Column == column && player.Position.Row == row)
                throw new Exception("You are already there");
        }

        private void LocatePlayerOnCell(Player player, int row, int column)
        {
            player.Position = board.Cells[row,column];
            board.Cells[row,column].Player = player;
        }

        private void RemovePlayerFromCell(Player player)
        {
            int  row = player.Position.Row;
            int  column = player.Position.Column;
            board.Cells[row,column].Player = null;
        }

        private void CheckIfSomeoneIsNear(Player player)
        {
            var lowerX = Math.Max(0, player.Position.Column - RADIUS);
            var upperX = Math.Min(WIDTH - 1, player.Position.Column + RADIUS);
            var lowerY = Math.Max(0, player.Position.Row - RADIUS);
            var upperY = Math.Min(HEIGHT - 1, player.Position.Row + RADIUS);     
            for(int i = lowerX; i <= upperX; ++i)
            {
                for(int j = lowerY; j <= upperY; ++j)
                {
                    if(board.Cells[i,j].Player != null)
                    {
                        //avisarle al player qué jugador está cerca
                    } 
                }
            }        
        }

        public void Attack(Player Attacker, Player Defender)
        {
            if(Attacker.GetType() == typeof(Survivor) && Defender.GetType() == typeof(Survivor)) 
                throw new Exception("Survivor can't attack survivors");
            Defender.HP = Defender.HP - Attacker.AP;
            //CheckIfPlayerIsDead(Defender);
        }


    }

}