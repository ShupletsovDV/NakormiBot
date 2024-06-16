using NLog;
using RestSharp;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NakormiBot.Service;
using NakormiBot.Service.Commands;
using NakormiBot.Infrastructure.Commands;
using NakormiBot.ViewModels.Base;
using Telegram.Bot.Types;
using System.IO;
using System.Xml.Serialization;
using NakormiBot.Model;

namespace NakormiBot.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();


        #region Свойства

        #region Статус состояния

        private string _Status;
        /// <summary>
        /// состояние программы
        /// </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }
        #endregion

        #region  Свойство очистки консоли
        private bool _attachedPropertyClear;
        public bool AttachedPropertyClear
        {
            get { return _attachedPropertyClear; }
            set { _attachedPropertyClear = value; OnPropertyChanged(); }
        }
        #endregion

        #region  Свойство консоли
        private string _attachedPropertyAppend;
        public string AttachedPropertyAppend
        {
            get { return _attachedPropertyAppend; }
            set { _attachedPropertyAppend = value; OnPropertyChanged(); }
        }
        #endregion

        #region Видимость консоли
        private string _visibleConsol;
        /// <summary>
        /// свойство консоли
        /// </summary>
        public string VisibleConsol
        {
            get => _visibleConsol;
            set => Set(ref _visibleConsol, value);
        }
        #endregion

        #region  Свойство очистки консоли ошибок
        private bool _attachedPropertyClearError;
        public bool AttachedPropertyClearError
        {
            get { return _attachedPropertyClearError; }
            set { _attachedPropertyClearError = value; OnPropertyChanged(); }
        }
        #endregion

        #region  Свойство консоли ошибок
        private string _attachedPropertyAppendError;
        public string AttachedPropertyAppendError
        {
            get { return _attachedPropertyAppendError; }
            set { _attachedPropertyAppendError = value; OnPropertyChanged(); }
        }
        #endregion

        #region Видимость настроек
        private string _visibleSettings;
        /// <summary>
        /// свойство консоли
        /// </summary>
        public string VisibleSettings
        {
            get => _visibleSettings;
            set => Set(ref _visibleSettings, value);
        }
        #endregion

        #region Токен бота

        private string _tokenBot;
        /// <summary>
        ///токен бота
        /// </summary>
        public string TokenBot
        {
            get => _tokenBot;
            set => Set(ref _tokenBot, value);
        }
        #endregion

        #region Видимость консоли ошибок
        private string _visibleError = "Hidden";
        /// <summary>
        /// свойство консоли ошибок
        /// </summary>
        public string VisibleError
        {
            get => _visibleError;
            set => Set(ref _visibleError, value);
        }
        #endregion

        #region Главная кнопка
        private bool _IsDefaultMain;
        /// <summary>
        /// свойство главной кнопке
        /// </summary>
        public bool IsDefaultMain
        {
            get => _IsDefaultMain;
            set => Set(ref _IsDefaultMain, value);
        }
        #endregion

        #region Кнопка настроек
        private bool _IsDefaultSetting;
        /// <summary>
        /// свойство  кнопки настроек
        /// </summary>
        public bool IsDefaultSetting
        {
            get => _IsDefaultSetting;
            set => Set(ref _IsDefaultSetting, value);
        }
        #endregion

        #region Кнопка неполадок
        private bool _IsDefaultError;
        /// <summary>
        /// свойство кнопки неполадок
        /// </summary>
        public bool IsDefaultError
        {
            get => _IsDefaultError;
            set => Set(ref _IsDefaultError, value);
        }
        #endregion

        #region кнопка свернуть

        private WindowState _WindowState;

        public WindowState WindowState
        {
            get => _WindowState;
            set => Set(ref _WindowState, value);
        }
        #endregion

        #region цвет неполадок
        private string _colorError = "Hidden";
        /// <summary>
        /// свойство кнопки ошибок
        /// </summary>
        public string ColorError
        {
            get => _colorError;
            set => Set(ref _colorError, value);
        }
        #endregion

        #region Видимость кнопки запустить

        private string _visibleStartBtn = "Hidden";
        /// <summary>
        /// логин
        /// </summary>
        public string VisibleStartBtn
        {
            get => _visibleStartBtn;
            set => Set(ref _visibleStartBtn, value);
        }
        #endregion

        #endregion

        #region Команды

        #region Команда главной кнопки
        public ICommand MainBtnCommand { get; set; }
        private void OnMainBtnCommandExecuted(object p)
        {
            IsDefaultMain = true;
            IsDefaultSetting = false;
            IsDefaultError = false;
            VisibleConsol = "Visible";
            VisibleError = "Hidden";
            VisibleSettings = "Hidden";


        }
        private bool CanMainBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки настроек
        public ICommand SettingBtnCommand { get; set; }
        private void OnSettingBtnCommandExecuted(object p)
        {
            IsDefaultMain = false;
            IsDefaultError = false;
            IsDefaultSetting = true;

            VisibleConsol = "Hidden";
            VisibleError = "Hidden";
            VisibleSettings = "Visible";

            //SettingPageViewModel.getSettingPage().ShowDialog();
        }
        private bool CanSettingBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки ошибок
        public ICommand ErrorBtnCommand { get; set; }
        private void OnErrorBtnCommandExecuted(object p)
        {
            IsDefaultMain = false;
            IsDefaultSetting = false;
            IsDefaultError = true;
            VisibleConsol = "Hidden";
            VisibleError = "Visible";
            ColorError = "Hidden";
            VisibleSettings = "Hidden";
        }
        private bool CanErrorBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки выхода
        public ICommand CloseAppCommand { get; set; }
        private void OnCloseAppCommandExecuted(object p)
        {
            object locker = new();
            lock (locker)
            {
                Environment.Exit(0);

            }
        }
        private bool CanCloseAppCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки запустить
        public ICommand StartBtnCommand { get; set; }
        private void OnStartBtnCommandExecuted(object p)
        {
            
            VisibleStartBtn = "Hidden";
        }
        private bool CanStartBtnCommandExecute(object p) => true;
        #endregion

        #region Команда кнопки остановить
        public ICommand StopBtnCommand { get; set; }
        private void OnStopBtnCommandExecuted(object p)
        {
           
           


        }
        private bool CanStopBtnCommandExecute(object p) => true;
        #endregion

        #region команда кнопки свернуть

        public ICommand RollUpCommand { get; set; }


        private void OnRollUpCommandExecuted(object p)
        {

            WindowState = WindowState.Minimized;


        }
        private bool CanRollUpCommandExecute(object p) => true;

        #endregion

        #region Команда кнопки сохранения 
        public ICommand SaveSettingsCommand { get; set; }
        private void OnSaveSettingsCommandExecuted(object p)
        {
            TelegramCore.TelegramToken = TokenBot;
            System.IO.File.WriteAllText("botToken.txt", TokenBot);

        }
        private bool CanSaveSettingsCommandExecute(object p) => true;
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            try
            {
          
                WindowState = WindowState.Normal;
                IsDefaultMain = true;
                VisibleSettings = "Hidden";

                #region Команды
                MainBtnCommand = new LambdaCommand(OnMainBtnCommandExecuted, CanMainBtnCommandExecute);
                SettingBtnCommand = new LambdaCommand(OnSettingBtnCommandExecuted, CanSettingBtnCommandExecute);
                ErrorBtnCommand = new LambdaCommand(OnErrorBtnCommandExecuted, CanErrorBtnCommandExecute);
                CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted, CanCloseAppCommandExecute);
                RollUpCommand = new LambdaCommand(OnRollUpCommandExecuted, CanRollUpCommandExecute);
                StartBtnCommand = new LambdaCommand(OnStartBtnCommandExecuted, CanStartBtnCommandExecute);
                StopBtnCommand = new LambdaCommand(OnStopBtnCommandExecuted, CanStopBtnCommandExecute);
                SaveSettingsCommand = new LambdaCommand(OnSaveSettingsCommandExecuted, CanSaveSettingsCommandExecute);

                #endregion

                #region загрузка данных из конфига

                #endregion

                TokenBot= System.IO.File.ReadAllText("botToken.txt");
                TelegramCore.TelegramToken = TokenBot;

                AttachedPropertyAppend = "Здесь будут отображаться сообщения из телеграма\n" + Environment.NewLine;
                AttachedPropertyAppendError = "Здесь будут отображаться неполадки в работе программы\n" + Environment.NewLine;
                LoadInformation();
                Common.adminIds = new List<string>(System.IO.File.ReadAllLines("adminIds.txt"));
                for(int i = 0; i < Common.adminIds.Count; i++)
                {
                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(Common.adminIds[i])).Value.IsAdmin=true;
                }
                StartTelegram();
              


            }
            catch (Exception ex)
            {
                Log.Error("Ошибка конструктора MainWindowViewModel | " + ex);
            }



        }


        private void StartTelegram()
        {
            var telegram = TelegramCore.getTelegramCore();
            telegram.Start();
            telegram.OnCommonLog += Telegram_OnCommonLog;
            telegram.OnCommonError += Telegram_OnCommonError;
            telegram.OnCommonStatus += Telegram_OnCommonStatus;
            
            //telegram.OnColorError += Telegram_OnColorError;
        }


        private void Telegram_OnCommonStatus(string message, TelegramCore.TelegramEvents events)
        {
            if (message == "Обработка сообщений остановлена" || message.ToLower().Contains("бот остановлен"))
            {
                VisibleStartBtn = "Visible";
                Status = $"{DateTime.Now.ToString("g")} - {message}";
            }
            else
            {
                VisibleStartBtn = "Hidden";
                Status = $"{DateTime.Now.ToString("g")} - {message}";
            }

        }

        private void Telegram_OnCommonError(string message, TelegramCore.TelegramEvents events)
        {
            AttachedPropertyAppendError = $"{DateTime.Now.ToString("G")}: {message} \n" + Environment.NewLine;
            if(IsDefaultError==false)
            {
                ColorError = "Visible";
            }
            
        }

        private void Telegram_OnCommonLog(string message, TelegramCore.TelegramEvents events)
        {

            AttachedPropertyAppend = $"{DateTime.Now.ToString("G")}: {message} \n" + Environment.NewLine;

        }

        private async void LoadInformation()
        {
            Common.volonters = DeserializeExperts2("Volonter.txt");
            Common.points = DeserializeExperts3("Point.txt");
            Common.peredachaKorm = DeSerialazePeredacha("Peredacha.txt");
            Common.logFeed = DeSerialazeFeed("FeedAnimal.txt");
            Common.LogsPoint = DeserializeLogPoint("LogPoint.txt");

            for(int i = 0;i<Common.volonters.Count;i++)
            {
                TelegramCore.getTelegramCore().bot.GetCacheData(Common.volonters[i].IdTelegram).Value.IdTelegram = Common.volonters[i].IdTelegram;
                TelegramCore.getTelegramCore().bot.GetCacheData(Common.volonters[i].IdTelegram).Value.FIO = Common.volonters[i].FIO;
                TelegramCore.getTelegramCore().bot.GetCacheData(Common.volonters[i].IdTelegram).Value.Email = Common.volonters[i].Email;
                TelegramCore.getTelegramCore().bot.GetCacheData(Common.volonters[i].IdTelegram).Value.Phone = Common.volonters[i].Phone;
                TelegramCore.getTelegramCore().bot.GetCacheData(Common.volonters[i].IdTelegram).Value.FileIdPasport = Common.volonters[i].Photo;
            }
        }
        public static List<PeredachaKormaModel> DeSerialazePeredacha(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PeredachaKormaModel>));
            using (TextReader reader = new StreamReader(filename))
            {
                return (List<PeredachaKormaModel>)serializer.Deserialize(reader);
            }
        }
        public static List<FeedAnimalModel> DeSerialazeFeed(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<FeedAnimalModel>));
            using (TextReader reader = new StreamReader(filename))
            {
                return (List<FeedAnimalModel>)serializer.Deserialize(reader);
            }
        }
        public static List<VolonterModel> DeserializeExperts2(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<VolonterModel>));
            using (TextReader reader = new StreamReader(filename))
            {
                return (List<VolonterModel>)serializer.Deserialize(reader);
            }
        }
        public static List<PointCollectionModel> DeserializeExperts3(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PointCollectionModel>));
            using (TextReader reader = new StreamReader(filename))
            {
                return (List<PointCollectionModel>)serializer.Deserialize(reader);
            }
        }
        public static List<LogPointCollectionModel> DeserializeLogPoint(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<LogPointCollectionModel>));
            using (TextReader reader = new StreamReader(filename))
            {
                return (List<LogPointCollectionModel>)serializer.Deserialize(reader);
            }
        }


    }
}
