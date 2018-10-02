using Business.Exceptions;
using System.Collections.Generic;
using Entities;
using Persistence;
using System;
using UI;
using System.Text.RegularExpressions;
using System.Linq;

namespace Business
{
    public class GameController
    {

        private Store Store { get; set; }
        private Server Server { get; set; }
        private ActionController ActionController { get; set; }
        private string ActiveGameResult { get; set; }

        private readonly object loginLock = new object();
        private readonly object selectRoleLock = new object();
        private readonly object joinGameLock = new object();
        private readonly object doActionLock = new object();
        private readonly object removePlayerFromGameLock = new object();

        public GameController(Store store)
        {
            Store = store;
            Server = new Server();
            ActionController = new ActionController(store);
        }

        public string Login(Client client)
        {
            lock (loginLock)
            {
                if (!Store.ClientExists(client))
                    Store.AddClient(client);
                Client storedClient = Store.GetClient(client.Username);
                bool isValidPassword = storedClient.ValidatePassword(client.Password);
                bool isClientConnected = Server.IsClientConnected(client);
                if (isValidPassword && isClientConnected)
                    throw new ClientAlreadyConnectedException();
                return isValidPassword ? Server.ConnectClient(storedClient) : "";
            }
        }

        public Client GetLoggedClient(string userToken)
        {
            lock (loginLock)
            {
                Client loggedUser = Server.GetLoggedClient(userToken);
                if (loggedUser == null)
                    throw new ClientNotConnectedException();
                return loggedUser;
            }
        }

        public List<Client> GetLoggedClients()
        {
            lock (loginLock)
            {
                return Server.GetLoggedClients();
            }
        }

        public Player GetLoggedPlayer(string username)
        {
            lock (joinGameLock)
            {
                return Store.GetLoggedPlayer(username);
            }
        }

        public List<Player> GetLoggedPlayers()
        {
            lock (loginLock)
            {
                List<Client> loggedClients = Server.GetLoggedClients();
                List<Player> ret = new List<Player>();
                foreach(Client cl in loggedClients)
                {
                    foreach(Player pl in Store.AllPlayers)
                    {
                        if(cl.Username.Equals(pl.Client.Username))
                            ret.Add(pl);
                    }
                }
                return ret;
            }
        }

        public List<Client> GetClients()
        {
            lock (loginLock)
            {
                return Store.GetClients();
            }
        }

        public List<Player> GetCurrentPlayers()
        {
            lock (loginLock)
            {
                try
                {
                    return Store.ActiveGame.Players;
                }
                catch (NullReferenceException)
                {
                    return new List<Player>();
                }
            }
        }

        public void DisconnectClient(string token)
        {
            lock (loginLock)
            {
                Server.DisconnectClient(token);
            }
        }

        public void SelectRole(Client loggedClient, string role)
        {

            lock(selectRoleLock)
            {
                if (loggedClient == null)
                    throw new ClientNotConnectedException();
                CreatePlayerWithRole(loggedClient, role);
            }
        }

        private void CreatePlayerWithRole(Client loggedClient, string role)
        {
            Player player;
            if(role == "Survivor")
            {
                CheckIfGameHasMonster();
                player = new Survivor();
            }else
            {
                player = new Monster();
            }
            player.Client = loggedClient;
            Store.AllPlayers.Add(player);
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

        public void JoinGame(string usernameFrom)
        {
            lock (joinGameLock)
            {
                Player logged = Store.GetLoggedPlayer(usernameFrom);
                if (logged == null) throw new RoleNotChosenException();
                InitializeGame();
                JoinPlayerToGame(logged);
            }
        }

        private void InitializeGame()
        {
            if (Store.ActiveGame == null) Store.ActiveGame = new Game();
            if (Store.Board      == null) Store.Board = new Board();
            if (Store.ActiveGame.Players.Count == 0)
            {
                Store.ActiveGame.isOn = true;
                Store.ActiveGame.StartTime = DateTime.Now;
                ActiveGameResult = "";
                Store.Board.InitializeBoard();
            }
        }

        private void JoinPlayerToGame(Player loggedPlayer)
        {
            if (Store.ActiveGame.Players.Count < 4)
            {
                Store.ActiveGame.Players.Add(loggedPlayer);
                loggedPlayer.NumOfActions = GetMaxTurn();
                LocatePlayersInBoard();
            }
            else if(TimeHasPassed(Store.ActiveGame.LimitJoiningTime))
            {
                var remainingTime = DateTime.Now - Store.ActiveGame.StartTime;
                throw new FullGameException("Game is full, try again in " + remainingTime.ToString());
            }else
            {
                var remainingTime = DateTime.Now - Store.ActiveGame.StartTime;
                throw new FullGameException("You can no longer join this game, try again in " + remainingTime.ToString());
            }
        }

        private void LocatePlayersInBoard()
        {
            foreach(Player pl in Store.ActiveGame.Players)
            {
                if(pl.Position == null)
                {
                    int[] pos = GetPlayerPosition();
                    pl.Position = Store.Board.Cells[pos[0], pos[1]];
                    Store.Board.Cells[pos[0],pos[1]].Player = pl;
                }
            }
        }

        private int[] GetPlayerPosition()
        {
            int[] pos = new int[2];
            Random ran = new Random();
            bool exit = false;
            while(!exit)
            {
                int x = ran.Next(0, 8);
                int y = ran.Next(0, 8);
                if(Store.Board.Cells[x,y].Player == null)
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

        public List<string> DoAction(string usernameFrom, string action)
        {
            lock (doActionLock)
            {
                List<string> ret = new List<string>();
                if (!Store.ActiveGame.isOn)
                {
                    ret.Add("FINISHED");
                    ret.Add(ActiveGameResult);
                }
                Player player = GetLoggedPlayer(usernameFrom);
                if (!player.isAlive) throw new LoggedPlayerIsDeadException();
                ret = ret.Concat(ActionController.DoAction(player, action)).ToList();
                if (CheckIfGameHasEnded() != null) ret = ret.Concat(CheckIfGameHasEnded()).ToList();
                return ret;
            }
        }

        private bool TimeHasPassed(int minutes)
        {
            DateTime startTime = Store.ActiveGame.StartTime;
            DateTime endTime = startTime.AddMinutes(minutes);
            DateTime now = DateTime.Now;
            if (now < endTime)
            {
                return false;
            }else
            {
                return true;
            }
        }

        public List<string> TimesOut()
        {
            if (Store.ActiveGame != null && Store.ActiveGame.isOn && TimeHasPassed(3)){
                return GetGameResultByTimeOut();
            }
            List<string> ret = new List<string>();
            ret.Add("timesNotOut");
            return ret;
        }

        public List<string> GetGameResultByTimeOut()
        {
            string aliveMonsters = "";
            string aliveSurvivors = "";
            int alivePlayers = 0;
            foreach (Player pl in Store.AllPlayers)
            {
                if (pl.isAlive)
                {
                    alivePlayers++;
                    if (pl is Monster) aliveMonsters = aliveMonsters + pl.Client.Username + ",";
                    if (pl is Survivor) aliveSurvivors = aliveSurvivors + pl.Client.Username + ",";
                }
            }
            if(aliveSurvivors != "")
            {
                aliveSurvivors.Trim(',');
                ActiveGameResult = aliveSurvivors + "won !";
                return EndGame();
            }else if(aliveSurvivors == "")
            {
                ActiveGameResult = "Nobody won :(";
                return EndGame();
            }
            return null;
        }

        public List<string> RemovePlayerFromGame(string username)
        {
            lock (removePlayerFromGameLock)
            {
                List<string> ret = new List<string>();
                Player player = GetLoggedPlayer(username);
                Store.ActiveGame.Players.Remove(player);
                Store.AllPlayers.Remove(player);
                if (Store.AllPlayers.Count > 0)
                {
                    return CheckIfGameHasEnded();
                }
                else if (Store.AllPlayers.Count == 0)
                {
                    ActiveGameResult = "Game has finished";
                    return EndGame();
                }
                return ret;
            }
        }

        private List<string> CheckIfGameHasEnded()
        {
            string aliveMonsters = "";
            string aliveSurvivors = "";
            int alivePlayers = 0;
            foreach (Player pl in Store.AllPlayers)
            {
                if (pl.isAlive)
                {
                    alivePlayers++;
                    if (pl is Monster) aliveMonsters = aliveMonsters + pl.Client.Username + ",";
                    if (pl is Survivor) aliveSurvivors = aliveSurvivors + pl.Client.Username + ",";
                }
            }
            if (aliveMonsters == "")
            {
                aliveSurvivors.Trim(',');
                ActiveGameResult = aliveSurvivors + "won !";
                return EndGame();
            }
            else if (alivePlayers == 1 && aliveSurvivors == "" && TimeHasPassed(Store.ActiveGame.LimitJoiningTime))
            {
                aliveMonsters.Trim(',');
                ActiveGameResult = aliveMonsters + "won !";
                return EndGame();
            }
            return null;
        }

        public List<string> EndGame() {
            Game game = Store.ActiveGame;
            if (game != null)
            {
                game.StartTime = DateTime.MinValue;
                game.isOn = false;
                game.Result = "";
                Store.ActiveGame.Players.Clear();
                List<string> ret = new List<string>();
                ret.Add("FINISHED");
                ret.Add(ActiveGameResult);
                return ret;             
            }
            return null;
        }

        public List<string> GetOnGameUsernamesAndStatus()
        {
            List<string> ret = new List<string>();
            ret.Add("PLAYERS");
            foreach (Player pl in Store.ActiveGame.Players)
            {
                string status = (pl.isAlive) ? "Alive" : "Dead";
                ret.Add(pl.Client.Username + "(" + status + ")");
            }
            return ret;
        }
    }
}