using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class CommonDataService: ICommonDataService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public CommonDataService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;

            _logger = logger;
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                return await dbContext.Categories.ToListAsync();
            }
        }

        public async Task<Category> FindCategory(string name)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                Category category = await dbContext.Categories.SingleOrDefaultAsync(c => c.Name == name);
                return category;
            }
        }

    }
}
