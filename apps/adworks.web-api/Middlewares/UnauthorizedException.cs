using System;

namespace adworks.media_web_api.Middlewares
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string msg): base(msg)
        {
        }
        public UnauthorizedException(string msg, Exception e): base(msg, e)
        {
        }
    }
}