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

        // Метод для обновления данных пользователя по email
        public async Task<bool> UpdateUserByEmailAsync(string email, ApplicationUser user)
        {
            var existingUser = await GetUserByEmailAsync(email);
            if (existingUser == null)
                return false;

            existingUser.UserName = user.UserName;
            existingUser.FullName = user.FullName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.AvatarUrl = user.AvatarUrl;
            existingUser.Gender = user.Gender;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }


    }
}
