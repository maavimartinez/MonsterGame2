using Business;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Protocol
{
    public class Response
    {
        private readonly string[][][] responsePackage;

        public string Message => responsePackage[1][0][0];
        
        private string MyResponseCode => responsePackage[0][0][0];

        private int Code => Int32.Parse(MyResponseCode);
        

        public Response(string[][][] response)
        {
            responsePackage = response;
        }

        public string GetClientToken()
        {
            return responsePackage[1][0][0];
        }

        public string GetUsername()
        {
            return responsePackage[1][0][0];
        }

        public string GetIfDead()
        {
            return responsePackage[1][0][0];
        }

        public string GetDeadPlayer()
        {
            return responsePackage[2][0][0];
        }

        public string GetPlayerPosition()
        {
            return responsePackage[1][0][0];
        }

        public string GetRemainingTime()
        {
            return responsePackage[1][0][0];
        }

        public string ErrorMessage()
        {
            return responsePackage[1][0][0];
        }

        public string ServerMessage()
        { 
            return responsePackage[1][0][0];
        }


        public List<string> Messages()
        {
            var messages = new List<string>();

            for (int i = 1; i < responsePackage.Length; i++)
                messages.Add(responsePackage[i][0][0]);

            return messages;
        }
  

        public List<string> UserList()
        {
            var users = new List<string>();
            for (var i = 1; i < responsePackage.Length; i++)
                users.Add(responsePackage[i][0][0]);
            return users;
        }


        public List<string> GetDoActionResponse()
        {
            var ret = new List<string>();
            for (int i = 1; i < responsePackage.Length; i++)
            {
                ret.Add(responsePackage[i][0][0]);
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

        public bool PlayerHasWon()
        {
            return HasCode(ResponseCode.GameWon);
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