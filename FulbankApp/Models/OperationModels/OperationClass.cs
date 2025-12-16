using FulbankApp.Models.AccountModels;
using FulbankApp.Models.WalletModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FulbankApp.Models.WalletCryptoLinkModels;

namespace FulbankApp.Models.OperationModels
{
    [Table("Operation")]
    public class OperationClass
    {
        [Key]
        public int IdOperation { get; set; }

        public DateTime? OperationDateTime { get; set; }
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(50)]
        public string OperationType { get; set; }

        [StringLength(50)]
        public string Description { get; set; }

        // Foreign Keys
        public int IdWalletCryptoSource { get; set; }
        public int IdWalletCryptoLink_1 { get; set; }
        public int IdAccount { get; set; }
        public int IdAccount_1 { get; set; }

        // Navigations
        [ForeignKey("IdWalletCryptoLink")]
        public virtual WalletCryptoLinkClass WalletSource { get; set; }

        [ForeignKey("IdWalletCryptoLink_1")]
        public virtual WalletCryptoLinkClass WalletDestination { get; set; }

        [ForeignKey("IdAccount")]
        public virtual AccountClass AccountSource { get; set; }

        [ForeignKey("IdAccount_1")]
        public virtual AccountClass AccountDestination { get; set; }
    }
}
