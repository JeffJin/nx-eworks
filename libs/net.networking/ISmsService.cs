namespace adworks.networking
{
    public interface ISmsService
    {
        string Send(string phone, string text);
    }
}