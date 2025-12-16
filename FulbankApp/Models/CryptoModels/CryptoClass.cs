using FulbankApp.Models.WalletCryptoLinkModels;
using FulbankApp.Models.WalletModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FulbankApp.Models.CryptoModels
{
    [Table("Crypto")]
    public class CryptoClass
    {
        [Key]
        public int IdCrypto { get; set; }

        [StringLength(50)]
        public string Label { get; set; }

        [StringLength(50)]
        public string Symbol { get; set; }

        public int IdWalletCryptoLink { get; set; }

        [ForeignKey("IdWalletCryptoLink")]
        public virtual WalletCryptoLinkClass WalletCryptoLink { get; set; }
    }
}
