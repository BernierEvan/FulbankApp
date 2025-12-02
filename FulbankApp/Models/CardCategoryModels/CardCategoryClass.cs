using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.CardCategoryModels
{
    public class CardCategoryClass
    {
        private int Id { get; set; }

        private string Label { get; set; }

        public CardCategoryClass(int id, string label)
        {
            Id = id;
            Label = label;
        }
    }
}
