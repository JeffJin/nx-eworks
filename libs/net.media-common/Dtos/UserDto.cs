using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace adworks.media_common
{
    public class UserDto
    {
        
        public UserDto()
        {
            
        }
        public UserDto(string email)
        {
            Email = email;
            UserName = email;
        }
        
        public string Id { get; set; }
        
        public string UserName { get; set; }
        
        public string ProfileLogo { get; set; }
        
        public string OrganizationName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public override string ToString()
        {
            return $"{UserName}, {OrganizationName}, {Email}, {Id}";
        }
    }
}