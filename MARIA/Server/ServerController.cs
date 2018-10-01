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

        public void InvalidCommand(Connection connection)
        {
            object[] response = BuildResponse(ResponseCode.BadRequest, "Unrecognizable command");
            connection.SendMessage(response);
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
            try
            {
                Client loggedClient = CurrentClient(request);

                string role = request.Role();

                gameController.SelectRole(loggedClient, role);

                connection.SendMessage(BuildResponse(ResponseCode.Ok));
            }
            catch (ClientNotConnectedException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
            catch (NoMonstersInGameException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.BadRequest, e.Message));
            }
        }

        public void JoinGame(Connection connection, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);

                gameController.JoinGame(loggedUser.Username);

                List<string> response = new List<string>();
                response.Add(GetPlayerPosition(loggedUser.Username));
                List<string> onGameUsernames = gameController.GetOnGameUsernames();
                response = response.Concat(onGameUsernames).ToList();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, response.ToArray()));
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
            catch (ActionException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.InvalidAction, e.Message));
            }
            catch (BusinessException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.BadRequest, e.Message));
            }
        }

        private string GetPlayerPosition(string username)
        {
            string pos;

            Player loggedPlayer = gameController.GetLoggedPlayer(username);
            pos = loggedPlayer.Position.X + "!" + loggedPlayer.Position.Y;
            return pos;
        }

        public void DisconnectClient(Connection connection, Request request)
        {
            gameController.DisconnectClient(request.UserToken());
            connection.SendMessage(BuildResponse(ResponseCode.Ok, "Client disconnected"));
        }

        public void TimesOut(Connection connection, Request request)
        {
            try
            {
                List<string> timesOut = gameController.TimesOut();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, timesOut.ToArray()));

                if (!timesOut[0].Equals("timesNotOut"))
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

                List<string> response = gameController.RemovePlayerFromGame(usernameFrom);

                connection.SendMessage(BuildResponse(ResponseCode.Ok, response.ToArray()));
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

        private Client CurrentClient(Request request)
        {
            return gameController.GetLoggedClient(request.UserToken());
        }

        private object[] BuildResponse(int responseCode, params object[] payload)
        {
            var responseList = new List<object>(payload);
            string code = responseCode.ToString();
            responseList.Insert(0, responseCode.ToString());

            return responseList.ToArray();
        }

    }

}
