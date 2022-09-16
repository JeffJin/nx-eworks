using System;
using Microsoft.AspNetCore.Identity;

namespace adworks.data_services.Identity
{
    public class Role : IdentityRole<string>
    {
        public Role()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}