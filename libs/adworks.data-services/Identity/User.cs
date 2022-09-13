using System;
using System.ComponentModel.DataAnnotations.Schema;
using adworks.data_services.DbModels;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace adworks.data_services.Identity
{
    public class User : IdentityUser<string>
    {
        public User()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public string ProfileLogo { get; set; }
        [JsonIgnore]
        public Organization Organization { get; set; }
    }
}
