using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RandomTGBot.Models;
using Telegram.Bot.Exceptions;
using System.Xml.Linq;

namespace RandomTGBot.Services
{
    public class GameProcess : IGameProcess
    {
        private readonly TelegramBotClient botClient;
        private readonly UserGameService userGameService;
        private readonly UserService userService;
        private readonly DuelStorageService duelStorageService;
        private Dictionary<long, DuelContext> duels;
        private bool Semafore = false;

        public GameProcess(TelegramBotClient botClient, UserGameService userGameService, UserService userService, DuelStorageService duelStorageService)
        {
            this.botClient = botClient;
            this.userGameService = userGameService;
            this.userService = userService;
            this.duelStorageService = duelStorageService;

            // Load duels from storage
            duels = duelStorageService.LoadDuelContextAsync().GetAwaiter().GetResult();
        }

        public async Task StartDuelAsync(Update update, CancellationToken cancellationToken, string opponentUsername)
        {
            var player1 = await userGameService.GetUserGameById(update.Message.From.Id);
            if (player1 == null)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Вы не зарегистрированы в игре.", cancellationToken: cancellationToken);
                return;
            }

            long opponentId;
            try
            {
                opponentId = await userService.GetUserIdByUsername(opponentUsername);
            }
            catch (KeyNotFoundException)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Противник с именем '{opponentUsername}' не найден.", cancellationToken: cancellationToken);
                return;
            }

            if (duels.ContainsKey(update.Message.From.Id) || duels.ContainsKey(opponentId))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Один из игроков уже находится в дуэли.", cancellationToken: cancellationToken);
                return;
            }

            var player2 = await userGameService.GetUserGameById(opponentId);
            if (player2 == null)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Противник не зарегистрирован в игре.", cancellationToken: cancellationToken);
                return;
            }

            string player1Username = update.Message.From.Username;
            var message = await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Дуэль началась между {player1Username} и {opponentUsername}!", replyMarkup: GetActionButtons(), cancellationToken: cancellationToken);
            int messageid = message.MessageId;

            var context = new DuelContext(message.Chat.Id, new Duel(player1), new Duel(player2), messageid, player1Username, opponentUsername, 0);

            duels[update.Message.From.Id] = context;
            duels[opponentId] = context;

            // Save updated duels to storage
            
            await duelStorageService.SaveDuelContextAsync(duels);
        }

        public async Task HandleAttackAsync(Update update, CancellationToken cancellationToken, string attackPart)
        {
            if (!duels.TryGetValue(update.CallbackQuery.From.Id, out var context))
            {
                // Attempt to load context from storage
                context = await duelStorageService.LoadDuelContextByUserIdAsync(update.CallbackQuery.From.Id);
                if (context == null)
                {
                    return;
                }

                // Add the loaded context to the dictionary
                duels[context.Duelist1.UserGame.Id] = context;
                duels[context.Duelist2.UserGame.Id] = context;
            }

            // Check if the attack action has already been chosen
            var duelist = context.GetDuelist(update.CallbackQuery.From.Id);
            if (duelist.HasChosenAttackAction)
            {
                return;
            }

            context.HandleAttack(update.CallbackQuery.From.Id, attackPart);

            if (context.IsTurnReady)
            {
                ProcessTurn(context);
            }

            Task.Run(() => UpdateGameMessageAsync(context));

           // Save updated duels to storage
           await duelStorageService.SaveDuelContextAsync(duels);
        }

        public async Task HandleDefenseAsync(Update update, CancellationToken cancellationToken, string defensePart)
        {
            if (!duels.TryGetValue(update.CallbackQuery.From.Id, out var context))
            {
                // Attempt to load context from storage
                context = await duelStorageService.LoadDuelContextByUserIdAsync(update.CallbackQuery.From.Id);
                if (context == null)
                {
                    return;
                }

                // Add the loaded context to the dictionary
                duels[context.Duelist1.UserGame.Id] = context;
                duels[context.Duelist2.UserGame.Id] = context;
            }

            // Check if the defense action has already been chosen
            var duelist = context.GetDuelist(update.CallbackQuery.From.Id);
            if (duelist.HasChosenDefenceAction)
            {
                return;
            }

            context.HandleDefense(update.CallbackQuery.From.Id, defensePart);

            if (context.IsTurnReady)
            {
                ProcessTurn(context);
            }

            Task.Run(() => UpdateGameMessageAsync(context));

            // Save updated duels to storage
            await duelStorageService.SaveDuelContextAsync(duels);
        }

        private void ProcessTurn(DuelContext context)
        {
            if (context.Duelist1.AttackPart != context.Duelist2.DefensePart)
            {
                context.Duelist2.Hp -= context.Duelist1.Damage;
            }

            if (context.Duelist2.AttackPart != context.Duelist1.DefensePart)
            {
                context.Duelist1.Hp -= context.Duelist2.Damage;
            }
            
            if (context.Duelist1.Hp <= 0 && context.Duelist2.Hp <= 0)
            {
                EndDuel(context, true);
            }
            else if (context.Duelist1.Hp <= 0 || context.Duelist2.Hp <= 0)
            {
                EndDuel(context, false);
            }
            else
            {
                context.ResetActions();
            }
        }

        private async void EndDuel(DuelContext context, bool isDraw)
        {

            if (isDraw)
            {
                context.Duelist1.UserGame.Money += 5;
                context.Duelist2.UserGame.Money += 5;

                await botClient.EditMessageTextAsync(context.ChatId, context.MessageId, $"Ничья!");
            }
            else
            {
                var winner = context.Duelist1.Hp > 0 ? context.Duelist1 : context.Duelist2;
                winner.UserGame.Money += 10;
                await userGameService.UpdateUserGame(winner.UserGame.Id, winner.UserGame);
                string name;
                if (winner == context.Duelist1)
                {
                    name = context.Name1;
                }
                else
                {
                    name = context.Name2;
                }

                Task.Run(() => botClient.EditMessageTextAsync(context.ChatId, context.MessageId, $"Победитель: {name}"));
            }

            context.Duelist1.UserGame.Experience += 1;
            context.Duelist2.UserGame.Experience += 1;

            await userGameService.UpdateUserGame(context.Duelist1.UserGame.Id, context.Duelist1.UserGame);
            await userGameService.UpdateUserGame(context.Duelist2.UserGame.Id, context.Duelist2.UserGame);

            duels.Remove(context.Duelist1.UserGame.Id);
            duels.Remove(context.Duelist2.UserGame.Id);

            // Save updated duels to storage
            await duelStorageService.SaveDuelContextAsync(duels);
        }

        public async Task UpdateGameMessageAsync(DuelContext context)
        {
            while (Semafore)
            {
                await Task.Delay(500);
            }

            if (!Semafore)
            {
                Semafore = !Semafore;
            }
            
            int temp = context.Round++;
            string gameStatus = $"Раунд {temp/4}\n\n";

            int hp1 = context.Duelist1.Hp < 0 ? 0 : context.Duelist1.Hp;
            gameStatus += $"{context.Name1} (HP: {context.Duelist1.Hp}, Damage: {context.Duelist1.Damage})\n";
            /*
            if (context.Duelist1.HasChosenAttackAction && context.Duelist1.HasChosenDefenceAction)
            {
                gameStatus += $"Статус хода : Выбор сделан\n";
            }
            else
            {
                gameStatus += $"Статус хода : Выбор не сделан\n";
            }*/
            
            if (context.Duelist1.HasChosenAttackAction)
            {
                gameStatus += $"Статус атаки : Выбор сделан\n";
            }
            else
            {
                gameStatus += $"Статус атаки : Выбор не сделан\n";
            }

            if (context.Duelist1.HasChosenDefenceAction)
            {
                gameStatus += $"Статус защиты : Выбор сделан\n\n";
            }
            else
            {
                gameStatus += $"Статус защиты : Выбор не сделан\n\n";
            }
            
            int hp2 = context.Duelist2.Hp > 0 ? 0 : context.Duelist2.Hp;
            gameStatus += $"{context.Name2} (HP: {context.Duelist2.Hp}, Damage: {context.Duelist2.Damage})\n";
            /*
            if (context.Duelist2.HasChosenAttackAction && context.Duelist2.HasChosenDefenceAction)
            {
                gameStatus += $"Статус хода : Выбор сделан\n";
            }
            else
            {
                gameStatus += $"Статус хода : Выбор не сделан\n";
            }
            */
            
            if (context.Duelist2.HasChosenAttackAction)
            {
                gameStatus += $"Статус атаки : Выбор сделан\n";
            }
            else
            {
                gameStatus += $"Статус атаки : Выбор не сделан\n";
            }

            if (context.Duelist2.HasChosenDefenceAction)
            {
                gameStatus += $"Статус защиты : Выбор сделан\n";
            }
            else
            {
                gameStatus += $"Статус защиты : Выбор не сделан\n";
            }

            if (duels.ContainsKey(context.Duelist1.UserGame.Id) && duels.ContainsKey(context.Duelist2.UserGame.Id))
            {
                Task.Run(() => botClient.EditMessageTextAsync(context.ChatId, context.MessageId, gameStatus, replyMarkup: GetActionButtons()));
            }

            Semafore = !Semafore;
        }

        private InlineKeyboardMarkup GetActionButtons()
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

        public async Task HandleCallbackQueryAsync(Update update, CancellationToken cancellationToken)
        {
            var callbackQuery = update.CallbackQuery;
            var message = callbackQuery.Message;
            var data = callbackQuery.Data;

            if (data.StartsWith("attack_"))
            {
                string attackPart = data.Replace("attack_", "");
                await HandleAttackAsync(update, cancellationToken, attackPart);
            }
            else if (data.StartsWith("defend_"))
            {
                string defensePart = data.Replace("defend_", "");
                await HandleDefenseAsync(update, cancellationToken, defensePart);
            }

            //await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
        }

        public async Task ClearDuelContextAsync()
        {
            await duelStorageService.ClearDuelContextAsync();
            duels.Clear();
        }
    }
}
