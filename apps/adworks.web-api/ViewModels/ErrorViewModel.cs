namespace adworks.media_web_api.ViewModels
{
    public class ErrorViewModel
    {
        public string HttpCode { get; }

        public ErrorViewModel(string httpCode)
        {
            HttpCode = httpCode;
        }
    }
}