using RestSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telegram.Bot;
using NakormiBot.Infrastructure.Commands;
using NakormiBot.Service;
using NakormiBot.View.Windows;
using NakormiBot.ViewModels.Base;

using DataFormat = RestSharp.DataFormat;
using System.IO;
using System.Configuration;
using NakormiBot.Service.Commands;
using System.Net.Http;

namespace NakormiBot.ViewModels
{
    internal class SettingPageViewModel : ViewModel
    {
        private static RestClient RestClient { get; set; }
        private static readonly HttpClient client = new HttpClient();
        private static string authToken;
        private static string sessionToken;
         public static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


        
        
        #region Свойства

        #region чекбокс проверки кавычек
        public bool IsPass { get; set; }

        #endregion

        #region Токен Elma

        private string _tokenElma;
        /// <summary>
        /// токен Elma
        /// </summary>
        public string TokenElma
        {
            get => _tokenElma;
            set => Set(ref _tokenElma, value);
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

        #region Адрес

        private string _adress;
        /// <summary>
        /// адресс
        /// </summary>
        public string Adress
        {
            get => _adress;
            set => Set(ref _adress, value);
        }
        #endregion

        #region Порт

        private string _port;
        /// <summary>
        /// порт
        /// </summary>
        public string Port
        {
            get => _port;
            set => Set(ref _port, value);
        }
        #endregion

        #region TypeUid справочника

        private string _typeUid ;
        /// <summary>
        /// Uid справочника
        /// </summary>
        public string TypeUid
        {
            get => _typeUid;
            set => Set(ref _typeUid, value);
        }
        #endregion

        #region Логин

        private string _login;
        /// <summary>
        /// логин
        /// </summary>
        public string Login
        {
            get => _login;
            set => Set(ref _login, value);
        }
        #endregion

        #region Пароль

        private string _password ;
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }
        #endregion

        #region Анимация загрузки

        private string _loading = "Hidden"; //Visible
        /// <summary>
        /// Анимация загрузки
        /// </summary>
        public string Loading
        {
            get => _loading;
            set => Set(ref _loading, value);
        }
        #endregion

        #endregion

        #region Команды

        #region Команда кнопки сохранения 
        public ICommand SaveSettingsCommand { get; set; }
        private void OnSaveSettingsCommandExecuted(object p)
        {
            try
            {
                if (TokenBot != null && TokenElma != null && Adress != null && Port != null && TypeUid != null && Login != null && Password != null)
                {

                    Loading = "Visible";
                    Task.Run(() => CheckSetting());

                }
                else
                {
                    MessageBox.Show("Заполните все поля");
                }
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка запуска потока проверки настроек | " + ex);
            }
        }
        private bool CanSaveSettingsCommandExecute(object p) => true;
        #endregion

        #region Команда перехода на предыдущию 
        public ICommand BackCommand { get; set; }
        private void OnBackCommandExecuted(object p)
        {
            
           

        }
        private bool CanBackCommandExecute(object p) => true;
        #endregion

        #endregion

        public SettingPageViewModel()
        {
            try
            {
                

                #region Команды
                SaveSettingsCommand = new LambdaCommand(OnSaveSettingsCommandExecuted, CanSaveSettingsCommandExecute);
                BackCommand = new LambdaCommand(OnBackCommandExecuted, CanBackCommandExecute);
                #endregion

               
                if (Common.IsPass == "true")
                {
                    IsPass = true;
                }
                else { IsPass = false; }

                RestClient = new RestClient();
            }
            catch(Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка конструкта SettingPageViewModel | " + ex);
            }
        }

        #region Функции проверки настроек

        private async void  CheckSetting()
        {
            try
            {
                //bool botToken = CheckTokenBot();//проверка токена бота
                //if (botToken == true)
                //{
                //    bool adresPort = CheckAdresPort();//проверка адреса и порта
                //    if (adresPort == true)
                //    {
                //        bool LoginAndTokenElmma = CheckTokenElmaandLoginPas();//проверка токена Ельмы, логина и пароля
                //        if (LoginAndTokenElmma == true)
                //        {                           
                //            bool IsTypeUid = CheckEnt();//проверка TypeUid справочника                          
                //            if (IsTypeUid == true)
                //            {
                //                if (IsPass == false)
                //                {
                //                    config.AppSettings.Settings["IsPass"].Value = "false";
                //                }
                //                else { config.AppSettings.Settings["IsPass"].Value = "true";}

                //                ConfigurationManager.RefreshSection("appSettings");
                //                config.Save(ConfigurationSaveMode.Modified);
                //                Loading = "Hidden";
                //                MessageBox.Show("Успешно.Настройка завершена. Для применения настроек перезапустите программу.");

                //            } 
                            
                //            else { Loading = "Hidden"; MessageBox.Show("Неверный Uid справочника. Настройка не завершена"); }
                //        }
                //        else { Loading = "Hidden"; MessageBox.Show("Неверный токен Elma или логин с паролем. Настройка не завершена"); }
                //    }
                //    else { Loading = "Hidden"; MessageBox.Show("Неверный адрес или порт. Настройка не завершена"); }
                //}
                //else { Loading = "Hidden"; MessageBox.Show("Неверный токен бота. Настройка не завершена"); }
            }
            catch (Exception ex)
            {
                Loading = "Hidden";
                MessageBox.Show("Что-то пошло не так. Попробуйте еще раз.");
                MainWindowViewModel.Log.Error("Ошибка проверки настроек | " + ex);
                
            }
        }

        public static bool CheckTokenBot(string TokenBot)
        {
            try
            {
                ITelegramBotClient bot = new TelegramBotClient(TokenBot);
                bot.GetMeAsync().Wait();
                TelegramCore.TelegramToken = TokenBot;
                config.AppSettings.Settings["TokenTelegram"].Value = TokenBot;
                ConfigurationManager.RefreshSection("appSettings");
                config.Save(ConfigurationSaveMode.Modified);

                return true;
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка проверки бота | " + ex);
                return false; 
            }

        }

        public static bool CheckTokenElmaandLoginPas(string Adress, string Port, string Login,string TokenElma,string Password, bool IsPass)
        {
            return true;
        }

        public static bool CheckAdresPort(string Adress,string Port)
        {
            return true;


        }

        public static bool CheckEnt(string Adress,string Port,string TypeUid)
        {

            return true;
            

        }

        private bool  CheckEntity()
        {
            return true;
            
        }
        #endregion
    }
}
