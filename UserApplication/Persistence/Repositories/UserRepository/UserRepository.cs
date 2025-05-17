using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserApplication.API.Models;

namespace UserApplication.Persistence.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync() =>
            await _context.Users.ToListAsync();
        public async Task<List<User>> GetAllByAsync(Expression<Func<User, bool>> predicate) =>
            await _context.Set<User>().Where(predicate).ToListAsync();
        public async Task<User?> GetByIdAsync(Guid id) =>
            await _context.Users.FindAsync(id);
        public async Task<User?> GetByLoginAsync(string login) =>
            await _context.Users.FirstOrDefaultAsync(w => w.Login == login);
        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
