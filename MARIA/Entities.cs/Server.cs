using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Entities
{
    public class Server
    {
        public List<Session> ConnectedClients { get; set; }

        public Server()
        {
            ConnectedClients = new List<Session>();
        }

        public string ConnectClient(Client client)
        {
            var session = new Session(client);
            ConnectedClients.Add(session);
            client.AddSession(session);

            return session.Id;
        }

        public Client GetLoggedClient(string token)
        {
            return ConnectedClients.Find(session => session.Id.Equals(token))?.Client;
        }

        public bool IsClientConnected(Client client)
        {
            return ConnectedClients.Exists(session => session.Client.Equals(client));
        }

        public void DisconnectClient(string token)
        {
            ConnectedClients.FindAll(session => session.Id.Equals(token)).ForEach(sesssion => sesssion.Deactivate());
            ConnectedClients.RemoveAll(session => session.Id.Equals(token));
        }

        public List<Client> GetLoggedClients()
        {
            return ConnectedClients.Select(session => session.Client).ToList();
        }

    }
}