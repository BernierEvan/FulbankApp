using FulbankApp.Models.WalletModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Pin { get; set; }
        public WalletClass Wallet { get; set; }


    }
}