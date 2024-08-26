using System.Collections.Generic;
using System.Threading.Tasks;
using RandomTGBot.Models;

namespace RandomTGBot.Services
{
    public interface IUserGameService
    {
        Task<List<UserGame>> GetAllUserGames();
        Task<UserGame> GetUserGameById(long id);
        Task<UserGame> CreateUserGame(UserGame userGame);
        Task UpdateUserGame(long id, UserGame userGame);
        Task DeleteUserGame(long id);
    }
}
