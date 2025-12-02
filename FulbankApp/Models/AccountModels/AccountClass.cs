using FulbankApp.Models.MoneyModels;
using FulbankApp.Models.SubscriptionModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.AccountModels
{
    public class AccountClass
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }

        private SubscriptionClass Subscription { get; set; }

        private FiatClass Currency { get; set; }

        private UserModel UserAccount { get; set; }

        public AccountClass(int id, string accountNumber, decimal balance, SubscriptionClass subscription, FiatClass currency, UserModel userAccount)
        {
            Id = id;
            AccountNumber = accountNumber;
            Balance = balance;
            Subscription = subscription;
            Currency = currency;
            UserAccount = userAccount;
        }

    }
}
