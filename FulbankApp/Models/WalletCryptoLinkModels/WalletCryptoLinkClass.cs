using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FulbankApp.Models.CryptoModels;

namespace FulbankApp.Models.WalletCryptoLinkModels
{
    [Table("WalletCryptoLink")]
    public class WalletCryptoLinkClass
    {
        [Key]
        public int IdWalletCryptoLink { get; set; }

        // DECIMAL(25,15) nécessite une précision élevée, decimal gère 28-29 digits
        [Column(TypeName = "decimal(25,15)")]
        public decimal Balance { get; set; }

        // Navigations
        public virtual ICollection<CryptoClass> Cryptos { get; set; }
    }
}
