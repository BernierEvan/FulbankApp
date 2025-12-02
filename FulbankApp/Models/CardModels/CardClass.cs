using FulbankApp.Models.AccountModels;
using FulbankApp.Models.CardCategoryModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.CardModels
{
    public class CardClass
    {
        private int Id { get; set; }

        private string Number { get; set; }

        private DateOnly ExpirationDate { get; set; }

        private string cvv { get; set; }

        private CardCategoryClass Category { get; set; }

        private AccountClass Account { get; set; }

        public CardClass(int id, string number, DateOnly expirationDate, string cvv, CardCategoryClass category, AccountClass account)
        {
            Id = id;
            Number = number;
            ExpirationDate = expirationDate;
            this.cvv = cvv;
            Category = category;
            Account = account;
        }
    }
}
