using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Business;
using Business.Exceptions;

namespace Persistence
{
    public class Store : IStore
    {
        private List<Client> Clients { get; set; }

        public Store()
        {
            Clients = new List<Client>();
        }

        public bool ClientExists(Client client)
        {
            return Clients.Contains(client);
        }

        public void AddClient(Client client)
        {
            Clients.Add(client);
        }

        public Client GetClient(string clientUsername)
        {
            return Clients.Find(client => client.Username.Equals(clientUsername));
        }

        public List<Client> GetOponentsOf(Client client)
        {
            return Clients.Find(c => c.Equals(client)).Oponents;
        }


        public void SendMessage(string usernameFrom, string usernameTo, string messageContent)
        {
            Client clientFrom = Clients.Find(c => c.Username.Equals(usernameFrom));
            Client clientTo = Clients.Find(c => c.Username.Equals(usernameTo));

            var message = new Message()
            {
                Sender = usernameFrom,
                Receiver = usernameTo,
                Content = messageContent,
                TimeStamp = DateTime.Now,
                Read = false
            };

            clientFrom.Messages.Add(message);
            clientTo.Messages.Add(message);
        }
        public List<Client> GetClients()
        {
            return Clients;
        }
    }
}
