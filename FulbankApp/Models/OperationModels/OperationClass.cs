using FulbankApp.Models.AccountModels;
using FulbankApp.Models.WalletModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.OperationModels
{
    public class OperationClass
    {
        private int Id { get; set; }

        private DateOnly Date { get; set; }

        private decimal Amount { get; set; }

        private string Status { get; set; }

        private string Type { get; set; }

        private string Description { get; set; }

        private string Reference { get; set; }

        private WalletClass Wallet { get; set; }

        private WalletClass CounterpartyWallet { get; set; }

        private AccountClass BankAccount { get; set; }

        private AccountClass CounterpartyBankAccount { get; set; }

        public OperationClass(int id, DateOnly date, decimal amount, string status, string type, string description, string reference, WalletClass wallet, WalletClass counterpartyWallet, AccountClass bankAccount, AccountClass counterpartyBankAccount)
        {
            Id = id;
            Date = date;
            Amount = amount;
            Status = status;
            Type = type;
            Description = description;
            Reference = reference;
            Wallet = wallet;
            CounterpartyWallet = counterpartyWallet;
            BankAccount = bankAccount;
            CounterpartyBankAccount = counterpartyBankAccount;
        }
    }
}
