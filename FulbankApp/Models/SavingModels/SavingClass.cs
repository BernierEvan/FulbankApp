using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.SavingModels
{
    public class SavingClass
    {
        private int Id { get; set; }

        private decimal InterestRate { get; set; }

        private decimal MinimumBalance { get; set; }

        public SavingClass(int id, decimal interestRate, decimal minimumBalance)
        {
            Id = id;
            InterestRate = interestRate;
            MinimumBalance = minimumBalance;
        }
    }
}
