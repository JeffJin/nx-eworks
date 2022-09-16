using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace adworks.media_common
{
    public class ExternalLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}