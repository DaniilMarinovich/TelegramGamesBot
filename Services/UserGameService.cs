using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RandomTGBot.Models;

namespace RandomTGBot.Services
{
    public class UserGameService : IUserGameService
    {
        private readonly HttpClient _client;

        public UserGameService(HttpClient client)
        {
            _client = new HttpClient { BaseAddress = new Uri("https://localhost:5101/api/UserGame/") };
        }

        public async Task<List<UserGame>> GetAllUserGames()
        {
            var response = await _client.GetAsync("GetAllUserGames");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<UserGame>>();
        }

        public async Task<UserGame> GetUserGameById(long id)
        {
            var response = await _client.GetAsync($"GetUserGameById/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserGame>();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return null;
            }
        }

        public async Task<UserGame> CreateUserGame(UserGame userGame)
        {
            var response = await _client.PostAsJsonAsync("CreateUserGame", userGame);
                response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserGame>();
        }

        public async Task UpdateUserGame(long id, UserGame userGame)
        {
            var response = await _client.PutAsJsonAsync($"UpdateUserGame/{id}", userGame);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteUserGame(long id)
        {
            var response = await _client.DeleteAsync($"DeleteUserGame/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
