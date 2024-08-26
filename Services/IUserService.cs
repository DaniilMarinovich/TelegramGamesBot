using System.Collections.Generic;
using System.Threading.Tasks;
using RandomTGBot.Models;

namespace RandomTGBot.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<User> GetUserById(long id);
        Task<long> GetUserIdByUsername(string username);
        Task<string> GetUsernameById(long id); // Новый метод
        Task<User> CreateUser(User user);
        Task UpdateUser(long id, User user);
        Task DeleteUser(long id);
    }
}
