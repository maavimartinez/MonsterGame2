using System;
using System.Collections.Generic;

namespace Entities
{
    public class Game
    {
        public List<Player> Players { get; set; }

        public List<string> PlayersThatLeft { get; set; }

        public List<string> PlayersThatDied { get; set; }

        public DateTime StartTime { get; set; }

        public double  LimitJoiningTime { get; set; }

        public bool isOn { get; set; }

        public string Result { get; set; }

        public Game()
        {
            LimitJoiningTime = 0.3;
            Players = new List <Player>();
            isOn = false;
            Result = "";
        }

    }
}