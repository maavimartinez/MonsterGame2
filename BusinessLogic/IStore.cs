using System.Collections.Generic;

namespace Business
{
    public interface IStore
    {
        bool ClientExists(Client client);
        void AddClient(Client client);
        Client GetClient(string clientUsername);
        List<Client> GetOponentssOf(Client client);
        void SendMessage(string usernameFrom, string usernameTo, string message);
        List<Client> GetClients();
    }
}