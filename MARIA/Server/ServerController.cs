using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Business;
using Business.Exceptions;
using Protocol;
using Entities;
using System.Net.Sockets;

namespace Server
{
    public class ServerController
    {
        private readonly GameController gameController;

        public ServerController(GameController gameController)
        {
            this.gameController = gameController;
        }

        public void ConnectClient(Connection connection, Request request)
        {
            try
            {
                var client = new Client(request.Username(), request.Password());
                string token = gameController.Login(client);

                object[] response = string.IsNullOrEmpty(token)
                    ? BuildResponse(ResponseCode.NotFound, "Client not found")
                    : BuildResponse(ResponseCode.Ok, token);
                connection.SendMessage(response);
            }
            catch (ClientAlreadyConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Forbidden, e.Message));
            }
        }

        public void ListPlayersInGame(Connection connection, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                List<Player> connectedUsers = gameController.GetLoggedPlayers();

                string[] connectedUsernames =
                    connectedUsers.Where(player => !player.Client.Equals(loggedUser)).Select(c => c.Client.Username)
                        .ToArray();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, connectedUsernames));
            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

        public void ListAllClients(Connection connection, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                List<Client> clients = gameController.GetClients();

                string[] clientsUsernames =
                    clients.Where(client => !client.Equals(loggedUser)).Select(c => c.Username)
                        .ToArray();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, clientsUsernames));
            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

        public void ListConnectedClients(Connection connection, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                List<Client> clients = gameController.GetLoggedClients();

                string[] clientsUsernames =
                    clients.Select(c => c.Username)
                        .ToArray();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, clientsUsernames));
            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

        public void SelectRole(Connection connection, Request request)
        {
            try{
                Client loggedClient = CurrentClient(request);

                string role = request.Role();

                gameController.SelectRole(loggedClient, role);

                connection.SendMessage(BuildResponse(ResponseCode.Ok));
            }catch(ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

        public void JoinGame(Connection connection, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);

                gameController.JoinGame(loggedUser.Username);

                string playerPosition = GetPlayerPosition(loggedUser.Username);

                connection.SendMessage(BuildResponse(ResponseCode.Ok, playerPosition));
            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
            catch (RoleNotChosenException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
            catch (FullGameException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }

        }

        public void DoAction(Connection connection, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                string usernameFrom = loggedUser.Username;

                string action = request.Action();

                List<string> answer = new List<string>();
                
                List<string> aux = gameController.DoAction(usernameFrom, action);

                answer.Add(GetPlayerPosition(loggedUser.Username));

                answer = answer.Concat(aux).ToList();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, answer.ToArray()));
            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
            catch (GameHasBeenWonException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.GameWon, e.Message)); //no se si esta bien que sea una exception
            }
            catch (ActionException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.InvalidAction, e.Message));
            }catch(BusinessException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.BadRequest, e.Message));
            }
        }

        public void DisconnectClient(Connection connection, Request request)
        {
            gameController.DisconnectClient(request.UserToken());
            connection.SendMessage(BuildResponse(ResponseCode.Ok, "Client disconnected"));
        }

        public void InvalidCommand(Connection connection)
        {
            object[] response = BuildResponse(ResponseCode.BadRequest, "Unrecognizable command");
            connection.SendMessage(response);
        }

        private object[] BuildResponse(int responseCode, params object[] payload)
        {
            var responseList = new List<object>(payload);
            string code = responseCode.ToString();
            responseList.Insert(0, responseCode.ToString());

            return responseList.ToArray();
        }

        private Client CurrentClient(Request request)
        {
            return gameController.GetLoggedClient(request.UserToken());
        }

        private string GetPlayerPosition(string username)
        {
            string pos;

            Player loggedPlayer = gameController.GetLoggedPlayer(username);
            pos = loggedPlayer.Position.X + "!" + loggedPlayer.Position.Y;
            return pos;
        }


        public void TimesOut(Connection connection, Request request)
        {
            try
            {
                string timesOut = gameController.TimesOut();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, timesOut));
                if (timesOut.Equals("timesOut"))
                {
                    connection.Close();
                }
            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

        public void RemovePlayerFromGame(Connection connection, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                string usernameFrom = loggedUser.Username;

                gameController.RemovePlayerFromGame(usernameFrom);

                connection.SendMessage(BuildResponse(ResponseCode.Ok));
            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

        public void EndGame(Connection connection, Request request)
        {
            try
            {
                gameController.EndGame();

                connection.SendMessage(BuildResponse(ResponseCode.Ok));

            }
            catch (RecordNotFoundException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

    }
}