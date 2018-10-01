using System;
using System.Collections.Generic;

namespace Protocol
{

    public static class ResponseCode
    {

        public static int Ok = 200;

        public static int Created = 201;

        public static int GameFinished = 408;

        public static int BadRequest = 400;

        public static int Unauthorized = 401;

        public static int Forbidden = 403;

        public static int NotFound = 404;

        public static int InvalidAction = 405;
        
        public static int InternalServerError = 500;

    }

}