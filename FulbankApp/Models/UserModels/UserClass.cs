using FulbankApp.Models.WalletModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FulbankApp.Models.AccountModels;
using FulbankApp.Models.OwnsWalletModels;

namespace FulbankApp.Models
{
    [Table("User_")] // Le nom de la table SQL a un underscore
    public class UserClass
    {
        [Key]
        public int IdUser { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        [StringLength(50)]
        public string Mail { get; set; }

        [StringLength(50)]
        public string Password { get; set; }

        [StringLength(50)]
        public string Pin { get; set; }

        // Navigation
        public virtual ICollection<AccountClass> Accounts { get; set; }
        public virtual ICollection<OwnsWalletClass> OwnedWallets { get; set; }
    }
}