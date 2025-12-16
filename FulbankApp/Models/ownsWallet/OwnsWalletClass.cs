using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Windows.Input;
using FulbankApp.Models.WalletModels;

namespace FulbankApp.Models.OwnsWalletModels
{
    [Table("ownsWallet")]
    public class OwnsWalletClass
    {
        // En EF Core, il faudra configurer la clé composite (IdUser, IdWallet) dans le DbContext
        // Data Annotations ne gère pas nativement les clés composites multiples simplement ici, 
        // mais voici les propriétés :

        [Key, Column(Order = 0)]
        public int IdUser { get; set; }

        [Key, Column(Order = 1)]
        public int IdWallet { get; set; }

        [ForeignKey("IdUser")]
        public virtual UserClass User { get; set; }

        [ForeignKey("IdWallet")]
        public virtual WalletClass Wallet { get; set; }
    }
}
