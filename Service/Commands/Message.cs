using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using NakormiBot.ViewModels;

namespace NakormiBot.Service.Commands
{

    public class Message
    {
        private CommandRoute route;
        public Message(CommandRoute route)
        {
            this.route = route;
        }
        /// <summary>
        /// Команда для отправки сообщения пользователю
        /// </summary>
        public async Task Send(ITelegramBotClient botClient, long chatId, string msg, CancellationToken cancellationToken, OptionTelegramMessage option = null)
        {
            try
            {
                if (option == null)
                {
                    var sentMessage = await botClient.SendTextMessageAsync(
                         chatId: chatId,
                         text: msg,
                         parseMode: ParseMode.Html,
                         cancellationToken: cancellationToken);
                }
                else
                {
                    if (option.ClearMenu)
                    {
                        var sentMessage = await botClient.SendTextMessageAsync(
                             chatId: chatId,
                             text: msg,
                             parseMode: ParseMode.Html,
                             replyMarkup: new ReplyKeyboardRemove(),
                             cancellationToken: cancellationToken);
                    }
                    else if (option.MenuReplyKeyboardMarkup != null)
                    {
                        var sentMessage = await botClient.SendTextMessageAsync(
                             chatId: chatId,
                             text: msg,
                             parseMode: ParseMode.Html,
                             replyMarkup: option.MenuReplyKeyboardMarkup,
                             cancellationToken: cancellationToken);
                    }
                    else if (option.MenuInlineKeyboardMarkup != null)
                    {

                        await botClient.SendChatActionAsync(
                            chatId: chatId,
                            chatAction: ChatAction.ChooseSticker,
                            cancellationToken: cancellationToken);
                        var sentMessage = await botClient.SendTextMessageAsync(
                             chatId: chatId,
                             text: msg,
                             parseMode: ParseMode.Html,
                             replyMarkup: option.MenuInlineKeyboardMarkup,
                             cancellationToken: cancellationToken);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка отправки сообщений в телеграм | " + ex);
                throw new Exception(ex.Message);
            }

        }

        public async Task SendPhoto(ITelegramBotClient botClient, long chatId, string caption,string fileId, CancellationToken cancellationToken, OptionTelegramMessage option = null)
        {
            try
            {
                var inputFile = new InputFileId(fileId);
                if (option == null)
                {
                    var sentMessage = await botClient.SendPhotoAsync(
                         chatId: chatId,
                         caption: caption,
                         photo:inputFile,
                         parseMode: ParseMode.Html,
                         cancellationToken: cancellationToken);
                }
                else
                {
                    if (option.ClearMenu)
                    {
                        var sentMessage = await botClient.SendPhotoAsync(
                              chatId: chatId,
                              caption: caption,
                              photo: inputFile,
                              parseMode: ParseMode.Html,
                             replyMarkup: new ReplyKeyboardRemove(),
                             cancellationToken: cancellationToken);
                    }
                    else if (option.MenuReplyKeyboardMarkup != null)
                    {
                        var sentMessage = await botClient.SendPhotoAsync(
                              chatId: chatId,
                              caption: caption,
                              photo: inputFile,
                             parseMode: ParseMode.Html,
                             replyMarkup: option.MenuReplyKeyboardMarkup,
                             cancellationToken: cancellationToken);
                    }
                    else if (option.MenuInlineKeyboardMarkup != null)
                    {


                        var sentMessage = await botClient.SendPhotoAsync(
                              chatId: chatId,
                              caption: caption,
                              photo: inputFile,
                             parseMode: ParseMode.Html,
                             replyMarkup: option.MenuInlineKeyboardMarkup,
                             cancellationToken: cancellationToken);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка отправки сообщений в телеграм | " + ex);
                throw new Exception(ex.Message);
            }

        }


    }
}
