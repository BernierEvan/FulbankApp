using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.CheckingModels
{
    public class CheckingClass
    {
        private int Id { get; set; }

        private string Label { get; set; }

        private decimal Overdraft { get; set; }

        private decimal Fees { get; set; }

        public CheckingClass(int id, string label, decimal overdraft, decimal fees)
        {
            Id = id;
            Label = label;
            Overdraft = overdraft;
            Fees = fees;
        }
    }
}
