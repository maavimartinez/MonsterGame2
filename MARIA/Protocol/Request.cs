using Business;

namespace Protocol
{
    public class Request
    {
        private readonly string[][][] requestObject;

        public Request(string[][][] request)
        {
            requestObject = request;
        }

        public Command Command => (Command) int.Parse(requestObject[0][0][0]);

        public string Action()
        {
            return requestObject[1][0][0];
        }

        public string Role()
        {
            return requestObject[1][0][0];
        }
        public string Recipient()
        {
          return requestObject[1][0][0];  
        } 

        public string Username()
        {
            return requestObject[1][0][0];
        } 

        public string Password()
        {
            return requestObject[2][0][0];
        }

        public string UserToken()
        {
            return requestObject[0][1][0];
        } 


    }
}