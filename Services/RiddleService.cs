using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RandomTGBot.Services
{
    public class RiddleService : IRiddleService
    {
        private List<Riddle> _riddles;

        private readonly string _riddlesFilePath = "riddles.json";

        public RiddleService()
        {
            _riddles = new List<Riddle>();
            LoadRiddlesAsync().Wait(); // Ждем загрузки загадок синхронно в конструкторе, чтобы обеспечить наш сервис.
        }

        public async Task LoadRiddlesAsync()
        {
            try
            {
                var jsonString = await File.ReadAllTextAsync(_riddlesFilePath);
                _riddles = JsonSerializer.Deserialize<List<Riddle>>(jsonString);
            }
            catch (FileNotFoundException)
            {
                _riddles = new List<Riddle>(); // Создаем новый список, если файл не существует
            }
        }

        public Riddle GetCurrentRiddle()
        {
            return 0 < _riddles.Count ? _riddles[0] : null;
        }

        public Riddle GetNextRiddle()
        {
            if (0 < _riddles.Count)
            {
                SaveRiddlesToFile();
                return GetCurrentRiddle();
            }
            return null;
        }

        public bool CheckAnswer(string answer)
        {
            if (0 < _riddles.Count)
            {
                bool isCorrect = string.Equals(answer, _riddles[0].Answer, StringComparison.OrdinalIgnoreCase);
                if (isCorrect)
                {
                    // Отложенное удаление загадки только если ответ правильный
                    _riddles.RemoveAt(0);
                    SaveRiddlesToFile();
                }
                return isCorrect;
            }
            return false;
        }

        public async Task AddRiddle(Riddle riddle)
        {
            _riddles.Add(riddle);
            SaveRiddlesToFile();
        }

        public async Task RemoveLastRiddle()
        {
            if (_riddles.Count > 0)
            {
                _riddles.RemoveAt(_riddles.Count - 1);
                SaveRiddlesToFile();
            }
        }

        // Метод для явного удаления текущей загадки из списка (например, при завершении работы бота)
        public void RemoveCurrentRiddle()
        {
            if (0 < _riddles.Count)
            {
                _riddles.RemoveAt(0);
                SaveRiddlesToFile();
            }
        }

        private void SaveRiddlesToFile()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Игнорируем экранирование Unicode
            };

            var jsonString = JsonSerializer.Serialize(_riddles, options);
            File.WriteAllText(_riddlesFilePath, jsonString);
        }
    }
}
