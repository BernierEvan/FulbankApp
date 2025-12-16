using FulbankApp.Models.FiatModels;
using FulbankApp.Models.BeneficiariesModels;

using FulbankApp.Models.SubscriptionModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FulbankApp.Models.AccountModels
{
    [Table("Account")]
    public class AccountClass
    {
        [Key]
        public int IdAccount { get; set; }

        public int Balance { get; set; }
        public int Limit { get; set; }

        public int IdSubscribtion { get; set; }
        public int IdFiat { get; set; }
        public int IdUser { get; set; }

        // Navigations
        [ForeignKey("IdSubscribtion")]
        public virtual SubscriptionClass Subscription { get; set; }

        [ForeignKey("IdFiat")]
        public virtual FiatClass Fiat { get; set; }

        [ForeignKey("IdUser")]
        public virtual UserClass User { get; set; }

        public virtual ICollection<BeneficiaryClass> Beneficiaries { get; set; }
    }
}
