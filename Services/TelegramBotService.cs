using RandomTGBot.Services;
using System.Threading.Tasks;
using System.Threading;
using System;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Telegram.Bot.Types.InputFiles;
using RandomTGBot;
using RandomTGBot.Models;

public class TelegramBotService : ITelegramBotService
{
    private readonly TelegramBotClient botClient;
    private readonly RiddleService riddleService;
    private readonly UserService userService;
    private readonly UserGameService userGameService;
    private readonly GameProcess gameProcess;
    private bool Semafore = false;
    private const int maxAttempts = 3;
    private static HashSet<string> Admins = new HashSet<string> { "Domanstik", "I_am_Captain_Pepe", "zazazyw", "prostojoker" };
    private static HashSet<string> Can = new HashSet<string> { "Mari_Shultz", "Feeman_dev", "brandapi", "Korzhinko", "KiroZiro", "benzomaggedon0_0", "BossFeeman" };
    private static HashSet<string> Who = new HashSet<string> {"Ebat_imba", "khm_qzumas"};
    private static readonly string[] Stickers = {
        "CAACAgIAAxkBAAEMbspmhrnm0DnUYC4WgRNbAw9sZFXgcQACh08AAu6pOEgzwN2zLru2dzUE" // Замените на ID ваших стикеров
    };

    public TelegramBotService(TelegramBotClient botClient, RiddleService riddleService, UserService userService, UserGameService userGameService, GameProcess gameProcess)
    {
        this.botClient = botClient;
        this.riddleService = riddleService;
        this.userService = userService;
        this.userGameService = userGameService;
        this.gameProcess = gameProcess;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
        {
            var messageText = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            /*
            await Console.Out.WriteLineAsync(
                    $"ID: {update.Message.From.Id,-10} || " +
                    $"Username: {update.Message.From.Username,-15} || " +
                    $"Message: {messageText,-30}"
                );
            */

            if (messageText.StartsWith("/"))
            {
                await Console.Out.WriteLineAsync(
                    $"ID: {update.Message.From.Id,-10} || " +
                    $"Username: {update.Message.From.Username,-15} || " +
                    $"Message: {messageText,-30}"
                );
            }
            
            // Run EnsureUserExistsAsync asynchronously
            
            Task ensureUserTask = EnsureUserExistsAsync(update);
            
            // Check the command and run the respective handler asynchronously
            if (messageText.StartsWith("/riddle"))
            {
                Task.Run(() => HandleRiddleAnswer(update, cancellationToken, messageText));
            }
            else if (messageText.StartsWith("/leaderboard"))
            {
                Task.Run(() => HandleLeaders(update, cancellationToken, messageText));
            }
            else if (messageText.StartsWith("/duel reset"))
            {
                try
                {
                    if (Admins.Contains(update.Message.From.Username) || Can.Contains(update.Message.From.Username))
                    {
                        Task.Run(() => gameProcess.ClearDuelContextAsync());

                        await botClient.SendTextMessageAsync(
                            chatId,
                            "Дуэли почищены",
                            replyToMessageId: update.Message.MessageId,
                            cancellationToken: cancellationToken
                        );
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Не заслужил",
                            replyToMessageId: update.Message.MessageId,
                            disableNotification: true,
                            cancellationToken: cancellationToken
                        );
                    }
                }
                catch (Exception)
                {

                    await botClient.SendTextMessageAsync(
                                chatId,
                                "Ты кто такой воин?",
                                replyToMessageId: update.Message.MessageId,
                                disableNotification: true,
                                cancellationToken: cancellationToken
                            );
                }
            }
            else if (messageText.StartsWith("/duel "))
            {
                var opponentUsername = messageText.Split(" ")[1];
                await gameProcess.StartDuelAsync(update, cancellationToken, opponentUsername);
            }
            else if (messageText.StartsWith("/quest"))
            {
                Task.Run(() => HandleLeaders(update, cancellationToken, messageText));
            }
            else 
            if (messageText.StartsWith("/reset"))
            {
                Task.Run(() => HandleReset(update, cancellationToken, messageText));
            }
            else if (messageText.StartsWith("/addRiddle"))
            {
                Task.Run(() => HandleAddRiddle(update, cancellationToken, messageText));
            }
            else if (messageText.StartsWith("/removeRiddle"))
            {
                Task.Run(() => HandleRemoveRiddle(update, cancellationToken, messageText));
            }
            else if (messageText.StartsWith("/skip"))
            {
                Task.Run(() => HandleSkipRiddle(update, cancellationToken, messageText));
            }
            else if (messageText.StartsWith("/report"))
            {
                await botClient.SendTextMessageAsync(
                        chatId,
                        "Техника призыва главного модера🥷🏿\n\n@Domanstik\n@feeman_moder",
                        replyToMessageId: update.Message.MessageId,
                        cancellationToken: cancellationToken
                    );
            }
            else if (messageText.StartsWith("/FeemanDaiDeneg"))
            {
                try
                {

                    if (Who.Contains(update.Message.From.Username))
                    {
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "Отобрано 250 $FMAN за плохое поведение",
                            replyToMessageId: update.Message.MessageId,
                            cancellationToken: cancellationToken
                        );
                    }
                    else if (Admins.Contains(update.Message.From.Username) || Can.Contains(update.Message.From.Username))
                    {
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "Получено 250 $FMAN",
                            replyToMessageId: update.Message.MessageId,
                            cancellationToken: cancellationToken
                        );
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Не заслужил",
                            replyToMessageId: update.Message.MessageId,
                            disableNotification: true,
                            cancellationToken: cancellationToken
                        );
                    }
                }
                catch (Exception)
                {

                    await botClient.SendTextMessageAsync(
                                chatId,
                                "Ты кто такой воин?",
                                replyToMessageId: update.Message.MessageId,
                                disableNotification: true,
                                cancellationToken: cancellationToken
                            );
                }
            }
            else if (messageText.ToLower().Contains("листинг"))
            {
                Random rand = new Random();
                int choice = rand.Next(3); 

                if (choice == 0)
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "Не будет листинга, всё скам, расходимся",
                        replyToMessageId: update.Message.MessageId,
                        disableNotification: true,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    var stickerId = Stickers[rand.Next(Stickers.Length)];
                    await botClient.SendStickerAsync(
                        chatId,
                        stickerId,
                        replyToMessageId: update.Message.MessageId,
                        disableNotification: true,
                        cancellationToken: cancellationToken
                    );
                }
            }
            else if (messageText.StartsWith("https://t.me/ha"))
            {
                await botClient.SendTextMessageAsync(
                        chatId,
                        "Чорт",
                        replyToMessageId: update.Message.MessageId,
                        disableNotification: true,
                        cancellationToken: cancellationToken
                    );

                /*
                using var gifStream = System.IO.File.Open(@"D:\For Documents\GIF\Hamster.mp4", FileMode.Open);
                var gifInputFile = new InputOnlineFile(gifStream, "Hamster.mp4");
                await botClient.SendAnimationAsync(
                    chatId,
                    gifInputFile,
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken
                );*/
            }
            else if (messageText.StartsWith("/prediction"))
            {
                await botClient.SendTextMessageAsync(
                                chatId,
                                "Ушли на каникулы",
                                replyToMessageId: update.Message.MessageId,
                                cancellationToken: cancellationToken
                            );

                /*
                if (!Semafore)
                {
                    Semafore = !Semafore;
                    Task.Run(() => HandlePrediction(update, cancellationToken, messageText));
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                                chatId,
                                "Ну и куда ты торопишься? Жди своей очереди",
                                replyToMessageId: update.Message.MessageId,
                                cancellationToken: cancellationToken
                            );
                }*/
            }
            
            
            // Await the ensureUserTask to ensure that the user exists in the database
            await ensureUserTask;
            
             
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                Task.Run(() => gameProcess.HandleCallbackQueryAsync(update, cancellationToken));
            }
        }
        else
        {
            return;
        }

    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
         await Task.CompletedTask;
    }

    private async Task EnsureUserExistsAsync(Update update)
    {
        var user = await userService.GetUserById(update.Message.From.Id);
        if (user == null)
        {
            RandomTGBot.Models.User newUser;
            UserGame newUserGame = new UserGame(update.Message.From.Id, 1, 0, "NoAchievements", 3, 10, 1, 1, 0);

            if (update.Message.From.Username == null)
            {
                newUser = new RandomTGBot.Models.User(update.Message.From.Id, "NoName", 0, 0, maxAttempts, update.Message.From.FirstName, 0);
            }
            else
            {
                newUser = new RandomTGBot.Models.User(update.Message.From.Id, update.Message.From.Username, 0, 0, maxAttempts, update.Message.From.FirstName, 0);
            }
            

            await userService.CreateUser(newUser);
            await userGameService.CreateUserGame(newUserGame);
        }
    }

    public async Task HandleLeaders(Update update, CancellationToken cancellationToken, string messageText)
    {
        List<RandomTGBot.Models.User> users = await userService.GetAllUsers();

        IUserSorter userSorter = new UserSorter();
        IPrintUsers userPrinter = new PrintUsers();

        userSorter.Sort(users);
        string message = userPrinter.Print(users);

        using var gifStream = System.IO.File.Open(@"D:\For Documents\GIF\Leaders.mp4", FileMode.Open);
        var gifInputFile = new InputOnlineFile(gifStream, "Leaders.mp4");
        await botClient.SendAnimationAsync(
            update.Message.Chat.Id,
            gifInputFile,
            caption: message,
            replyToMessageId: update.Message.MessageId,
            disableNotification: true,
            cancellationToken: cancellationToken
        );
    }

    public async Task HandleRiddleAnswer(Update update, CancellationToken cancellationToken, string messageText)
    {
        var chatId = update.Message.Chat.Id;
        var userId = update.Message.From.Id;
        var user = await userService.GetUserById(userId);

        var riddle = riddleService.GetCurrentRiddle();
        string answer = messageText.Substring(7).Trim();

        if (string.IsNullOrEmpty(answer))
        {
            if (riddle == null)
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    "Загадок ещё не завезли.",
                    replyToMessageId: update.Message.MessageId,
                    disableNotification: true,
                    cancellationToken: cancellationToken
                );
                await riddleService.LoadRiddlesAsync();
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    riddle.Question,
                    replyToMessageId: update.Message.MessageId,
                    disableNotification: true,
                    cancellationToken: cancellationToken
                );
            }
        }
        else
        {
            if (user != null)
            {
                if (user.Attempts > 0 && riddleService.CheckAnswer(answer))
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        "Правильно!",
                        replyToMessageId: update.Message.MessageId,
                        disableNotification: true,
                        cancellationToken: cancellationToken
                    );
                    user.ScoreRiddles++;
                    await userService.UpdateUser(userId, user);
                    riddle = riddleService.GetNextRiddle();
                }
                else
                {
                    if (user.Attempts == 0)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId,
                            "У вас закончились попытки.",
                            replyToMessageId: update.Message.MessageId,
                            disableNotification: true,
                            cancellationToken: cancellationToken
                        );
                    }
                    else
                    {
                        user.Attempts--;
                        await userService.UpdateUser(userId, user);
                        await botClient.SendTextMessageAsync(
                            chatId,
                            $"Неправильно! У вас осталось {user.Attempts} попыток.",
                            replyToMessageId: update.Message.MessageId,
                            disableNotification: true,
                            cancellationToken: cancellationToken
                        );
                    }
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                        chatId,
                        "Ты кто такой воин?",
                        replyToMessageId: update.Message.MessageId,
                        disableNotification: true,
                        cancellationToken: cancellationToken
                    );
            }
        }
    }

    private async Task HandleReset(Update update, CancellationToken cancellationToken, string messageText)
    {
        var chatId = update.Message.Chat.Id;
        var username = update.Message.From.Username;

        string attempts = messageText.Substring(6).Trim();

        if (Admins.Contains(username))
        {
            int maxAttempts;
            if (string.IsNullOrEmpty(attempts) || !int.TryParse(attempts, out maxAttempts))
            {
                maxAttempts = 3;
            }

            await ResetAttemptsAsync(maxAttempts);

            await Console.Out.WriteLineAsync(
                $"ID: {update.Message.From.Id} || " +
                $"Username: {update.Message.From.Username} || " +
                $"Answer: Попытки сброшены."
            );

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Попытки сброшены.",
                replyToMessageId: update.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await Console.Out.WriteLineAsync(
                $"ID: {update.Message.From.Id} || " +
                $"Username: {update.Message.From.Username} || " +
                $"Answer: Эээ Вуася, ты хто таке? (Данная команда вам недоступна)"
            );

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Эээ Вуася, ты хто таке? (Данная команда вам недоступна)",
                replyToMessageId: update.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task HandleAddRiddle(Update update, CancellationToken cancellationToken, string messageText)
    {
        var chatId = update.Message.Chat.Id;
        var username = update.Message.From.Username;

        if (Admins.Contains(username))
        {
            // Extract the riddle question and answer from the message text
            string[] parts = messageText.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                string question = parts[1].Trim();
                string answer = parts[2].Trim();

                // Create a new riddle object
                Riddle newRiddle = new Riddle
                {
                    Question = question,
                    Answer = answer
                };

                // Add the new riddle to the service
                await riddleService.AddRiddle(newRiddle);

                // Send confirmation message
                await botClient.SendTextMessageAsync(
                    chatId,
                    $"Новая загадка добавлена:\n\nВопрос: {question}\nОтвет: {answer}",
                    replyToMessageId: update.Message.MessageId,
                    disableNotification: true,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    "Некорректный формат сообщения. Используйте /addRiddle /вопрос /ответ",
                    replyToMessageId: update.Message.MessageId,
                    disableNotification: true,
                    cancellationToken: cancellationToken
                );
            }
        }
        else
        {
            await Console.Out.WriteLineAsync(
                $"ID: {update.Message.From.Id} || " +
                $"Username: {update.Message.From.Username} || " +
                $"Answer: Эээ Вуася, ты хто таке? (Данная команда вам недоступна)"
            );

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Эээ Вуася, ты хто таке? (Данная команда вам недоступна)",
                replyToMessageId: update.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task HandleRemoveRiddle(Update update, CancellationToken cancellationToken, string messageText)
    {
        var chatId = update.Message.Chat.Id;
        var username = update.Message.From.Username;

        if (Admins.Contains(username))
        {
            await riddleService.RemoveLastRiddle();

            await botClient.SendTextMessageAsync(
                chatId,
                $"Последняя загадка убрана",
                replyToMessageId: update.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await Console.Out.WriteLineAsync(
                $"ID: {update.Message.From.Id} || " +
                $"Username: {update.Message.From.Username} || " +
                $"Answer: Эээ Вуася, ты хто таке? (Данная команда вам недоступна)"
            );

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Эээ Вуася, ты хто таке? (Данная команда вам недоступна)",
                replyToMessageId: update.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task HandleSkipRiddle(Update update, CancellationToken cancellationToken, string messageText)
    {
        var chatId = update.Message.Chat.Id;
        var username = update.Message.From.Username;

        if (Admins.Contains(username))
        {
            riddleService.RemoveCurrentRiddle();

            await botClient.SendTextMessageAsync(
                chatId,
                $"Загадка пропущена!",
                replyToMessageId: update.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await Console.Out.WriteLineAsync(
                $"ID: {update.Message.From.Id} || " +
                $"Username: {update.Message.From.Username} || " +
                $"Answer: Эээ Вуася, ты хто таке? (Данная команда вам недоступна)"
            );

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Эээ Вуася, ты хто таке? (Данная команда вам недоступна)",
                replyToMessageId: update.Message.MessageId,
                disableNotification: true,
                cancellationToken: cancellationToken
            );
        }
    }

    private async Task ResetAttemptsAsync(int maxAttempts)
    {
        var users = await userService.GetAllUsers();
        foreach (var user in users)
        {
            user.Attempts = maxAttempts;
            await userService.UpdateUser(user.Id, user);
        }
    }

    private async Task HandlePrediction(Update update, CancellationToken cancellationToken, string messageText)
    {
        var chatId = update.Message.Chat.Id;
        var username = update.Message.From.Username;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.GetEncoding("Windows-1251");

        try
        {
            string pythonScriptPath = @"D:\Python\logalGPT\myNN_gpt_query.py";

            if (string.IsNullOrEmpty(pythonScriptPath) || !System.IO.File.Exists(pythonScriptPath))
            {
                Console.WriteLine($"Ошибка: скрипт Python не найден по пути: {pythonScriptPath}");
                return;
            }

            string pythonInterpreterPath = @"D:\Python\logalGPT\env\Scripts\python.exe";

            if (string.IsNullOrEmpty(pythonInterpreterPath) || !System.IO.File.Exists(pythonInterpreterPath))
            {
                Console.WriteLine($"Ошибка: интерпретатор Python не найден по пути: {pythonInterpreterPath}");
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string question = "Давай представим, что ты опытная гадалка, а я клиент. Сделай мне предсказание на ближайшее время в стиле гадалки";

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pythonInterpreterPath,
                Arguments = $"{pythonScriptPath} \"{question}\"",
                WorkingDirectory = Path.GetDirectoryName(pythonScriptPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var task = Task.Run(async () =>
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string response = await reader.ReadToEndAsync();
                        string[] responseLines = response.Split("\n");
                        string result = responseLines[0];
                        Console.WriteLine($"Результат: {result}");

                        await Console.Out.WriteLineAsync(
                            $"ID: {update.Message.From.Id} || " +
                            $"Username: {update.Message.From.Username} || " +
                            $"Answer: Предсказание"
                        );

                        await botClient.SendTextMessageAsync(
                            chatId,
                            result,
                            replyToMessageId: update.Message.MessageId,
                            disableNotification: true,
                            cancellationToken: cancellationToken
                        );
                    }

                    using (StreamReader errorReader = process.StandardError)
                    {
                        string error = await errorReader.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine($"Ошибка: {error}");
                        }
                    }

                    await process.WaitForExitAsync();
                    int exitCode = process.ExitCode;
                    Console.WriteLine($"Процесс завершился с кодом: {exitCode}");
                }
            });

            // Run other code while the neural network request is processing
            await DoOtherWorkWhileWaiting(update, chatId, cancellationToken);

            // Wait for the Python script execution to complete
            await task;

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine($"Время выполнения: {ts.TotalMilliseconds} мс");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Исключение: {ex.Message}");
            Console.WriteLine($"Стек вызовов: {ex.StackTrace}");
        }

        Semafore = !Semafore;
    }

    private async Task DoOtherWorkWhileWaiting(Update update, long chatId, CancellationToken cancellationToken)
    {
        // Here you can perform other asynchronous tasks
        await Task.Delay(1000); // Example delay
        await botClient.SendTextMessageAsync(
            chatId,
            "Пожалуйста, подождите, предсказание готовится...",
            replyToMessageId: update.Message.MessageId,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleRandomCommand(Update update, CancellationToken cancellationToken)
    {
        var messageText = update.Message.Text;
        var chatId = update.Message.Chat.Id;
        var parameters = messageText.Substring(7).Split('-');

        if (string.IsNullOrWhiteSpace(messageText.Substring(7)))
        {
            var random = new Random();
            int randomNumber = random.Next(1, 101);

            await botClient.SendTextMessageAsync(
                chatId,
                $"{randomNumber}",
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken
            );
        }
        else if (parameters.Length == 2 && int.TryParse(parameters[0], out int firstNumber) && int.TryParse(parameters[1], out int secondNumber))
        {
            var random = new Random();
            int randomNumber = random.Next(firstNumber, secondNumber + 1);

            await botClient.SendTextMessageAsync(
                chatId, 
                $"{randomNumber}",
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken
            );
        }
        else if (parameters.Length == 1 && int.TryParse(parameters[0], out int maxNumber))
        {
            var random = new Random();
            int randomNumber = random.Next(1, maxNumber + 1);

            await botClient.SendTextMessageAsync(
                chatId, 
                $"{randomNumber}",
                replyToMessageId: update.Message.MessageId,
                cancellationToken: cancellationToken
             );
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId, 
                "Использование: \n/random \n/random [число] \n/random [первое число]-[второе число].", 
                replyToMessageId: update.Message.MessageId, 
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task UpdateUserFirstNameAsync()
    {
        var users = await userService.GetAllUsers();

        foreach (var user in users)
        {
            if (user != null)
            {
                var chatMember = await botClient.GetChatMemberAsync(user.Id, user.Id);
                var firstName = chatMember.User.FirstName;
                user.Name = firstName;
                await Console.Out.WriteLineAsync($"Update user with id = {user.Id} and Name = {user.Name}");
                await userService.UpdateUser(user.Id, user);
            }
        }
    }
}
