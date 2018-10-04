using System;
using System.Linq;
using Business.Exceptions;
using Entities;
using Persistence;

namespace Business
{
    public class PlayerLogic
    {

        private Store Store { get; set; }

        public PlayerLogic(Store store)
        {
            Store = store;
        }

        public void SelectRole(Client loggedClient, string role)
        {
            Player player;
            if (role == "Survivor")
            {
                CheckIfGameHasMonster();
                player = new Survivor();
            }
            else
            {
                player = new Monster();
            }
            player.Client = loggedClient;
            Store.AllPlayers.Add(player);
        }

        public void JoinPlayerToGame(Player loggedPlayer)
        {
            if (Store.ActiveGame.Players.Count < 4 && !TimeHasPassed(Store.ActiveGame.LimitJoiningTime))
            {
                Store.ActiveGame.Players.Add(loggedPlayer);
                loggedPlayer.NumOfActions = GetMaxTurn();
                LocatePlayersInBoard();
            }
            else if (!TimeHasPassed(Store.ActiveGame.LimitJoiningTime))
            {
                var remainingTime = Store.ActiveGame.StartTime.AddMinutes(3) - (DateTime.Now - Store.ActiveGame.StartTime);
                throw new FullGameException("Game is full, try again at " + remainingTime.ToString("HH:mm"));
            }
            else
            {
                var remainingTime = Store.ActiveGame.StartTime.AddMinutes(3) - (DateTime.Now - Store.ActiveGame.StartTime);
                throw new FullGameException("You can no longer join this game, try again at " + remainingTime.ToString("HH:mm"));
            }
        }

        private void CheckIfGameHasMonster()
        {
            if (Store.ActiveGame != null && Store.ActiveGame.Players.Count() == 3)
            {
                int countMonsters = 0;
                foreach (Player pl in Store.ActiveGame.Players)
                {
                    if (pl is Monster) countMonsters++;
                }
                if (countMonsters == 0) throw new NoMonstersInGameException();
            }
        }

        private bool TimeHasPassed(double minutes)
        {
            DateTime startTime = Store.ActiveGame.StartTime;
            DateTime endTime = startTime.AddMinutes(minutes);
            DateTime now = DateTime.Now;
            if (now < endTime)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void LocatePlayersInBoard()
        {
            foreach (Player pl in Store.ActiveGame.Players)
            {
                if (pl.Position == null)
                {
                    int[] pos = GeneratePlayerPosition();
                    pl.Position = Store.Board.Cells[pos[0], pos[1]];
                    Store.Board.Cells[pos[0], pos[1]].Player = pl;
                }
            }
        }

        private int[] GeneratePlayerPosition()
        {
            int[] pos = new int[2];
            Random ran = new Random();
            bool exit = false;
            while (!exit)
            {
                int x = ran.Next(0, 8);
                int y = ran.Next(0, 8);
                if (Store.Board.Cells[x, y].Player == null)
                {
                    pos[0] = x;
                    pos[1] = y;
                    exit = true;
                }
            }
            return pos;
        }

        private int GetMaxTurn()
        {
            int max = 0;
            foreach (Player pl in Store.ActiveGame.Players)
            {
                if (pl.NumOfActions > max) max = pl.NumOfActions;
            }
            if (max % 2 == 1) max = max - 1;
            return max;
        }

    }
}
