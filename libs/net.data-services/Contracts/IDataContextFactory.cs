namespace adworks.data_services
{
    public interface IDataContextFactory
    {
        CommonDbContext Create();
    }
}
