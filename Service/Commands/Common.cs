using Aspose.Cells;
using NakormiBot.Model;
using NakormiBot.ViewModels;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NakormiBot.Service.Commands
{
    public class Common
    {
        public static string IsPass;
        private CommandRoute route;
        public static Common instance;
        public static List<VolonterModel> volonters = new List<VolonterModel>();
        public static List<PointCollectionModel> points = new List<PointCollectionModel>();
        public static List<PeredachaKormaModel> peredachaKorm = new();
        public static List<LogPointCollectionModel> LogsPoint = new List<LogPointCollectionModel>();
        public static List<FeedAnimalModel> logFeed = new List<FeedAnimalModel>();
        public static List<string> PathFIles = new();
        public static List<string> adminIds = new List<string>();


        public string Login1 = "Admin";
        public string Password = "1234";
        public Common(CommandRoute route)
        {
            this.route = route;
        }
        public static Common GetCommon()
        {
            if (instance == null)
                instance = new Common(new CommandRoute());
            return instance;
        }

        public async Task Start(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//шаг старт
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите ваше ФИО", cancellationToken); //TODO по номеру
                botClient.RegisterNextStep(update.Message.Chat.Id, RegisterVolonter);
            }
            catch (WebException exception)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге /start | " + exception);
            }
        }
        public async Task AddKormPoint(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
            {
                var adresPoint = update.CallbackQuery.Message.Text.Split('\n')[1];
                var foundPoint = points.FirstOrDefault(p => p.Adres == adresPoint);

                botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint = points.IndexOf(foundPoint);
                await route.MessageCommand.Send(botClient, update.CallbackQuery.From.Id, $"Введите название корма", cancellationToken);
                botClient.RegisterNextStep(update.CallbackQuery.From.Id, AddNameKormPoint);
            }
            else
            {
                await route.MessageCommand.Send(botClient, update.GetChatId(), "У вас нет прав на это действие", cancellationToken);

            }
        }
        public async Task AddNameKormPoint(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm = update.Message.Text;
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Введите кол-во корма(кг)", cancellationToken);


                botClient.RegisterNextStep(update.Message.Chat.Id, AddCountKormPoint);
                
            }
            catch
            {
                botClient.ClearStepUser(update.Message.Chat.Id);
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddCountKormPoint(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {

                bool success = false;
                for (int j = 0; j < points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm.Count; j++)
                {
                    if (botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm.ToLower() == points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm[j].VolumeName.ToLower())
                    {
                        botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm =Convert.ToInt32( update.Message.Text);
                        points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm[j].VolumeCount+= Convert.ToInt32(update.Message.Text);
                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Корм добавлен", cancellationToken);
                        success = true;
                        break;
                    }
                }

                if (!success)
                {
                    VolumeKormModel k = new();
                    k.VolumeName = botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm;
                    k.VolumeCount= Convert.ToInt32(update.Message.Text);
                    points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm.Add(k);
                    SerialazePoint();
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Корм добавлен", cancellationToken);
                    botClient.ClearStepUser(update.Message.Chat.Id);
                }
                else
                {
                    botClient.ClearStepUser(update.Message.Chat.Id);
                }
            }
            catch
            {
                botClient.ClearStepUser(update.Message.Chat.Id);
            }
        }
        public async Task SearchPoint(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите название точки", cancellationToken);
            botClient.RegisterNextStep(update.Message.Chat.Id, GetSearchPoint);
        }
        public async Task GetSearchPoint(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var adresPoint = update.Message.Text.ToLower();
                var foundPoint = points.FirstOrDefault(p => p.Name.ToLower() == adresPoint);
                if(foundPoint == null)
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Точчка не найдена", cancellationToken);
                    botClient.ClearStepUser(update.Message.Chat.Id);
                    return;

                }
                string msg = "";
                for (int j = 0; j < points[points.IndexOf(foundPoint)].VolumeKorm.Count; j++)
                {
                    if (points[points.IndexOf(foundPoint)].VolumeKorm[j].VolumeCount > 0)
                    {
                        msg += $"{points[points.IndexOf(foundPoint)].VolumeKorm[j].VolumeName}\n{points[points.IndexOf(foundPoint)].VolumeKorm[j].VolumeCount} кг\n------\n";
                    }
                }
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    OptionTelegramMessage message = new();
                    var ikm = new InlineKeyboardMarkup(new[]
                    {
                         new[]
                             {
                                InlineKeyboardButton.WithCallbackData("Редактировать"),
                                InlineKeyboardButton.WithCallbackData(CommandRoute.AddKormPoint),

                             },
                             new[]
                             {

                                InlineKeyboardButton.WithCallbackData("Удалить")
                             }
                    });
                    message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Найденная точка\n{points[points.IndexOf(foundPoint)].Name}\n{points[points.IndexOf(foundPoint)].Adres}\n\nКорм в наличии:\n{msg}", cancellationToken, message);

                }
                else
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Найденная точка\n{points[points.IndexOf(foundPoint)].Name}\n{points[points.IndexOf(foundPoint)].Adres}\n\nКорм в наличии:\n{msg}", cancellationToken);

                }
                botClient.ClearStepUser(update.Message.Chat.Id);
            }
            catch
            {

            }
        }
        public async Task SearchVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите номер волонтера", cancellationToken);
            botClient.RegisterNextStep(update.Message.Chat.Id, GetSearchVolonter);
        }
        public async Task GetSearchVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var adresPoint = update.Message.Text.ToLower();
                var foundPoint = volonters.FirstOrDefault(p => p.Phone.ToLower() == adresPoint);
                if (foundPoint == null)
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Волонтер не найден", cancellationToken);
                    botClient.ClearStepUser(update.Message.Chat.Id);
                    return;

                }
                string msg = "";
                for (int j = 0; j < volonters[volonters.IndexOf(foundPoint)].VolumeKorm.Count; j++)
                {
                    if (volonters[volonters.IndexOf(foundPoint)].VolumeKorm[j].VolumeCount > 0)
                    {
                        msg += $"{volonters[volonters.IndexOf(foundPoint)].VolumeKorm[j].VolumeName}\n{volonters[volonters.IndexOf(foundPoint)].VolumeKorm[j].VolumeCount} кг\n------\n";
                    }
                }
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    
                
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Найденная волонер\n{volonters[volonters.IndexOf(foundPoint)].FIO}\n{volonters[volonters.IndexOf(foundPoint)].Email}\n{volonters[volonters.IndexOf(foundPoint)].Phone}\n\nКорм на руках:\n{msg}", cancellationToken);

                }
                else
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Найденная волонер\n{volonters[volonters.IndexOf(foundPoint)].FIO}\n{volonters[volonters.IndexOf(foundPoint)].Email}\n{volonters[volonters.IndexOf(foundPoint)].Phone}\n\nКорм на руках:\n{msg}", cancellationToken);

                }
                botClient.ClearStepUser(update.Message.Chat.Id);
            }
            catch
            {

            }
        }
        public async Task RegisterVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            bool success = false; //TODO пароль  для волонтера
            for (int i = 0; i < volonters.Count; i++)
            {
                if (volonters[i].FIO.ToLower().Contains(update.Message.Text.ToLower()))
                {
                    TelegramCore.getTelegramCore().bot.GetCacheData(update.Message.Chat.Id).Value.IdTelegram = update.GetChatId();
                    TelegramCore.getTelegramCore().bot.GetCacheData(update.Message.Chat.Id).Value.FIO = volonters[i].FIO;
                    TelegramCore.getTelegramCore().bot.GetCacheData(update.Message.Chat.Id).Value.Email = volonters[i].Email;
                    TelegramCore.getTelegramCore().bot.GetCacheData(update.Message.Chat.Id).Value.Phone = volonters[i].Phone;
                    TelegramCore.getTelegramCore().bot.GetCacheData(update.Message.Chat.Id).Value.FileIdPasport = volonters[i].Photo;
                    volonters[i].IdTelegram = update.GetChatId();
                    SerialazeVolonter();

                    OptionTelegramMessage message = new();
                    List<string> ids = new() { CommandRoute.GetKorm, CommandRoute.GiveKorm, CommandRoute.FeedAnimal, CommandRoute.ViewSelfCard, CommandRoute.MENU };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");

                    await route.MessageCommand.SendPhoto(botClient, update.Message.Chat.Id, $"Ваша карточка:\n🧑 {volonters[i].FIO}\n📧 {volonters[i].Email}\n📱 {volonters[i].Phone}", volonters[i].Photo, cancellationToken, message);

                    success = true;
                    botClient.ClearStepUser(update.Message.Chat.Id);
                    break;
                }
            }
            if (!success)
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Ваша карточка не найдена. Попробуйте снова", cancellationToken);
                botClient.RegisterNextStep(update.Message.Chat.Id, RegisterVolonter);
            }
        }
        public async Task FeedAnimal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
            var p = volonters.FirstOrDefault(p => p.FIO == q);
            int index = volonters.IndexOf(p);
            string msg = "";
            for (int j = 0; j < volonters[index].VolumeKorm.Count; j++)
            {
                msg += $"{volonters[index].VolumeKorm[j].VolumeName}\n{volonters[index].VolumeKorm[j].VolumeCount} кг\n------\n";
            }

            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите названия корма для кормления:\n\n{msg}", cancellationToken);
            botClient.RegisterNextStep(update.Message.Chat.Id, SelectNameKormFeedAnimal);
        }
        public async Task SelectNameKormFeedAnimal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            bool succes = false;
            var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
            var p = volonters.FirstOrDefault(p => p.FIO == q);
            int index = volonters.IndexOf(p);

            for (int j = 0; j < volonters[index].VolumeKorm.Count; j++)
            {
                if (update.Message.Text.ToLower() == volonters[index].VolumeKorm[j].VolumeName.ToLower())
                {
                    botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm = update.Message.Text;
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите кол-во корма (доступно {volonters[index].VolumeKorm[j].VolumeCount} кг)", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, SelectCountKormFeedAnimal);
                    succes = true;
                    break;
                }
            }

            if (!succes)
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Нет корма с таким названием.\nВведите название корма снова", cancellationToken);
                botClient.RegisterNextStep(update.Message.Chat.Id, SelectNameKormFeedAnimal);
            }
        }
        public async Task SelectCountKormFeedAnimal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            bool succes = false;
            var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
            var p = volonters.FirstOrDefault(p => p.FIO == q);
            int index = volonters.IndexOf(p);

            var foundKorm = p.VolumeKorm.FirstOrDefault(p => p.VolumeName == botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm);
            if (int.Parse(update.Message.Text) <= foundKorm.VolumeCount && int.Parse(update.Message.Text) > 0)
            {
                botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm = int.Parse(update.Message.Text);
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Сфотографируйте и прикрепите фото", cancellationToken);
                botClient.GetCacheData(update.GetChatId()).Value.StepPhoto = CommandRoute.FeedKormVolonter;
                botClient.RegisterNextStep(update.Message.Chat.Id, SelectGetPhotoFeedKorm);
            }
            else
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введенно не корректное значение\nВведите кол-во корма", cancellationToken);
                botClient.RegisterNextStep(update.Message.Chat.Id, SelectCountKormFeedAnimal);
            }
        }
        public async Task SelectGetPhotoFeedKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var fileId = update.Message.Photo.Last().FileId;
            var korm = new VolumeKormModel();
            korm.FileIdPhoto = fileId;
            korm.VolumeName = botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm;
            korm.VolumeCount = botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm;

            var j = botClient.GetCacheData(update.GetChatId()).Value.FIO;
            var p = volonters.FirstOrDefault(p => p.FIO == j);
            p.VolumeKorm.FirstOrDefault(p => p.VolumeName == botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm).VolumeCount -= korm.VolumeCount;

            volonters[volonters.IndexOf(p)] = p;
            botClient.ClearStepUser(update.Message.Chat.Id);
            SerialazeVolonter();

            //TODO записать кормление
            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы накормили кормом {korm.VolumeName}\t{korm.VolumeCount} кг", cancellationToken);
            var feed = new FeedAnimalModel();
            feed.date = DateTime.Now.ToString("g");
            feed.FIO = p.FIO;
            feed.Photo = fileId;
            feed.NameKorm = korm.VolumeName;
            feed.CountKorm = korm.VolumeCount.ToString();
            logFeed.Add(feed);
            SerialazeFeedAnimal();
            string msg = $"Волонтер {p.FIO} накормил кормом {korm.VolumeName} {korm.VolumeCount} кг  ";
            TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
        }
        public async Task ViewStatisticAdmin(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {


                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    Workbook workbook = new Workbook();
                    Worksheet worksheet = workbook.Worksheets[0];

                    worksheet.Cells[1, 0].PutValue("ФИО");
                    worksheet.Cells[1, 1].PutValue("Почта");
                    worksheet.Cells[1, 2].PutValue("Телефон");
                    worksheet.Cells[1, 3].PutValue("NikTelegram");
                    worksheet.Cells[1, 4].PutValue("Фото");
                    worksheet.Cells[1, 5].PutValue("IdTelegram");

                    for (int i = 0, r = 2; i < volonters.Count; i++, r++)
                    {
                        var volonter = volonters[i];
                        worksheet.Cells[r + 1, 0].PutValue(volonter.FIO);
                        worksheet.Cells[r + 1, 1].PutValue(volonter.Email);
                        worksheet.Cells[r + 1, 2].PutValue(volonter.Phone);
                        worksheet.Cells[r + 1, 3].PutValue(volonter.NikTelegram);
                        worksheet.Cells[r + 1, 4].PutValue(volonter.Photo);
                        worksheet.Cells[r + 1, 5].PutValue(volonter.IdTelegram);
                        var path = await DownloadPhotoAsync(botClient, volonter.Photo, "Photo");
                        if (System.IO.File.Exists(path))
                        {
                            int pictureIndex = worksheet.Pictures.Add(r + 1, 4, path);
                            Aspose.Cells.Drawing.Picture picture = worksheet.Pictures[pictureIndex];
                            picture.Width = 100;
                            picture.Height = 100;
                        }
                    }
                    await botClient.SendChatActionAsync(update.Message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.UploadDocument);
                    workbook.Save($"VolontersExport\\VolontersExp{update.Message.Chat.Id}.xlsx", SaveFormat.Xlsx);
                    using (var fileStream = new FileStream($"VolontersExport\\VolontersExp{update.Message.Chat.Id}.xlsx", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputFileStream(fileStream, Path.GetFileName($"VolontersExp{update.Message.Chat.Id}.xlsx"));
                        await botClient.SendDocumentAsync(update.Message.Chat.Id, inputOnlineFile);
                    }
                    PathFIles.Add($"VolontersExport\\VolontersExp{update.Message.Chat.Id}.xlsx");
                    Workbook workbook1 = new Workbook();
                    Worksheet worksheet1 = workbook1.Worksheets[0];

                    worksheet1.Cells[1, 0].PutValue("название точки");
                    worksheet1.Cells[1, 1].PutValue("Адрес");
                    worksheet1.Cells[1, 2].PutValue("Корм");
                    for (int i = 0, r = 2; i < points.Count; i++, r++)
                    {
                        var point = points[i];
                        worksheet1.Cells[r + 1, 0].PutValue(point.Name);
                        worksheet1.Cells[r + 1, 1].PutValue(point.Adres);
                        string korm = "";
                        for (int j = 0; j < point.VolumeKorm.Count; j++)
                        {
                            korm += $"{point.VolumeKorm[j].VolumeName} {point.VolumeKorm[j].VolumeCount} кг ||||";
                        }
                        worksheet1.Cells[r + 1, 2].PutValue(korm);

                    }
                    await botClient.SendChatActionAsync(update.Message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.UploadDocument);
                    workbook1.Save($"PointExport\\PointsExp{update.Message.Chat.Id}.xlsx", SaveFormat.Xlsx);
                    using (var fileStream = new FileStream($"PointExport\\PointsExp{update.Message.Chat.Id}.xlsx", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputFileStream(fileStream, Path.GetFileName($"PointsExp{update.Message.Chat.Id}.xlsx"));
                        await botClient.SendDocumentAsync(update.Message.Chat.Id, inputOnlineFile);
                    }
                    PathFIles.Add($"PointExport\\PointsExp{update.Message.Chat.Id}.xlsx");





                    Workbook workbook2 = new Workbook();
                    Worksheet worksheet2 = workbook2.Worksheets[0];

                    worksheet2.Cells[1, 0].PutValue("Кто");
                    worksheet2.Cells[1, 1].PutValue("Кому");
                    worksheet2.Cells[1, 2].PutValue("Корм");
                    worksheet2.Cells[1, 3].PutValue("Сколько");
                    worksheet2.Cells[1, 4].PutValue("Фото");
                    for (int i = 0, r = 2; i < peredachaKorm.Count; i++, r++)
                    {
                        var point = peredachaKorm[i];
                        worksheet2.Cells[r + 1, 0].PutValue(point.From);
                        worksheet2.Cells[r + 1, 1].PutValue(point.To);
                        worksheet2.Cells[r + 1, 2].PutValue(point.NameKorm);
                        worksheet2.Cells[r + 1, 3].PutValue(point.CountKorm + " кг");
                        var path = await DownloadPhotoAsync(botClient, point.Photo, "Photo");
                        if (System.IO.File.Exists(path))
                        {
                            int pictureIndex = worksheet2.Pictures.Add(r + 1, 4, path);
                            Aspose.Cells.Drawing.Picture picture = worksheet2.Pictures[pictureIndex];
                            picture.Width = 100;
                            picture.Height = 100;
                        }
                    }


                    await botClient.SendChatActionAsync(update.Message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.UploadDocument);
                    workbook2.Save($"PeredachaExport\\Peredacha{update.Message.Chat.Id}.xlsx", SaveFormat.Xlsx);
                    using (var fileStream = new FileStream($"PeredachaExport\\Peredacha{update.Message.Chat.Id}.xlsx", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputFileStream(fileStream, Path.GetFileName($"Peredacha{update.Message.Chat.Id}.xlsx"));
                        await botClient.SendDocumentAsync(update.Message.Chat.Id, inputOnlineFile);
                    }
                    PathFIles.Add($"PeredachaExport\\Peredacha{update.Message.Chat.Id}.xlsx");






                    Workbook workbook3 = new Workbook();
                    Worksheet worksheet3 = workbook3.Worksheets[0];

                    worksheet3.Cells[1, 0].PutValue("Точка сбора");
                    worksheet3.Cells[1, 1].PutValue("ФИО, кто взял");
                    worksheet3.Cells[1, 2].PutValue("Корм");
                    worksheet3.Cells[1, 3].PutValue("Сколько");
                    worksheet3.Cells[1, 4].PutValue("Фото");
                    worksheet3.Cells[1, 5].PutValue("Дата");
                    for (int i = 0, r = 2; i < LogsPoint.Count; i++, r++)
                    {
                        var point = LogsPoint[i];
                        worksheet3.Cells[r + 1, 0].PutValue(point.Point);
                        worksheet3.Cells[r + 1, 1].PutValue(point.FIO);
                        worksheet3.Cells[r + 1, 2].PutValue(point.NameKorm);
                        worksheet3.Cells[r + 1, 3].PutValue(point.CountKorm + " кг");
                        var path = await DownloadPhotoAsync(botClient, point.Photo, "Photo");
                        if (System.IO.File.Exists(path))
                        {
                            int pictureIndex = worksheet3.Pictures.Add(r + 1, 4, path);
                            Aspose.Cells.Drawing.Picture picture = worksheet3.Pictures[pictureIndex];
                            picture.Width = 100;
                            picture.Height = 100;
                        }
                        worksheet3.Cells[r + 1, 5].PutValue(point.date);
                    }


                    await botClient.SendChatActionAsync(update.Message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.UploadDocument);
                    workbook3.Save($"LogPointExport\\LogPoints{update.Message.Chat.Id}.xlsx", SaveFormat.Xlsx);
                    using (var fileStream = new FileStream($"LogPointExport\\LogPoints{update.Message.Chat.Id}.xlsx", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputFileStream(fileStream, Path.GetFileName($"LogPoints{update.Message.Chat.Id}.xlsx"));
                        await botClient.SendDocumentAsync(update.Message.Chat.Id, inputOnlineFile);
                    }
                    PathFIles.Add($"LogPointExport\\LogPoints{update.Message.Chat.Id}.xlsx");





                    Workbook workbook4 = new Workbook();
                    Worksheet worksheet4 = workbook4.Worksheets[0];

                    worksheet4.Cells[1, 0].PutValue("ФИО");
                    worksheet4.Cells[1, 1].PutValue("Корм");
                    worksheet4.Cells[1, 2].PutValue("Сколько");
                    worksheet4.Cells[1, 3].PutValue("Фото");
                    worksheet4.Cells[1, 4].PutValue("Дата");
                    for (int i = 0, r = 2; i < logFeed.Count; i++, r++)
                    {
                        var point = logFeed[i];
                        worksheet4.Cells[r + 1, 0].PutValue(point.FIO);
                        worksheet4.Cells[r + 1, 1].PutValue(point.NameKorm);
                        worksheet4.Cells[r + 1, 2].PutValue(point.CountKorm + " кг");

                        var path = await DownloadPhotoAsync(botClient, point.Photo, "Photo");
                        if (System.IO.File.Exists(path))
                        {
                            int pictureIndex = worksheet4.Pictures.Add(r + 1, 3, path);
                            Aspose.Cells.Drawing.Picture picture = worksheet4.Pictures[pictureIndex];
                            picture.Width = 100;
                            picture.Height = 100;
                        }
                        worksheet4.Cells[r + 1, 4].PutValue(point.date);
                    }


                    await botClient.SendChatActionAsync(update.Message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.UploadDocument);
                    workbook4.Save($"LogFeed\\LogFeed{update.Message.Chat.Id}.xlsx", SaveFormat.Xlsx);
                    using (var fileStream = new FileStream($"LogFeed\\LogFeed{update.Message.Chat.Id}.xlsx", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var inputOnlineFile = new InputFileStream(fileStream, Path.GetFileName($"LogLogFeed{update.Message.Chat.Id}.xlsx"));
                        await botClient.SendDocumentAsync(update.Message.Chat.Id, inputOnlineFile);
                    }
                    PathFIles.Add($"LogFeed\\LogFeed{update.Message.Chat.Id}.xlsx");
                    for (int i = 0; i < PathFIles.Count; i++)
                    {
                        System.IO.File.Delete(PathFIles[i]);
                    }
                    PathFIles.Clear();
                }
                else
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
;
        }
        public async Task AuthAdmin(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //авторизация админа
        {
            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите логин", cancellationToken);
            botClient.RegisterNextStep(update.Message.Chat.Id, Login);
        }
        public async Task Login(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//ввод логина
        {
            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите пароль", cancellationToken);

            botClient.GetCacheData(update.GetChatId()).Value.Login = update.Message.Text;
            botClient.RegisterNextStep(update.Message.Chat.Id, LoginPasswordHandler);
        }

        public async Task LoginPasswordHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//ввод пароля и авторизация
        {
            botClient.ClearStepUser(update.Message.Chat.Id);
            botClient.GetCacheData(update.GetChatId()).Value.Password = update.Message.Text;

            KeyValuePair<long, UserCache> loginpas = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);

            if (loginpas.Value.Password == Password && loginpas.Value.Login == Login1)
            {
                botClient.GetCacheData(update.GetChatId()).Value.IsAdmin = true;
            }

            OptionTelegramMessage message = new();
            List<string> ids = new() { CommandRoute.AddVolonter, CommandRoute.AddPointCollection, CommandRoute.ViewVolonters, CommandRoute.ViewPoinCollection, CommandRoute.SearchPoint, CommandRoute.SearchVolonter, CommandRoute.ViewStatisticAdmin, CommandRoute.MENU };
            message.ClearMenu = false;
            message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");

            await botClient.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Вы успешно авторизованы", cancellationToken, message);
            adminIds.Add(update.Message.Chat.Id.ToString());
            System.IO.File.WriteAllLines("adminIds.txt", adminIds);
        }
        public async Task ViewSelfCard(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
                var p = volonters.FirstOrDefault(p => p.FIO == q);
                int index = volonters.IndexOf(p);
                string msg = "";
                for (int j = 0; j < volonters[index].VolumeKorm.Count; j++)
                {
                    msg += $"{volonters[index].VolumeKorm[j].VolumeName}\n{volonters[index].VolumeKorm[j].VolumeCount} кг\n------\n";
                }

                await route.MessageCommand.SendPhoto(botClient, update.Message.Chat.Id, $"Ваша карточка:\n🧑 {botClient.GetCacheData(update.GetChatId()).Value.FIO}\n📧 {botClient.GetCacheData(update.GetChatId()).Value.Email}\n📱 {botClient.GetCacheData(update.GetChatId()).Value.Phone}\nУ вас на руках корма:\n\n{msg}", botClient.GetCacheData(update.GetChatId()).Value.FileIdPasport, cancellationToken);
                botClient.ClearStepUser(update.Message.Chat.Id);
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }

        public async Task Menu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //шаг меню
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == false)
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { CommandRoute.GiveKorm, CommandRoute.GetKorm, CommandRoute.ViewSelfCard, CommandRoute.FeedAnimal, CommandRoute.SearchPoint, CommandRoute.SearchVolonter }; //TODO поиск при забрать 
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");

                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Вы вышли в меню", cancellationToken, message);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { CommandRoute.AddVolonter, CommandRoute.AddPointCollection, CommandRoute.ViewVolonters, CommandRoute.ViewPoinCollection, CommandRoute.SearchPoint, CommandRoute.SearchVolonter, CommandRoute.ViewStatisticAdmin, CommandRoute.MENU };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Вы вышли в меню", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }

        public async Task AddVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//добавление волонтера
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите ФИО волонтера", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, AddFIOVolonter);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddFIOVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) // добавление фио волонтера
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    botClient.GetCacheData(update.GetChatId()).Value.FIO = update.Message.Text;
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите почту волонтера", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, AddEmailVolonter);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddEmailVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //добавление почты волонтера
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    botClient.GetCacheData(update.GetChatId()).Value.Email = update.Message.Text;
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите телефон волонтера", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, AddPhoneVolonter);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddPhoneVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) // добавление телефона волонтера
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    botClient.GetCacheData(update.GetChatId()).Value.Phone = update.Message.Text;
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Отправьте фото паспорта волонтера", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, AddPasportVolonter);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddPasportVolonter(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //добавление фото паспорта волонтера
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    if (update.Message.Photo != null && update.Message.Photo.Length > 0)
                    {
                        KeyValuePair<long, UserCache> loginpas = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);

                        var fileId = update.Message.Photo.Last().FileId; // Берем последнее фото в массиве (обычно самое высокое качество)
                        var file = await botClient.GetFileAsync(fileId, cancellationToken);

                        var r = new VolonterModel();
                        r.FIO = loginpas.Value.FIO;
                        r.Phone = loginpas.Value.Phone;
                        r.Email = loginpas.Value.Email;
                        r.Photo = file.FileId;
                        r.VolumeKorm = new();

                        volonters.Add(r);
                        SerialazeVolonter();
                        var inputFile = new InputFileId(fileId);
                        await botClient.SendPhotoAsync(update.Message.Chat.Id, inputFile, caption: $"Карточка волонтера:\n🧑 {r.FIO}\n📧 {r.Email}\n📱 {r.Phone}");
                        botClient.ClearStepUser(update.Message.Chat.Id);
                        string msg = $"Добавлен волонтер {r.FIO}\t\t{r.Phone}";
                        TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Пожалуйста, отправьте фото паспорта волонтера.", cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = true;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }

            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddPointCollection(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                    TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Login);
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { CommandRoute.MENU };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите Название точки сбора", cancellationToken, message);
                    botClient.RegisterNextStep(update.Message.Chat.Id, AddNamePointCollection);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { CommandRoute.MENU };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddNamePointCollection(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                    TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Login);

                    botClient.GetCacheData(update.GetChatId()).Value.NamePountCollection = update.Message.Text;

                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите адрес точки сбора", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, AddAdresPointCollection);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task AddAdresPointCollection(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {


                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    botClient.GetCacheData(update.GetChatId()).Value.AdresPointCollection = update.Message.Text;
                    KeyValuePair<long, UserCache> loginpas = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);
                    var p = new PointCollectionModel();
                    p.Adres = update.Message.Text;
                    p.Name = loginpas.Value.NamePountCollection;
                    p.VolumeKorm = new();

                    var krom = new VolumeKormModel();
                    krom.VolumeName = "Для кошек";
                    krom.VolumeCount = 15;
                    krom.FileIdPhoto = "";
                    p.VolumeKorm.Add(krom);
                    var krom1 = new VolumeKormModel();
                    krom1.VolumeName = "Для собак";
                    krom1.VolumeCount = 25;
                    krom1.FileIdPhoto = "";
                    p.VolumeKorm.Add(krom1);

                    points.Add(p);
                    SerialazePoint();
                    botClient.ClearStepUser(update.Message.Chat.Id);
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { CommandRoute.AddVolonter, CommandRoute.AddPointCollection, CommandRoute.ViewVolonters, CommandRoute.ViewPoinCollection, CommandRoute.MENU };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Точка добавлена", cancellationToken, message);
                    string msg = $"Добавлена точка '{p.Name}'\t\t{p.Adres}";
                    TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }

        public async Task GetKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//Получить количество непрочитанных сообщений
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Выберите удобную вам точку", cancellationToken);
                for (int i = 0; i < points.Count; i++)
                {
                    OptionTelegramMessage message = new();
                    var ikm = new InlineKeyboardMarkup(new[]
                    {
                         new[]
                         {
                            InlineKeyboardButton.WithCallbackData(CommandRoute.SelectPoint),
                         }
                    });
                    message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);
                    string msg = "";
                    for (int j = 0; j < points[i].VolumeKorm.Count; j++)
                    {
                        if (points[i].VolumeKorm[j].VolumeCount > 0)
                        {
                            msg += $"{points[i].VolumeKorm[j].VolumeName}\n{points[i].VolumeKorm[j].VolumeCount} кг\n------\n";
                        }
                    }
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"{points[i].Name}\n{points[i].Adres}\n\nКорм в наличии:\n\n{msg}", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task SelectGetKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//Получить количество непрочитанных сообщений
        {
            try
            {
                var adresPoint = update.CallbackQuery.Message.Text.Split('\n')[1];
                var foundPoint = points.FirstOrDefault(p => p.Adres == adresPoint);

                botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint = points.IndexOf(foundPoint);

                string msg = "";
                for (int j = 0; j < points[points.IndexOf(foundPoint)].VolumeKorm.Count; j++)
                {
                    if (points[points.IndexOf(foundPoint)].VolumeKorm[j].VolumeCount > 0)
                    {
                        msg += $"{points[points.IndexOf(foundPoint)].VolumeKorm[j].VolumeName}\n{points[points.IndexOf(foundPoint)].VolumeKorm[j].VolumeCount} кг\n------\n";
                    }
                }
                await route.MessageCommand.Send(botClient, update.CallbackQuery.From.Id, $"Выбранная точка\n{points[points.IndexOf(foundPoint)].Name}\n{points[points.IndexOf(foundPoint)].Adres}\n\nКорм в наличии:\n{msg}", cancellationToken);

                await route.MessageCommand.Send(botClient, update.CallbackQuery.From.Id, $"Введите название корма", cancellationToken);
                botClient.RegisterNextStep(update.CallbackQuery.From.Id, SelectGetNameKorm);
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.CallbackQuery.From.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task SelectGetNameKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                bool success = false;
                for (int j = 0; j < points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm.Count; j++)
                {
                    if (update.Message.Text.ToLower() == points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm[j].VolumeName.ToLower())
                    {
                        botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm = update.Message.Text;
                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите кол-во корма (доступно {points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm[j].VolumeCount} кг)", cancellationToken);
                        success = true;
                        break;
                    }
                }

                if (!success)
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Нет корма с таким названием. Введите название корма снова", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, SelectGetNameKorm);
                }
                else
                {
                    botClient.RegisterNextStep(update.Message.Chat.Id, SelectGetCountKorm);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task SelectGetCountKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var foundPoint = points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm.FirstOrDefault(p => p.VolumeName == botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm);
                if (int.Parse(update.Message.Text) <= foundPoint.VolumeCount && int.Parse(update.Message.Text) > 0)
                {
                    botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm = int.Parse(update.Message.Text);
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Сфотографируйте и прикрепите фото", cancellationToken);
                    botClient.GetCacheData(update.GetChatId()).Value.StepPhoto = CommandRoute.SelectKormPhoto;
                    botClient.RegisterNextStep(update.Message.Chat.Id, SelectGetPhotoKorm);
                }
                else
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введенно не корректное значение\nВведите кол-во корма", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, SelectGetCountKorm);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task SelectGetPhotoKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var fileId = update.Message.Photo.Last().FileId;
                var korm = new VolumeKormModel();
                korm.FileIdPhoto = fileId;
                korm.VolumeName = botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm;
                korm.VolumeCount = botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm;

                points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].VolumeKorm.FirstOrDefault(p => p.VolumeName == botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm).VolumeCount -= korm.VolumeCount;
                var j = botClient.GetCacheData(update.GetChatId()).Value.FIO;
                var p = volonters.FirstOrDefault(p => p.FIO == j);
                bool finded = false;
                for (int i = 0; i < p.VolumeKorm.Count; i++)
                {
                    if (p.VolumeKorm[i].VolumeName == korm.VolumeName)
                    {
                        p.VolumeKorm[i].VolumeCount += korm.VolumeCount;
                        finded = true;
                    }
                }

                if (!finded)
                {
                    p.VolumeKorm.Add(korm);
                }
                volonters[volonters.IndexOf(p)] = p;
                botClient.ClearStepUser(update.Message.Chat.Id);
                SerialazeVolonter();
                SerialazePoint();
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы взяли корм {korm.VolumeName}\t{korm.VolumeCount} кг", cancellationToken);

                var logPoint = new LogPointCollectionModel();
                logPoint.FIO = p.FIO;
                logPoint.Photo = korm.FileIdPhoto;
                logPoint.NameKorm = korm.VolumeName;
                logPoint.CountKorm = korm.VolumeCount.ToString();
                logPoint.date = DateTime.Now.ToString("g");
                logPoint.Point = points[botClient.GetCacheData(update.GetChatId()).Value.SelectedPoint].Name;
                LogsPoint.Add(logPoint);
                SerialazeLogPoint();

                string msg = $"Волонтер {p.FIO} взял {korm.VolumeCount} кг корма для {korm.VolumeName}";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task GiveKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Введите ФИО волонтера кому хотите передать корм", cancellationToken);
            botClient.RegisterNextStep(update.Message.Chat.Id, FindVolonterGiveKorm);
        }
        public async Task FindVolonterGiveKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                bool succes = false;
                for (int i = 0; i < volonters.Count; i++)
                {
                    if (volonters[i].FIO.ToLower() == update.Message.Text.ToLower())
                    {
                        OptionTelegramMessage message = new();
                        var ikm = new InlineKeyboardMarkup(new[]
                        {
                         new[]
                         {
                            InlineKeyboardButton.WithCallbackData(CommandRoute.SelectVolonterGive)
                         }
                    });
                        message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);

                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, msg: $"Карточка волонтера:\n🧑 {volonters[i].FIO}\n📧 {volonters[i].Email}\n📱 {volonters[i].Phone}", cancellationToken, message);

                        succes = true;
                    }
                }

                if (!succes)
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, msg: "Волонтер не найден. Введите ФИО снова", cancellationToken);
                    botClient.RegisterNextStep(update.CallbackQuery.From.Id, FindVolonterGiveKorm);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.CallbackQuery.From.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task NameKormVolonterGiveKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                botClient.GetCacheData(update.GetChatId()).Value.SelectedVolonterGive = update.CallbackQuery.Message.Text.Split('\n')[1];
                var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
                var p = volonters.FirstOrDefault(p => p.FIO == q);
                int index = volonters.IndexOf(p);
                string msg = "";
                for (int j = 0; j < volonters[index].VolumeKorm.Count; j++)
                {
                    msg += $"{volonters[index].VolumeKorm[j].VolumeName}\n{volonters[index].VolumeKorm[j].VolumeCount} кг\n------\n";
                }

                await route.MessageCommand.Send(botClient, update.CallbackQuery.From.Id, $"Введите названия корма для передачи:\n\n{msg}", cancellationToken);
                botClient.RegisterNextStep(update.CallbackQuery.From.Id, GetNameKormVolonterGiveKorm);
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task GetNameKormVolonterGiveKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                bool succes = false;
                var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
                var p = volonters.FirstOrDefault(p => p.FIO == q);
                int index = volonters.IndexOf(p);

                for (int j = 0; j < volonters[index].VolumeKorm.Count; j++)
                {
                    if (update.Message.Text.ToLower() == volonters[index].VolumeKorm[j].VolumeName.ToLower())
                    {
                        botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm = update.Message.Text;
                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите кол-во корма (доступно {volonters[index].VolumeKorm[j].VolumeCount} кг)", cancellationToken);
                        botClient.RegisterNextStep(update.Message.Chat.Id, CountKormVolonterGiveKorm);
                        succes = true;
                        break;
                    }
                }

                if (!succes)
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Нет корма с таким названием.\nВведите название корма снова", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, GetNameKormVolonterGiveKorm);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task CountKormVolonterGiveKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
                var p = volonters.FirstOrDefault(p => p.FIO == q);
                int index = volonters.IndexOf(p);
                var foundVolonter = volonters[index].VolumeKorm.FirstOrDefault(p => p.VolumeName == botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm);

                if (int.Parse(update.Message.Text) <= foundVolonter.VolumeCount && int.Parse(update.Message.Text) > 0)
                {
                    botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm = int.Parse(update.Message.Text);
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Сфотографируйте и прикрепите фото", cancellationToken);
                    botClient.GetCacheData(update.GetChatId()).Value.StepPhoto = CommandRoute.GiveKormVolonter;
                    botClient.RegisterNextStep(update.Message.Chat.Id, PhotoKormVolonterGiveKorm);
                }
                else
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введенно не корректное значение\nВведите кол-во корма снова", cancellationToken);
                    botClient.RegisterNextStep(update.Message.Chat.Id, CountKormVolonterGiveKorm);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task PhotoKormVolonterGiveKorm(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var fileId = update.Message.Photo.Last().FileId;
                var korm = new VolumeKormModel();
                korm.FileIdPhoto = fileId;
                korm.VolumeName = botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm;
                korm.VolumeCount = botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm;

                var q = botClient.GetCacheData(update.GetChatId()).Value.FIO;
                var p = volonters.FirstOrDefault(p => p.FIO == q);
                int index = volonters.IndexOf(p);

                volonters[index].VolumeKorm.FirstOrDefault(p => p.VolumeName == botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm).VolumeCount -= korm.VolumeCount;
                var sss = botClient.GetCacheData(update.GetChatId()).Value.SelectedVolonterGive.Split(' ')[1] + " " + botClient.GetCacheData(update.GetChatId()).Value.SelectedVolonterGive.Split(' ')[2] + " " + botClient.GetCacheData(update.GetChatId()).Value.SelectedVolonterGive.Split(' ')[3];
                var FoundVolonter = volonters.FirstOrDefault(FoundVolonter => FoundVolonter.FIO == sss);
                int indexFound = volonters.IndexOf(p);

                bool finded = false;
                for (int i = 0; i < FoundVolonter.VolumeKorm.Count; i++)
                {
                    if (FoundVolonter.VolumeKorm[i].VolumeName == korm.VolumeName)
                    {
                        FoundVolonter.VolumeKorm[i].VolumeCount += korm.VolumeCount;
                        finded = true;
                    }
                }

                if (!finded)
                {
                    FoundVolonter.VolumeKorm.Add(korm);
                }
                volonters[volonters.IndexOf(FoundVolonter)] = FoundVolonter; //кому передаем

                botClient.ClearStepUser(update.Message.Chat.Id);
                SerialazeVolonter();
                var perKorm = new PeredachaKormaModel();
                perKorm.From = volonters[index].FIO; perKorm.To = FoundVolonter.FIO;
                perKorm.Photo = fileId; perKorm.NameKorm = botClient.GetCacheData(update.GetChatId()).Value.SelectedNameKorm; perKorm.CountKorm = botClient.GetCacheData(update.GetChatId()).Value.SelectedCountKorm.ToString();
                peredachaKorm.Add(perKorm);
                SerialazePeredacha();

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы передали корм {korm.VolumeName}\t{korm.VolumeCount} кг волонтеру {FoundVolonter.FIO}", cancellationToken);

                string msg = $"Волонтер {p.FIO} взял {korm.VolumeCount} кг корма для {korm.VolumeName}";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task ViewPointCollection(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        OptionTelegramMessage message = new();
                        var ikm = new InlineKeyboardMarkup(new[]
                        {
                             new[]
                             {
                                InlineKeyboardButton.WithCallbackData("Редактировать"),
                                InlineKeyboardButton.WithCallbackData(CommandRoute.AddKormPoint),
                             
                             },
                             new[]
                             {
                                
                                InlineKeyboardButton.WithCallbackData("Удалить")
                             }
                        });
                        message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);
                        string msg = "";
                        for (int j = 0; j < points[i].VolumeKorm.Count; j++)
                        {
                            msg += $"{points[i].VolumeKorm[j].VolumeName}\n{points[i].VolumeKorm[j].VolumeCount} кг\n------\n";
                        }
                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"{points[i].Name}\n{points[i].Adres}\n\nКорм в наличии:\n{msg}", cancellationToken, message);
                    }
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public async Task ViewVolonters(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (botClient.GetCacheData(update.GetChatId()).Value.IsAdmin == true)
                {
                    for (int i = 0; i < volonters.Count; i++)
                    {
                        OptionTelegramMessage message = new();
                        var ikm = new InlineKeyboardMarkup(new[]
                        {
                         new[]
                         {
                           InlineKeyboardButton.WithCallbackData("Редактировать"),
                            InlineKeyboardButton.WithCallbackData("Удалить")
                         }
                    });
                        message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);

                        await route.MessageCommand.SendPhoto(botClient, update.Message.Chat.Id, caption: $"Карточка волонтера:\n🧑 {volonters[i].FIO}\n📧 {volonters[i].Email}\n📱 {volonters[i].Phone}", volonters[i].Photo, cancellationToken, message);
                    }
                    if (volonters.Count == 0)
                    {
                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Волонтеры не найдены", cancellationToken);
                    }
                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { };
                    message.ClearMenu = false;
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "У вас нет прав на это действие", cancellationToken, message);
                }
            }
            catch
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Что-то пошло не так(", cancellationToken);
            }
        }
        public void SerialazeVolonter()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<VolonterModel>));
                using (TextWriter writer = new StreamWriter("Volonter.txt"))
                {
                    serializer.Serialize(writer, volonters);
                }
            }
            catch
            {

            }
        }
        public void SerialazeFeedAnimal()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<FeedAnimalModel>));
                using (TextWriter writer = new StreamWriter("FeedAnimal.txt"))
                {
                    serializer.Serialize(writer, logFeed);
                }
            }
            catch
            {

            }
        }
        public void SerialazePoint()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<PointCollectionModel>));
                using (TextWriter writer = new StreamWriter("Point.txt"))
                {
                    serializer.Serialize(writer, points);
                }
            }
            catch
            {

            }
        }
        public void SerialazePeredacha()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<PeredachaKormaModel>));
                using (TextWriter writer = new StreamWriter("Peredacha.txt"))
                {
                    serializer.Serialize(writer, peredachaKorm);
                }
            }
            catch
            {

            }
        }
        public void SerialazeLogPoint()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<LogPointCollectionModel>));
                using (TextWriter writer = new StreamWriter("LogPoint.txt"))
                {
                    serializer.Serialize(writer, LogsPoint);
                }
            }
            catch
            {

            }
        }

        public static async Task<string> DownloadPhotoAsync(ITelegramBotClient botClient, string fileId, string downloadPath)
        {
            try
            {
                var file = await botClient.GetFileAsync(fileId);
                var filePath = Path.Combine(downloadPath, file.FilePath.Split('/').Last());

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await botClient.DownloadFileAsync(file.FilePath, fileStream);
                }
                PathFIles.Add(filePath);
                return filePath;
            }
            catch
            {
                return "";
            }
        }
    }
}