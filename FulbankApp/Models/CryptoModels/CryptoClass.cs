using FulbankApp.Models.WalletModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.CryptoModels
{
    public class CryptoClass
    {
        private int Id { get; set; }

        private string Label { get; set; }

        private string Symbol { get; set; }

        private WalletClass Wallet { get; set; }

        public CryptoClass(int id, string label, string symbol)
        {
            Id = id;
            Label = label;
            Symbol = symbol;
        }
    }
}
