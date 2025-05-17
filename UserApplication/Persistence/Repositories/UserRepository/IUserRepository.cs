using System.Linq.Expressions;
using UserApplication.API.Models;

namespace UserApplication.Persistence.Repositories.UserRepository
{
    public interface IUserRepository
    {
        public Task<List<User>> GetAllAsync();
        public Task<List<User>> GetAllByAsync(Expression<Func<User, bool>> predicate);
        public Task<User?> GetByIdAsync(Guid id);
        public Task<User?> GetByLoginAsync(string login);
        public Task AddAsync(User user);
        public Task UpdateAsync(User user);
        public Task DeleteAsync(User id);
        public Task DeleteByIdAsync(Guid id);
    }
}
