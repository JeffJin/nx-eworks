using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace adworks.media_web_api.Models.ManageViewModels
{
    public class DisplayRecoveryCodesViewModel
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }

    }
}
