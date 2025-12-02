using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.WalletModels
{
    public class WalletClass
    {
        private int Id { get; set; }
        private string Label { get; set; }
        private decimal Balance { get; set; }
        private int UserId { get; set; }
        public WalletClass(int id, string label, decimal balance, int userId)
        {
            Id = id;
            Label = label;
            Balance = balance;
            UserId = userId;
        }
    }
}
