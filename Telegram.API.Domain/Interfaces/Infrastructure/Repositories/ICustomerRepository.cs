using Telegram.API.Domain.Entities.User;

namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetCustomerByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
