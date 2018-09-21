using System;

namespace Entities
{
    public class Cell
    {

        public int   Row  { get; set; }

        public int   Column  { get; set; }

        public Player Player  { get; set; }

        public Cell(){}

        public override string ToString()
        {
            if(Player == null)
            {
                return " • ";
            } 
            else{
                return Player.User.ToString();
            } 
        }

    }
    
}