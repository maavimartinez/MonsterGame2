using System;
using System.Linq;
using System.Collections.Generic;

namespace Protocol
{
    public class Response
    {
        private readonly string[] responsePackage;

        public string Message => responsePackage[1];
        
        private string MyResponseCode => responsePackage[0];

        private int Code {
            get{return Int32.Parse(MyResponseCode);}
            set{}
        }
        
        public Response(string[] response)
        {
            responsePackage = response;
        }

        public string GetClientToken()
        {
            return responsePackage[1];
        }

        public string GetUsername()
        {
            return responsePackage[2];
        }

        public string GetIfDead()
        {
            return responsePackage[1];
        }

        public string GetDeadPlayer()
        {
            return responsePackage[1];
        }

        public string GetPlayerPosition()
        {
            return responsePackage[1];
        }

        public string GetRemainingTime()
        {
            return responsePackage[1];
        }

        public string ErrorMessage()
        {
            return responsePackage[1];
        }

        public string ServerMessage()
        { 
            return responsePackage[1];
        }

        public List<string> Messages()
        {
            var messages = new List<string>();

            for (int i = 2; i < responsePackage.Length; i++)
                messages.Add(responsePackage[i]);

            return messages;
        }
  
        public List<string> UserList()
        {
            var users = new List<string>();
            for (var i = 1; i < responsePackage.Length; i++)
                users.Add(responsePackage[i]);
            return users;
        }

        public List<string> GetOnGameUsernamesAndStatus()
        {
            var ret = new List<string>();
            for (int i = 3; i < responsePackage.Length; i++)
            {
                ret.Add(responsePackage[i]);
            }
            return ret;
        }

        public List<string> GetDoActionResponse()
        {
            var ret = new List<string>();
            for (int i = 1; i < responsePackage.Length; i++)
            {
                ret.Add(responsePackage[i]);
            }
            return ret;
        }

        public List<string> GetTimeOutResponse()
        {
            var ret = new List<string>();
            for (int i = 1; i < responsePackage.Length; i++)
            {
                ret.Add(responsePackage[i]);
            }
            return ret;
        }

        public List<string> GetRemovePlayerFromGameResponse()
        {
            var ret = new List<string>();
            for (int i = 1; i < responsePackage.Length; i++)
            {
                ret.Add(responsePackage[i]);
            }
            return ret;
        }

        public bool HadSuccess()
        {
            return HasCode(ResponseCode.Ok) || HasCode(ResponseCode.Created);
        } 
        
        public bool IsInvalidAction()
        {
            return HasCode(ResponseCode.InvalidAction);
        }

        private bool HasCode(int responseCode)
        {
            if(MyResponseCode != null)
            {
                return Code == responseCode;
            }else
            {
                return false;
            }
        }

    }
}