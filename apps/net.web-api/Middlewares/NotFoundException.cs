using System;

namespace adworks.media_web_api.Middlewares
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string msg): base(msg)
        {
        }
    }
}