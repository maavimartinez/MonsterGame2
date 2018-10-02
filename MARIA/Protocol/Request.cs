namespace Protocol
{
    public class Request
    {
        private readonly string[] requestObject;

        public Request(string[] request)
        {
            requestObject = request;
        }

        public Command Command => (Command)int.Parse(requestObject[0]);

        public string UserToken()
        {
            return requestObject[1];
        }

        public string Action()
        {
            return requestObject[2];
        }

        public string Role()
        {
            return requestObject[2];
        }
        public string Recipient()
        {
          return requestObject[2];  
        } 

        public string Username()
        {
            return requestObject[2];
        } 

        public string Password()
        {
            return requestObject[3];
        }

    }
}