using System;

namespace Entities
{
    public class Monster : Player
    {
        public Monster()
        {
            HP = 100;
            AP = 10;
        }

        public override string ToString()
        {
            return "M";
        }

    }
    
}