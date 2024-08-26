using System.Threading.Tasks;

namespace RandomTGBot.Services
{
    public interface IRiddleService
    {
        Task LoadRiddlesAsync();
        Task AddRiddle(Riddle riddle);
        Task RemoveLastRiddle();
        public Riddle GetCurrentRiddle();
        Riddle GetNextRiddle();
        bool CheckAnswer(string answer);
    }
}
