namespace Entities
{
    public class Monster : Player
    {
        public Monster()
        {
            HP = 100;
            AP = 10;
            isAlive = true;
        }

        public override string ToString()
        {
            return "Monster";
        }

    }
    
}