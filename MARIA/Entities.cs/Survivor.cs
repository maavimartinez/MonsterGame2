using System;

namespace Entities
{
    public class Survivor : Player
    {
        
        public Survivor() 
        {
            HP = 20;
            AP = 5;
            isAlive = true;
        }

        public override string ToString()
        {
            return "Survivor";
        }

    }
    
}