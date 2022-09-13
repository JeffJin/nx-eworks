namespace adworks.data_services
{
    public static class DbManagement
    {
        public static void InitDatabase()
        {
            var dbFactory = new DataContextFactory();
            using (var context = dbFactory.Create())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.SaveChanges();
            }
            
        }
    }
}
