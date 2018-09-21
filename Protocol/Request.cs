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

        public string Message => requestObject[2][0][0];

        public string Recipient() => requestObject[1][0][0];

        public string Username() => requestObject[1][0][0];

        public string Password() => requestObject[2][0][0];

        public string UserToken() => requestObject[0][1][0];

    }
}