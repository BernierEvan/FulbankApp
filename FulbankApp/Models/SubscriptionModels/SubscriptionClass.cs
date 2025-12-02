using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.SubscriptionModels
{
    public class SubscriptionClass
    {
        private int Id { get; set; }

        private string Label { get; set; }

        public SubscriptionClass(int id, string label)
        {
            Id = id;
            Label = label;
        }
    }
}
