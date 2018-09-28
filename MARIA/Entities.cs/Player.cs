using System;

namespace Entities
{
    public abstract class Player
    {

        public Client Client { get; set; }

        public int  HP       { get; set; }

        public int  AP       { get; set; }

        public bool isAlive  { get; set; }

        public int  NumOfActions { get; set; }

        public Cell Position { get;set; }

        public Player(){}

    }
    
}