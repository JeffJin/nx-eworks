using System.Linq;
using System.Threading.Tasks;
using adworks.data_services.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class UserService : IUserService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public UserService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            using (var context = _dbContextFactory.Create())
            {
                var user = await context.Users.Where(u => u.Email == email).Include(u => u.Organization).SingleOrDefaultAsync();
                return user;
            }
        }
        public async Task<bool> ValidateEmail(string email)
        {
            using (var context = _dbContextFactory.Create())
            {
                var user = await context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
                return user != null;
            }
        }
        public async Task<bool> ValidateUserName(string userName)
        {
            using (var context = _dbContextFactory.Create())
            {
                var user = await context.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync();
                return user != null;
            }
        }
    }
}
