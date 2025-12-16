using FulbankApp.Models.AccountModels;
using FulbankApp.Models.CardCategoryModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FulbankApp.Models.CheckingAccountModels;

namespace FulbankApp.Models.CardModels
{
    [Table("Card")]
    public class CardClass
    {
        [Key]
        public int IdCard { get; set; }

        [StringLength(50)]
        public string Label { get; set; }

        public decimal Fees { get; set; }

        public int IdAccount { get; set; } // Référence CheckingAccount
        public int IdCardCategory { get; set; }

        [ForeignKey("IdAccount")]
        public virtual CheckingAccountClass CheckingAccount { get; set; }

        [ForeignKey("IdCardCategory")]
        public virtual CardCategory Category { get; set; }
    }
}
