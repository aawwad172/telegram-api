using Telegram.API.Domain.Entities;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}
