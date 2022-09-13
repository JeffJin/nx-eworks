using System.Threading.Tasks;
using adworks.data_services.Identity;

namespace adworks.data_services
{
    public interface IUserService
    {
        Task<User> GetUserByEmail(string email);
        Task<bool> ValidateEmail(string email);
        Task<bool> ValidateUserName(string userName);
    }
}