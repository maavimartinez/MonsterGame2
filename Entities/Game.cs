using System;
using System.Collections.Generic;

namespace Entities
{
    public class Game //MOSTRAR GANADOR DE LA PARTIDA CON LA LOGICA,COMO CONTROLAR LOS TURNOS
    {

        public int   NumberOfPlayers { get; set; }

        public int   Duration        { get; set; }

        public int   JoiningLapse    { get; set; }

        public int   Turn            { get; set; }

        public Board Board           { get; set; }

        public ICollection<Player> Players  { get; set; }

        
        public Game()
        {
            Board = new Board();
            Players = new List<Player>();
        }

    }

}
    