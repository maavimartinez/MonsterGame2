using System.Collections.Generic;
using System;

namespace Business
{
    public class GameController
    {
        private IStore Store { get; set; }
        private Server Server { get; set; }
        private readonly object messagesLocker = new object();
        private readonly object loginLocker = new object();
        private readonly object oponentLocker = new object();

        public GameController(IStore store)
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
                if (isValidPassword && isClientConnected){
                    return null;
                    // throw new ClientAlreadyConnectedException();
                }
               else return isValidPassword ? Server.ConnectClient(storedClient) : "";
            }
        }

        public Client GetLoggedClient(string userToken)
        {
            lock (loginLocker)
            {
                Client loggedUser = Server.GetLoggedClient(userToken);
                if (loggedUser == null)
                {
                    return null;
                      //en verdad va throw new ClientNotConnectedException();
                }
                  
               else return loggedUser;
            }
        }

        public List<Client> GetLoggedClients()
        {
            lock (loginLocker)
            {
                return Server.GetLoggedClients();
            }
        }

        public void DisconnectClient(string token)
        {
            lock (loginLocker)
            {
                Server.DisconnectClient(token);
            }
        }



        public List<Client> GetOponentsOf(Client client)
        {
            lock (oponentLocker)
            {
                return Store.GetOponentsOf(client);
            }
        }

        public void SendMessage(string usernameFrom, string usernameTo, string message)
        {
            lock (messagesLocker)
            {
                Store.SendMessage(usernameFrom, usernameTo, message);
            }
        }

        public List<Client> GetClients()
        {
            lock (loginLocker)
            {
                return Store.GetClients();
            }
        }
    }
}