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
        const int RADIUS = 1;
        const int WIDTH = 8;
        const int HEIGHT = 8;

        private Store Store { get; set; }
        private Server Server { get; set; }
        private readonly object gameLocker = new object();
        private readonly object selectRoleLocker = new object();
        private readonly object loginLocker = new object();
        private readonly object actionLocker = new object();
        private readonly object joinGameLocker = new object();
        private readonly object doActionLocker = new object();

        public GameController(Store store)
        {
            Store = store;
            Server = new Server();
        }

        public string Login(Client client)
        {
            lock (loginLocker)
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
            lock (loginLocker)
            {
                Client loggedUser = Server.GetLoggedClient(userToken);
                if (loggedUser == null)
                    throw new ClientNotConnectedException();
                return loggedUser;
            }
        }

        public List<Client> GetLoggedClients()
        {
            lock (loginLocker)
            {
                return Server.GetLoggedClients();
            }
        }

        public List<Player> GetLoggedPlayers()
        {
            lock (loginLocker)
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

        public void DisconnectClient(string token)
        {
            lock (loginLocker)
            {
                Server.DisconnectClient(token);
            }
        }

        public List<Client> GetClients()
        {
            lock (loginLocker)
            {
                return Store.GetClients();
            }
        }

        public Player GetLoggedPlayer(string username)
        {
            lock (joinGameLocker)
            {
                return Store.GetLoggedPlayer(username);
            }
        }

        public void SelectRole(Client loggedClient, string role)
        {

            lock(selectRoleLocker)
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
            if (Store.ActiveGame.Players.Count() == 3)
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
            lock (joinGameLocker)
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
                /////////////////////////////////////*
              /*  Monster m = new Monster();
                Client c = new Client("hola", "hol");
                Guid ip = Guid.NewGuid();
                c.Username = "soyPrueba";
                m.Client = c;
                int i = 5;
                int j = 5;
                m.Position = new Cell();
                m.Position.X = i;
                m.Position.Y = j;
                m.Position.Player = m;
                Store.ActiveGame.Players.Add(m);
                Store.AllPlayers.Add(m);
                Store.Board.Cells[i, j].Player = m;*/
                /////////////////////////////////////////
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
            lock (actionLocker)
            {
                if (!Store.ActiveGame.isOn)
                {
                    throw new GameHasFinishedException(Store.ActiveGame.Result);
                }
                Player player = GetLoggedPlayer(usernameFrom);
                CheckRightTurn(player);
                List<string> ret = new List<string>();
                ret = TranslateAndDoAction(player, action);
                int x = GetLoggedPlayer(usernameFrom).Position.X;
                int y = GetLoggedPlayer(usernameFrom).Position.Y;
                ret = ret.Concat(GetNearPlayers(x, y)).ToList();
                CheckIfGameHasEnded();
                return ret;
            }
        }

        private void CheckRightTurn(Player player)
        {
            int minTurn = GetMinTurn();
            if (player.NumOfActions - minTurn == 2) throw new WaitForTurnException();
        }

        private int GetMinTurn()
        {
            int min = Int32.MaxValue;
            foreach (Player pl in Store.ActiveGame.Players)
            {
                if (pl.NumOfActions < min) min = pl.NumOfActions;
            }
            return min;
        }

        private List<string> TranslateAndDoAction(Player player, string cmd)
        {
            List<string> ret = new List<string>();
            if (!player.isAlive) throw new LoggedPlayerIsDeadException();
            string aux = cmd.Replace(" ", String.Empty).ToUpper();
            if (aux.Length < 5) throw new ActionException("Invalid action format"); //ESTO NO ANDA BIN SI EL USERNAME ES DE UNA LETRA
            string action = aux.Substring(0, 3);
            string sndParameter = aux.Substring(3);
            if (action.Equals("MOV"))
            {
                CheckCorrectMoveFormat(sndParameter);
                Move(player, sndParameter);
                player.NumOfActions++;
                ret = new List<string>();
            }
            else if (action.Equals("ATT"))
            {
                Player defender = GetDefender(sndParameter);
                ret = Attack(player, defender);
                player.NumOfActions++;
            }
            else
            {
                throw new ActionException("Invalid Action");
            }
            return ret;
        }

        public void CheckCorrectMoveFormat(string move)
        {
            char row = move[0];
            int column = (int)Char.GetNumericValue(move[1])-1;
            bool validRow = Regex.IsMatch(row.ToString(), "[a-h]", RegexOptions.IgnoreCase);
            bool validColumn = (column>=1 && column<=8);
            if (move.Length > 2 || !validRow || !validColumn) throw new Exception("Invalid format");
        }

        public void CheckDefenderForAttackExists(string username)
        {
            int count = 0;
            foreach (Player pl in Store.ActiveGame.Players)
                if (pl.Client.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) count++;
            if (count == 0) throw new Exception("Non existent player");
        }

        public Player GetDefender(string username)
        {
            foreach (Player pl in Store.ActiveGame.Players)
            {
                if (pl.Client.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    return pl;
                }
            }
            throw new ClientNotConnectedException("Defender does not exist");
        }

        public void Move(Player player, string pos)
        {
            int y = pos[0] - 65;
            int x = (int)Char.GetNumericValue(pos[1]) - 1;
            CheckValidMovement(player, x, y);
            CheckFreePosition(x, y);
;           RemovePlayerFromCell(player);
            LocatePlayerOnCell(player, x, y);
        }

        private void CheckValidMovement(Player player, int x, int y)
        {
            var lowerY = Math.Max(0, player.Position.Y - RADIUS);
            var upperY = Math.Min(WIDTH - 1, player.Position.Y + RADIUS);
            var lowerX = Math.Max(0, player.Position.X - RADIUS);
            var upperX = Math.Min(HEIGHT - 1, player.Position.X + RADIUS);
            if (x < lowerX || x > upperX || y < lowerY || y > upperY)
                throw new MovementOutOfBoundsException();
            if (player.Position.Y == y && player.Position.X == x)
                throw new SamePlaceMovementException();
        }

        private void CheckFreePosition(int x, int y)
        {
            if (Store.Board.Cells[x, y].Player != null) throw new CellAlreadyContainsAPlayerException();
        }

        private void LocatePlayerOnCell(Player player, int x, int y)
        {
            player.Position = Store.Board.Cells[x, y];
            Store.Board.Cells[x, y].Player = player;
        }

        private void RemovePlayerFromCell(Player player)
        {
            int x = player.Position.X;
            int y = player.Position.Y;
            Store.Board.Cells[x,y].Player = null;
        }

        public List<string> GetNearPlayers(int x, int y)
        {
            List<string> nearPlayers = new List<string>();
            nearPlayers.Add("near");

            var lowerY = Math.Max(0, y - RADIUS);
            var upperY = Math.Min(WIDTH - 1, y + RADIUS);
            var lowerX = Math.Max(0, x - RADIUS);
            var upperX = Math.Min(HEIGHT - 1, x + RADIUS);
            for (int i = lowerX; i <= upperX; ++i)
            {
                for (int j = lowerY; j <= upperY; ++j)
                {
                    if (Store.Board.Cells[i, j].Player != null && (i != x || j != y))
                    {
                        string nearUsername = Store.Board.Cells[i, j].Player.Client.Username;
                        string role = Store.Board.Cells[i, j].Player.ToString();

                        nearPlayers.Add(nearUsername + "(" + role + ")" );
                    }
                }
            }
            if(nearPlayers.Count == 1)
            {
                return new List<string>();
            }else
            {
                return nearPlayers;
            }

        }

        public List<string> Attack(Player attacker, Player defender)
        {
            if (attacker.GetType() == typeof(Survivor) && defender.GetType() == typeof(Survivor))
                throw new ActionException("Survivor can't attack survivors");
            defender.HP = defender.HP - attacker.AP;
            if (defender.HP == 0) defender.isAlive = false;
            List<string> ret = new List<string>();
            if (!defender.isAlive)
            {
                ret.Add("killed");
                ret.Add(defender.Client.Username);
            }
            return ret;
        }

        private void CheckIfGameHasEnded()
        {
            string aliveMonsters = "";
            string aliveSurvivors = "";
            int alivePlayers = 0;
            foreach (Player pl in Store.AllPlayers)
            {
                if (pl.isAlive)
                {
                    alivePlayers++;
                    if (pl is Monster)  aliveMonsters = aliveMonsters + pl.Client.Username + ",";
                    if (pl is Survivor) aliveSurvivors = aliveSurvivors + pl.Client.Username +",";
                }
            }
            if (aliveMonsters == "")
            {
                aliveSurvivors.Trim(',');
                Store.ActiveGame.Result = aliveSurvivors + "won !";
                EndGame();
                throw new GameHasFinishedException(Store.ActiveGame.Result);
            }
            else if (alivePlayers == 1 && aliveSurvivors == "" && TimeHasPassed(Store.ActiveGame.LimitJoiningTime)) 
            {
                aliveMonsters.Trim(',');
                Store.ActiveGame.Result = aliveMonsters + "won !";
                EndGame();
                throw new GameHasFinishedException(Store.ActiveGame.Result);
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

        public string TimesOut()
        {
            string ret = "timesNotOut";
            if (Store.ActiveGame.isOn && TimeHasPassed(3)){
                GetGameResultByTimeOut();
            }
            return ret;
        }

        public void GetGameResultByTimeOut()
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
                Store.ActiveGame.Result = aliveSurvivors + "won !";
                EndGame();
                throw new GameHasFinishedException(Store.ActiveGame.Result);
            }else if(aliveSurvivors == "")
            {
                Store.ActiveGame.Result = "Nobody won :(";
                EndGame();
                throw new GameHasFinishedException(Store.ActiveGame.Result);
            }
        }

        public void RemovePlayerFromGame(string username)
        {
            Player player = GetLoggedPlayer(username);
            Store.ActiveGame.Players.Remove(player);
            Store.AllPlayers.Remove(player);
            if (Store.AllPlayers.Count > 0)
            {
                CheckIfGameHasEnded();
            }else if(Store.AllPlayers.Count == 0)
            {
                EndGame();
                Store.ActiveGame.Result = "Game has finished";
                throw new GameHasFinishedException(Store.ActiveGame.Result); //ver si se puede meter adentor de endgame
            }
        }

        public void EndGame() {
            Game game = Store.ActiveGame;
            if (game != null)
            {
                game.StartTime = DateTime.MinValue;
                game.isOn = false;
                game.Result = "";
                Store.ActiveGame.Players.Clear();
            }
        }

        /* FOTOS tambuffer 2048*5
         int size = GetFileSize();
         int parts = GetPartsToSend(size);
         var totalRead = 0;
         using(Filestream fs = new ...)
         {
            var read = 0;
            while(read<partsize)
            {
                read += fs.Read(buffer,read,partsize-read);
            } 
            SendSocket(buffer);
            totalRead += read;
            fs.Seek(totalRead);
        }
         */
    }
}