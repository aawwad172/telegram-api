namespace Telegram.API.Domain.Interfaces.Infrastructure.Repositories;

public interface IRepository<T>
{
    Task<T> GetById(int id);


}