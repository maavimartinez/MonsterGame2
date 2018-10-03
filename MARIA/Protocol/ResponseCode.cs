namespace Protocol
{

    public static class ResponseCode
    {

        public readonly static int Ok = 200;

        public readonly static int Created = 201;

        public readonly static int GameFinished = 202;

        public readonly static int BadRequest = 400;

        public readonly static int Unauthorized = 401;

        public readonly static int Forbidden = 403;

        public readonly static int NotFound = 404;

        public readonly static int InvalidAction = 405;
        
        public readonly static int InternalServerError = 500;

    }

}