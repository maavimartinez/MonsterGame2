using System;
using System.Collections.Generic;
using System.Threading;

namespace Entities
{
    public class Client
    {
        public Client(string username, string password)
        {
            Username = username;
            Password = password;
            Friends = new List<Client>();
            Sessions = new List<Session>();
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public List<Client> Friends { get; set; }
        public int FriendsCount => Friends.Count;
        public DateTime? ConnectedSince => Sessions.Find(session => session.Active)?.ConnectedSince;
        public int ConnectionsCount => Sessions.Count;
        private List<Session> Sessions { get; }

        public override bool Equals(object obj)
        {
            var toCompare = (Client) obj;
            return toCompare != null && Username.Equals(toCompare.Username);
        }

        public bool ValidatePassword(string clientPassword)
        {
            return Password.Equals(clientPassword);
        }

        public void AddSession(Session session)
        {
            Sessions.Add(session);
        }

    }
}