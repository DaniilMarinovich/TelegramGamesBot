using RandomTGBot.Services;
using RandomTGBot.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class Game : IGame
{
    private readonly TelegramBotClient botClient;
    private readonly UserGameService userGameService;
    private readonly UserService userService;
    private UserGame player1;
    private UserGame player2;
    private Duel duelist1;
    private Duel duelist2;
    private long chatId;
    private int messageId;

    public Game(TelegramBotClient botClient, UserGameService userGameService, UserService userService)
    {
        this.botClient = botClient;
        this.userGameService = userGameService;
        this.userService = userService;
    }

    public async Task StartDuelAsync(Update update, CancellationToken cancellationToken, string opponentUsername)
    {
        chatId = update.Message.Chat.Id;
        messageId = update.Message.MessageId;

        player1 = await userGameService.GetUserGameById(update.Message.From.Id);

        duelist1 = new Duel(player1);

        long opponentId;
        try
        {
            opponentId = await userService.GetUserIdByUsername(opponentUsername);
        }
        catch (KeyNotFoundException)
        {
            await botClient.SendTextMessageAsync(chatId, $"Противник с именем '{opponentUsername}' не найден.", cancellationToken: cancellationToken);
            return;
        }

        player2 = await userGameService.GetUserGameById(opponentId);

        duelist2 = new Duel(player2);

        string player1Username = update.Message.From.Username;

        await botClient.SendTextMessageAsync(chatId, $"Дуэль началась между {player1Username} и {opponentUsername}!", replyMarkup: GetActionButtons(), cancellationToken: cancellationToken);
    }

    public async Task HandleAttackAsync(Update update, CancellationToken cancellationToken, string attackPart)
    {
        // Реализация обработки атаки
        await UpdateGameMessageAsync();
    }

    public async Task HandleDefenseAsync(Update update, CancellationToken cancellationToken, string defensePart)
    {
        // Реализация обработки защиты
        await UpdateGameMessageAsync();
    }

    public async Task UpdateGameMessageAsync()
    {
        string gameStatus = $"Игрок 1 (HP: {duelist1.Hp}, Damage: {duelist1.Damage})\n" +
                            $"Игрок 2 (HP: {duelist2.Hp}, Damage: {duelist2.Damage})";

        await botClient.EditMessageTextAsync(chatId, messageId, gameStatus, replyMarkup: (InlineKeyboardMarkup)GetActionButtons());
    }

    private IReplyMarkup GetActionButtons()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Атака головы", "attack_head"),
                InlineKeyboardButton.WithCallbackData("Защита головы", "defend_head"),   
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Атака туловища", "attack_body"),
                InlineKeyboardButton.WithCallbackData("Защита туловища", "defend_body")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Атака ног", "attack_legs"),                
                InlineKeyboardButton.WithCallbackData("Защита ног", "defend_legs")
            }
        });
    }
}
