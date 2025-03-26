using System.Threading.Tasks;
using chatApp.Models;

namespace chatApp.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserByEmailAsync(string email, ApplicationUser user);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    }
}
