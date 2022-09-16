using System;

namespace adworks.data_services
{
    public class UserNotFoundException : Exception
    {
        public string Email { get; }

        public UserNotFoundException(string email)
        {
            Email = email;
        }
    }
}