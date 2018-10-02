namespace Entities
{
    public class Cell
    {

        public int   X { get; set; }

        public int   Y  { get; set; }

        public Player Player  { get; set; }

        public Cell(){}

        public override string ToString()
        {
            if(Player == null)
            {
                return "      ";
            } 
            else{
                return Player.Client.ToString();
            } 
        }

    }
    
}