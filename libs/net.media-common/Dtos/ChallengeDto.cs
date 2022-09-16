using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace adworks.media_common
{
    public class ChallengeDto
    {
        private readonly string _authenticationScheme;
        private readonly AuthenticationProperties _properties;

        public ChallengeDto(string authenticationScheme, IList<string> properties)
        {
        }
        
        public ChallengeDto(string authenticationScheme, AuthenticationProperties properties)
        {
            _authenticationScheme = authenticationScheme;
            _properties = properties;
        }
    }
}