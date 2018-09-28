using System;

namespace Business.Exceptions
{
    public class ClientAlreadyConnectedException : BusinessException
    {
        public ClientAlreadyConnectedException() : base("Client already connected")
        {
        }
    }
}