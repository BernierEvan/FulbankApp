using FulbankApp.Models.AccountModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FulbankApp.Models.SubscriptionModels
{
    [Table("Subscription")]
    public class SubscriptionClass
    {
        [Key]
        public int IdSubscribtion { get; set; }

        [StringLength(50)]
        public string Label { get; set; }

        public decimal Price { get; set; } // DECIMAL(15,2) -> decimal

        // Navigation (facultatif, pour voir qui a cet abonnement)
        public virtual ICollection<AccountClass> Accounts { get; set; }
    }
}
