using RandomTGBot;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using Telegram.Bot.Polling;
using Telegram.Bot;
using RandomTGBot.Services;
using Telegram.Bot.Types.Enums;

namespace RandomTGBot
{
    class Program
    {
        private static TelegramBotClient botClient;
        private static readonly HttpClient client;

        static async Task Main(string[] args)
        {
            StartAPI api = new StartAPI();

            api.Start();

            botClient = new TelegramBotClient("your token"); // Замените на свой токен бота

            UserService userService = new UserService(client);
            UserGameService gameService = new UserGameService(client);
            RiddleService riddleService = new RiddleService();
            DuelStorageService duelStorageService = new DuelStorageService();
            GameProcess gameProcess = new GameProcess(botClient, gameService, userService, duelStorageService);
            await riddleService.LoadRiddlesAsync();

            ITelegramBotService telegramBotService = new TelegramBotService(botClient, riddleService, userService, gameService, gameProcess);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() }; // Пустой список допустимых обновлений

            botClient.StartReceiving(
                new DefaultUpdateHandler(telegramBotService.HandleUpdateAsync, telegramBotService.HandleErrorAsync),
                receiverOptions,
                cancellationToken
            );

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            cts.Cancel();
            api.Stop();
        }
    }
}