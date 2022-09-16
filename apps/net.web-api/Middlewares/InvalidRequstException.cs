using System;

namespace adworks.media_web_api.Middlewares
{
    public class InvalidRequstException : Exception
    {
        public InvalidRequstException(string msg): base(msg)
        {
        }
    }
}