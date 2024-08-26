using RandomTGBot.Models;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RandomTGBot.Services
{
    public interface IGameProcess
    {
        Task StartDuelAsync(Update update, CancellationToken cancellationToken, string opponentUsername);
        Task HandleAttackAsync(Update update, CancellationToken cancellationToken, string attackPart);
        Task HandleDefenseAsync(Update update, CancellationToken cancellationToken, string defensePart);
        Task UpdateGameMessageAsync(DuelContext context); // Ensure this method is present
    }
}
