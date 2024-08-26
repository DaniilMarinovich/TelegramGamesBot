using RandomTGBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RandomTGBot.Services
{
    public class DuelStorageService
    {
        private const string StorageFilePath = "duels.json";

        public async Task SaveDuelContextAsync(Dictionary<long, DuelContext> duels)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(duels, options);
            await File.WriteAllTextAsync(StorageFilePath, json);
        }

        public async Task<Dictionary<long, DuelContext>> LoadDuelContextAsync()
        {
            if (!File.Exists(StorageFilePath))
            {
                return new Dictionary<long, DuelContext>();
            }

            var json = await File.ReadAllTextAsync(StorageFilePath);
            return JsonSerializer.Deserialize<Dictionary<long, DuelContext>>(json);
        }

        public async Task<DuelContext> LoadDuelContextByUserIdAsync(long userId)
        {
            var duels = await LoadDuelContextAsync();
            return duels.Values.FirstOrDefault(context => context.Duelist1.UserGame.Id == userId || context.Duelist2.UserGame.Id == userId);
        }

        public async Task ClearDuelContextAsync()
        {
            if (File.Exists(StorageFilePath))
            {
                await File.WriteAllTextAsync(StorageFilePath, "{}");
            }
        }
    }
}
