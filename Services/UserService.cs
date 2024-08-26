using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RandomTGBot.Models;

namespace RandomTGBot.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _client;

        public UserService(HttpClient client)
        {
            _client = new HttpClient { BaseAddress = new Uri("https://localhost:5101/api/User/") };
            }

        public async Task<List<User>> GetAllUsers()
        {
            var response = await _client.GetAsync("GetAllUsers");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<User>>();
        }

        public async Task<User> GetUserById(long id)
        {
            var response = await _client.GetAsync($"GetUserById/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>();
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

        public async Task<long> GetUserIdByUsername(string username)
        {
            var response = await _client.GetAsync($"GetUserIdByUsername/{username}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<long>();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"User with username '{username}' not found.");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return -1;
            }
        }

        public async Task<string> GetUsernameById(long id)
        {
            var response = await _client.GetAsync($"GetUsernameById/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<string>();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found.");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return null;
            }
        }

        public async Task<User> CreateUser(User user)
        {
            var response = await _client.PostAsJsonAsync("CreateUser", user);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task UpdateUser(long id, User user)
        {
            var response = await _client.PutAsJsonAsync($"UpdateUser/{id}", user);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteUser(long id)
        {
            var response = await _client.DeleteAsync($"DeleteUser/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
