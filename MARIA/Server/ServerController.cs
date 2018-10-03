using System.Collections.Generic;
using System.Linq;
using Business;
using Business.Exceptions;
using Entities;
using System.Net.Sockets;
using System;
using System.Text;
using System.IO;
using System.Drawing;
using Protocol;

namespace Server
{

    public class ServerController
    {

        private readonly GameLogic gameLogic;

        private string picture = string.Empty;

        private string picturesUsername;

        private string extension;

        private int parts = 0;

        public ServerController(GameLogic gameLogic)
        {
            this.gameLogic = gameLogic;
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
                string token = gameLogic.Login(client);

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
                List<Player> connectedUsers = gameLogic.GetLoggedPlayers();

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
                List<Client> clients = gameLogic.GetClients();

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
                List<Client> clients = gameLogic.GetLoggedClients();

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

                gameLogic.SelectRole(loggedClient, role);

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

                gameLogic.JoinGame(loggedUser.Username);

                List<string> response = new List<string>();
                response.Add(GetPlayerPosition(loggedUser.Username));
                List<string> onGameUsernames = gameLogic.GetOnGameUsernamesAndStatus();
                response = response.Concat(onGameUsernames).ToList();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, response.ToArray()));
            }
            catch (ClientNotConnectedException e)
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
                answer = answer.Concat(gameLogic.DoAction(usernameFrom, action)).ToList();
                answer.Insert(0, GetPlayerPosition(loggedUser.Username));
                answer = answer.Concat(gameLogic.GetOnGameUsernamesAndStatus()).ToList();

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

        public void DisconnectClient(Connection connection, Request request)
        {
            gameLogic.DisconnectClient(request.UserToken());
            connection.SendMessage(BuildResponse(ResponseCode.Ok, "Client disconnected"));
        }

        public void TimesOut(Connection connection, Request request)
        {
            try
            {
                gameLogic.TimesOut();
                connection.SendMessage(BuildResponse(ResponseCode.Ok));
            }
            catch (TimesOutException e)
            {
                connection.SendMessage(BuildResponse(ResponseCode.GameFinished, e.Message));

                connection.Close();
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

        public void GetResultByTimesOut(Connection connection, Request request)
        {
            try
            {
                List<string> timesOut = gameLogic.GetGameResultByTimeOut();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, timesOut.ToArray()));
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

                List<string> response = gameLogic.RemovePlayerFromGame(usernameFrom);

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


        public void CheckIfGameHasFinished(Connection connection, Request request)
        {
            try
            {
                string response = gameLogic.GetGameResult();

                connection.SendMessage(BuildResponse(ResponseCode.Ok, response));
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


        public void SendPicturePart(Connection connection, Request request)
        {
            picture += request.Bytes();
            connection.SendMessage(BuildResponse(ResponseCode.Ok));

        }

        public void SendLastPicturePart(Connection connection, Request request)
        {
            picture += request.Bytes();

            try
            {
                //La foto se guarda con tu nombre en Server/bin/Debug que es donde esta corriendo el server (exe)
                string pathTest = Path.Combine(Environment.CurrentDirectory, picturesUsername+extension);

                var img = Image.FromStream(new MemoryStream(Convert.FromBase64String(picture)));
                img.Save(pathTest);
                connection.SendMessage(BuildResponse(ResponseCode.Ok));
            }
            catch (Exception ex)
            {
                connection.SendMessage(BuildResponse(ResponseCode.BadRequest, ex.Message));
            }

        }

        public void ReadyToSendPicture(Connection connection, Request request)
        {
            picture = string.Empty;
            picturesUsername = request.Username();
            extension = request.PictureExtension();

            connection.SendMessage(BuildResponse(ResponseCode.Ok));
        }

        private Client CurrentClient(Request request)
        {
            return gameLogic.GetLoggedClient(request.UserToken());
        }

        private string GetPlayerPosition(string username)
        {
            string pos;

            Player loggedPlayer = gameLogic.GetLoggedPlayer(username);
            pos = loggedPlayer.Position.X + "!" + loggedPlayer.Position.Y;
            return pos;
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


