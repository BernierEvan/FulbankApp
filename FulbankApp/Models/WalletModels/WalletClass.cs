using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FulbankApp.Models.WalletCryptoLinkModels;
using FulbankApp.Models.OwnsWalletModels;

namespace FulbankApp.Models.WalletModels
{
    [Table("Wallet")]
    public class WalletClass
    {
        [Key]
        public int IdWallet { get; set; }

        public int IdWalletCryptoLink { get; set; }

        [ForeignKey("IdWalletCryptoLink")]
        public virtual WalletCryptoLinkClass WalletCryptoLink { get; set; }

        public virtual ICollection<OwnsWalletClass> WalletOwners { get; set; }
    }
}
