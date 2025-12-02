using System;

namespace FulbankApp.Models
{
    public class UserSettings
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool SmsNotifications { get; set; }
        public bool EmailNotifications { get; set; }
        public bool ConnectionAlerts { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
