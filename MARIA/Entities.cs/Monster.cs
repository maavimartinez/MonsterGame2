using System;

namespace Entities
{
    public class Monster : Player
    {
        public Monster()
        {
            HP = 100;
            AP = 100;
            isAlive = true;
        }

        public override string ToString()
        {
            return "Monster";
        }

    }
    
}