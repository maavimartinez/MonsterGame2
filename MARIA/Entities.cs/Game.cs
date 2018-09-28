using System;
using System.Collections.Generic;
using System.Threading;

namespace Entities
{
    public class Game
    {
        public List<Player> Players { get; set; }

        public DateTime StartTime { get; set; }

        public bool isOn { get; set;}

        public Game()
        {
            Players = new List <Player>();
            isOn = false;
        }

    }
}