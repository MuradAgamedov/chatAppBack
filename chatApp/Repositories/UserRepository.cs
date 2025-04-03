using System.Threading.Tasks;
using chatApp.Data;
using chatApp.Interfaces;
using chatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace chatApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }


        public async Task<ApplicationUser?> UpdateUserByEmailAsync(string email, ApplicationUser updatedUser)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return null;

            user.FullName = updatedUser.FullName;
            user.UserName = updatedUser.UserName;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.AvatarUrl = updatedUser.AvatarUrl;
            user.Gender = updatedUser.Gender;

            await _context.SaveChangesAsync();
            return user;
        }



        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }


    }
}
