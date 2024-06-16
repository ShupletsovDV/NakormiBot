using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using NakormiBot.Service.Commands;
using NakormiBot.ViewModels;
using System.Windows;

namespace NakormiBot.Service
{
    public class CommandRoute
    {
        #region кнопки
        public const string AddVolonter = "Добавить волонтера";
        public const string AddPointCollection = "Добавить точку сбора";
        public const string ViewVolonters = "Посмотреть волонтеров";
        public const string ViewPoinCollection = "Посмотреть точки сбора";
        public const string AuthAdmin = "/auth";
        public const string MENU = "Меню";
        public const string START = "/start";
        public const string GetKorm = "Забрать корм";
        public const string GiveKorm = "Передать корм";
        public const string FeedAnimal = "Накормить";
        public const string SelectPoint = "Выбрать эту точку";
        public const string SelectVolonterGive = "Передать";
        public const string SelectKormPhoto = "1";
        public const string ViewSelfCard = "Моя карточка";
        public const string GiveKormVolonter = "2";
        public const string FeedKormVolonter = "3";
        public const string SearchPoint = "Найти точку";
        public const string SearchVolonter = "Найти волонтера";
        public const string ViewStatisticAdmin = "Выгрузить статистику";
        public const string RedactPoint = "Редактировать";
        public const string RDeletePoint = "Удалить";
        public const string AddKormPoint = "Добавить корм";


        #endregion

        private delegate Task TypeCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
        private Dictionary<string, TypeCommand> _commands = new Dictionary<string, TypeCommand>();

        public Commands.Common CommonCommand;
        public Commands.Message MessageCommand;

        public CommandRoute()
        {
            CommonCommand = new Commands.Common(this);
            MessageCommand = new Commands.Message(this);
            RegisterUserCommand();
        }

        /// <summary>
        /// Регистрация доступных команд
        /// </summary>
        public void RegisterUserCommand()
        {
            _commands.Add(START, CommonCommand.Start);
            _commands.Add(AuthAdmin,CommonCommand.AuthAdmin);
            _commands.Add(AddVolonter, CommonCommand.AddVolonter);
            _commands.Add("", CommonCommand.AddPasportVolonter);
            _commands.Add(MENU, CommonCommand.Menu);
            _commands.Add(AddPointCollection, CommonCommand.AddPointCollection);
            _commands.Add(GetKorm, CommonCommand.GetKorm);
            _commands.Add(GiveKorm, CommonCommand.GiveKorm);
            _commands.Add(FeedAnimal, CommonCommand.FeedAnimal);
            _commands.Add(ViewPoinCollection, CommonCommand.ViewPointCollection);
            _commands.Add(ViewVolonters, CommonCommand.ViewVolonters);
            _commands.Add(SelectPoint, CommonCommand.SelectGetKorm);
            _commands.Add(SelectKormPhoto, CommonCommand.SelectGetPhotoKorm);
            _commands.Add(GiveKormVolonter, CommonCommand.PhotoKormVolonterGiveKorm);
            _commands.Add(ViewSelfCard, CommonCommand.ViewSelfCard);
            _commands.Add(SelectVolonterGive, CommonCommand.NameKormVolonterGiveKorm);
            _commands.Add(FeedKormVolonter, CommonCommand.SelectGetPhotoFeedKorm);
            _commands.Add(ViewStatisticAdmin, CommonCommand.ViewStatisticAdmin);
            _commands.Add(SearchPoint, CommonCommand.SearchPoint);
            _commands.Add(SearchVolonter, CommonCommand.SearchVolonter);
            _commands.Add(AddKormPoint, CommonCommand.AddKormPoint);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="command">Запрос команды</param>
        public async Task ExecuteCommand(string command, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    if(update.CallbackQuery.Data==SelectPoint)
                    {
                        await _commands[SelectPoint](botClient, update, cancellationToken);
                    }
                    if(update.CallbackQuery.Data==SelectVolonterGive)
                    {
                        await _commands[SelectVolonterGive](botClient, update, cancellationToken);
                    }
                    if(update.CallbackQuery.Data==AddKormPoint)
                    {
                        await _commands[AddKormPoint](botClient, update, cancellationToken);
                    }
                }
                else if (command != null &&  update.Type!=Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    var userId = update.GetChatId();
                    if (botClient.HasStep(userId))
                    {
                        if (command == MENU || command == START)
                        {
                            botClient.ClearStepUser(userId);
                            //ExecuteCommand(update.Message.Text, botClient, update, cancellationToken);
                            await new Common(this).Menu(botClient, update, cancellationToken);
                            return;
                        }
                        await botClient.GetStepOrNull(userId).Value(botClient, update, cancellationToken);
                        return;
                    }


                    foreach (var item in _commands)
                    {
                        if (item.Key.ToLower() == command.ToLower())
                        {
                            //Выполнение команды
                            await item.Value(botClient, update, cancellationToken);
                            
                            return;
                        }
                    }
                    //Сообщение что команда не найдена
                    await CommandMissing(botClient, update, cancellationToken);
                }
                else if ( update.Message.Type == Telegram.Bot.Types.Enums.MessageType.Photo && botClient.GetCacheData(update.GetChatId()).Value.IsAdmin==true )
                {

                    await _commands[""](botClient, update, cancellationToken);
                }
                else if (update.Message.Type == Telegram.Bot.Types.Enums.MessageType.Photo && botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == false)
                {

                    await _commands[botClient.GetCacheData(update.GetChatId()).Value.StepPhoto](botClient, update, cancellationToken);
                }
                else
                {
                    await CommandMissing(botClient, update, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка ExecuteCommand | " + ex);

            }
        }

        /// <summary>
        /// Не найдена команда
        /// </summary>
        public async Task CommandMissing(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            OptionTelegramMessage message = new OptionTelegramMessage();
            List<string> ids = new List<string> { CommandRoute.MENU };
            message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");

            await MessageCommand.Send(botClient, update.GetChatId(), $"Отсутствует команда '{update.Message.Text}'", cancellationToken,message);
        }
    }
}
