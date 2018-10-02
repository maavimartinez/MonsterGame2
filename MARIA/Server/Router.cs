using System;
using Protocol;

namespace Server
{
    public class Router
    {

        private readonly ServerController serverController;

        public Router(ServerController serverController)
        {
            this.serverController = serverController;
        }

        public void Handle(Connection conn)
        {
            while (conn.IsConnected())
            {
                try
                {
                    string[] message = conn.ReadMessage();
                    var request = new Request(message);

                    switch (request.Command)
                    {
                        case Command.Login:
                            serverController.ConnectClient(conn, request);
                            break;
                        case Command.DoAction:
                            serverController.DoAction(conn, request);
                            break;
                        case Command.ListPlayersInGame:
                            serverController.ListPlayersInGame(conn, request);
                            break;
                        case Command.ListAllClients:
                            serverController.ListAllClients(conn, request);
                            break;
                        case Command.ListConnectedClients:
                            serverController.ListConnectedClients(conn, request);
                            break;
                        case Command.DisconnectClient:
                            serverController.DisconnectClient(conn, request);
                            break;
                        case Command.JoinGame:
                            serverController.JoinGame(conn, request);
                            break;
                        case Command.SelectRole:
                            serverController.SelectRole(conn, request);
                            break;
                        case Command.TimesOut:
                            serverController.TimesOut(conn, request);
                            break;
                        case Command.RemovePlayerFromGame:
                            serverController.RemovePlayerFromGame(conn, request);
                            break;
                        case Command.SendPicture:
                            serverController.SendPicture(conn, request);
                            break;
                        case Command.CheckIfGameHasFinished:
                            serverController.CheckIfGameHasFinished(conn, request);
                            break;
                        case Command.GetResultByTimesOut:
                            serverController.GetResultByTimesOut(conn, request);
                            break;
                        default:
                            serverController.InvalidCommand(conn);
                            break;
                    }
                }
                catch (Exception e)
                {
                   // Console.WriteLine("Exception: " + e.Message);
                    //Console.WriteLine(e.StackTrace);
                    conn.SendMessage(new string[] { ResponseCode.InternalServerError.ToString(), "There was a problem with the server" });
                    break;
                }
            }
        }

    }

}