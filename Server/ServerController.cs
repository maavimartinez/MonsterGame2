using System.Collections.Generic;
using System.Linq;
using Business;
using Business.Exceptions;
using Protocol;
using System.Threading;

namespace Server
{
    public class ServerController
    {
        private readonly GameLogic GameLogic;

        public ServerController(GameLogic gameLogic)
        {
            GameLogic = gameLogic;
        }

        public void ConnectClient(Connection conn, Request req)
        {
            try
            {
                var client = new Client(req.Username(), req.Password());
                string token = GameLogic.Login(client);
                object[] response = string.IsNullOrEmpty(token)                 //cambiar esto
                    ? BuildResponse(ResponseCode.NotFound, "User not found")
                    : BuildResponse(ResponseCode.Ok, token);
                conn.SendMessage(response);
            }
            catch (ClientAlreadyConnectedException e)
            {
                conn.SendMessage(BuildResponse(ResponseCode.Forbidden, e.Message));
            }
        }

        public void ReadCommand(Connection conn, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                //
                GameLogic.TranslateCommandToAction(loggedUser, cmd);
            }
            catch (RecordNotFoundException e)
            {
               
            }
            catch (ClientNotConnectedException e)
            {
               
            }
        }

        public void ListMyOpponents(Connection conn, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                List<Client> friends = businessController.GetFriendsOf(loggedUser);
                var clientFriends = new List<string[]>();

                friends.ForEach(c => clientFriends.Add(new[] {c.Username, c.FriendsCount.ToString()}));

                conn.SendMessage(BuildResponse(ResponseCode.Ok, clientFriends.ToArray()));
            }
            catch (RecordNotFoundException e)
            {
                conn.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                conn.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

        public void DisconnectUser(Connection conn, Request request)
        {
            GameLogic.DisconnectClient(request.UserToken());
            conn.SendMessage(BuildResponse(ResponseCode.Ok, "Client disconnected"));
        }

        public void InvalidCommand(Connection conn)
        {
            object[] response = BuildResponse(ResponseCode.BadRequest, "Unrecognizable command");
            conn.SendMessage(response);
        }

        private object[] BuildResponse(ResponseCode responseCode, params object[] payload)
        {
            var responseList = new List<object>(payload);
            responseList.Insert(0, responseCode.GetHashCode());

            return responseList.ToArray();
        }

        private Client CurrentClient(Request req)
        {
            return GameLogic.GetLoggedClient(req.UserToken());
        }

        public void SendMessage(Connection conn, Request request)
        {
            try
            {
                Client loggedUser = CurrentClient(request);
                string usernameFrom = loggedUser.Username;
                string usernameTo = request.Recipient();

                string message = request.Message;

                GameLogic.SendMessage(usernameFrom, usernameTo, message);
                conn.SendMessage(BuildResponse(ResponseCode.Ok));
            }
            catch (RecordNotFoundException e)
            {
                conn.SendMessage(BuildResponse(ResponseCode.NotFound, e.Message));
            }
            catch (ClientNotConnectedException e)
            {
                conn.SendMessage(BuildResponse(ResponseCode.Unauthorized, e.Message));
            }
        }

    }
}