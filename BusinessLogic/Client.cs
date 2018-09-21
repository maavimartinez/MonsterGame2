using System;
using System.Collections.Generic;
using System.Threading;

namespace Business
{
    public class Client
    {
        public Client(string username, string password)
        {
            Username = username;
            Password = password;
            Oponents = new List<Client>();
            Messages = new List<Message>();
            GameSessions = new List<GameSession>();
        }

        public string Username { get; set; }
        public string Password { get; set; }
         public List<Client> Oponents { get; set; }
        public List<Message> Messages { get; set; }
        private List<GameSession> GameSessions { get; }

        public override bool Equals(object obj)
        {
            var toCompare = (Client) obj;
            return toCompare != null && Username.Equals(toCompare.Username);
        }

        public bool ValidatePassword(string clientPassword)
        {
            return Password.Equals(clientPassword);
        }

        public void AddGameSession(GameSession session)
        {
            GameSessions.Add(session);
        }

      private void AddOponent(Client client)
        {
           // if (HasOponent(client) || client.HasOponent(this)) throw new ClientAlreadyBeoponentsException();
            Oponents.Add(client);
            client.Oponents.Add(this);
        }

    public bool HasOponent(Client otherClient)
        {
            return Oponents.Contains(otherClient);
        }
    }
}