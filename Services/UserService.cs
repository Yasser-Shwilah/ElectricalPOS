using ElectricalPOS.Models;
using ElectricalPOS.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ElectricalPOS.Services
{
    public class UserService
    {
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            using var context = new ElectricalPOSContext();
            
            // First, try to find the user by username only
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                // User not found
                return null;
            }
            
            // Hash the provided password
            var hashedPassword = PasswordHasher.HashPassword(password);
            
            // Compare the hashes
            if (user.Password == hashedPassword)
            {
                return user;
            }
            
            // Password doesn't match
            return null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Users.FindAsync(id);
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                user.Password = PasswordHasher.HashPassword(user.Password);
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Users.Update(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var user = await context.Users.FindAsync(id);
                if (user != null)
                {
                    context.Users.Remove(user);
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
