using System;
using Protocol;

namespace Server
{
    public class Router
    {
        private readonly ServerController ServerController;

        public Router(ServerController serverController)
        {
            this.ServerController = serverController;
        }

        public void Handle(Connection conn)
        {
            try
            {
                string[][][] message = conn.ReadMessage();
                var request = new Request(message);

                switch (request.Command)
                {
                    case Command.Login:
                        serverController.ConnectClient(conn, request);
                        break;
                    case Command.CreateUser:
                        serverController.CreateUser(conn, request);
                        break;
                    case Command.ListMyOpponents:
                        serverController.ListMyOpponents(conn, request);
                        break;
                    case Command.SendCommand:
                        serverController.SendCommand(conn, request);
                        break;
                    case Command.ReadCommand:
                        serverController.ReadCommand(conn, request);
                        break;
                    case Command.DisconnectUser:
                        serverController.DisconnectUser(conn, request);
                        break;
                    default:
                        serverController.InvalidCommand(conn);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown: " + e.Message);
                Console.WriteLine(e.StackTrace);
                conn.SendMessage(new object[] {ResponseCode.InternalServerError.GetHashCode(), "There was a problem in the server"});
            }
            finally
            {
                conn.Close();
            }
        }
    }
}