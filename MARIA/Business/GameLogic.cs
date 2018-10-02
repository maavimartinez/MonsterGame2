using System.Collections.Generic;
using System;
using System.Linq;
using Business.Exceptions;
using Entities;
using Persistence;

namespace Business
{
    public class GameLogic
    {

        public Store Store { get; set; }
        private Server Server { get; set; }
        private ActionLogic ActionLogic { get; set; }
        private PlayerLogic PlayerLogic { get; set; }
        private string ActiveGameResult { get; set; }

        private readonly object loginLock = new object();
        private readonly object selectRoleLock = new object();
        private readonly object joinGameLock = new object();
        private readonly object doActionLock = new object();
        private readonly object removePlayerFromGameLock = new object();

        public GameLogic(Store store)
        {
            Store = store;
            Server = new Server();
            ActionLogic = new ActionLogic(Store);
            PlayerLogic = new PlayerLogic(Store);
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

            lock (selectRoleLock)
            {
                if (loggedClient == null)
                    throw new ClientNotConnectedException();
                PlayerLogic.SelectRole(loggedClient, role);
            }
        }

        public void JoinGame(string usernameFrom)
        {
            lock (joinGameLock)
            {
                Player logged = Store.GetLoggedPlayer(usernameFrom);
                if (logged == null) throw new RoleNotChosenException();
                InitializeGame();
                PlayerLogic.JoinPlayerToGame(logged);
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
                ret = ret.Concat(ActionLogic.DoAction(player, action)).ToList();
                List<string> ended = CheckIfGameHasEnded();
                if (ended != null) ret = ret.Concat(ended).ToList();
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

        public void TimesOut()
        {
            if (Store.ActiveGame != null && Store.ActiveGame.isOn && TimeHasPassed(10))
            {
                throw new TimesOutException("");
            }
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
                ActiveGameResult = aliveSurvivors + " won !";
                return EndGame();
            }
            else if(aliveSurvivors == "")
            {
                ActiveGameResult = "Nobody won :(";
                return EndGame();
            }
            return new List<string>();
        }

        public List<string> RemovePlayerFromGame(string username)
        {
            lock (removePlayerFromGameLock)
            {
                List<string> ret = new List<string>();
                Player player = GetLoggedPlayer(username);
                if (player != null)
                {
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
                }else
                {
                    ret.Add("Player was not in the game");
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
            if (aliveMonsters == "" && TimeHasPassed(Store.ActiveGame.LimitJoiningTime))
            {
                aliveSurvivors = aliveSurvivors.Trim(',');
                ActiveGameResult = aliveSurvivors + " won !";
                return EndGame();
            }
            else if (alivePlayers == 1 && aliveSurvivors == "" && TimeHasPassed(Store.ActiveGame.LimitJoiningTime))
            {
                aliveMonsters = aliveMonsters.Trim(',');    
                ActiveGameResult = aliveMonsters + " won !";
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
                ret.Add(ActiveGameResult.ToUpper());
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

        public string GetGameResult()
        {
            if(ActiveGameResult != "")
            {
                return ActiveGameResult.ToUpper();
            }else
            {
                return "GameNotFinished";
            }
            
        }
    }
}