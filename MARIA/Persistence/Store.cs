using System;
using System.Collections.Generic;
using Entities;

namespace Persistence
{
    public class Store
    {
        private List<Client> Clients { get; set; }

        public List<Player> AllPlayers { get; set; }

        public Game ActiveGame { get; set; }

        public Board Board { get; set; }

        public Store()
        {
            Clients = new List<Client>();
            AllPlayers = new List<Player>();
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

        public List<Client> GetClients()
        {
            return Clients;
        }

        public Player GetLoggedPlayer(string clientUsername)
        {
            return AllPlayers.Find(p => p.Client.Username.Equals(clientUsername));
        }

        public Game StartGame()
        {
            throw new NotImplementedException();
        }
    }
}