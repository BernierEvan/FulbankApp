using FulbankApp.Helpers;
using System;
namespace FulbankApp.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public string PageTitle => "Paramètres";
        public bool SMS_NOTIFICATION = false;
        public bool EMAIL_NOTIFICATION = false;
        public bool CONNECTION_ALERT = false;
        public bool SECURITY_2FA = false;

        public void GetUserSettings()
        {
            this.SMS_NOTIFICATION = Constants.SMS_NOTIFICATION;
            this.EMAIL_NOTIFICATION = Constants.EMAIL_NOTIFICATION;
            this.CONNECTION_ALERT = Constants.CONNECTION_ALERT;
            this.SECURITY_2FA = Constants.SECURITY_2FA;
        }
    }
}