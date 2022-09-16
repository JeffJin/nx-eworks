namespace adworks.media_common
{
    public class JwtToken
    {

        public JwtToken(string token)
        {
            Token = token;
        }
        
        public string Token { get; }
    }
}