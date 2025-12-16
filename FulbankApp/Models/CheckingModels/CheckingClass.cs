using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FulbankApp.Models.AccountModels;
using FulbankApp.Models.CardModels;

namespace FulbankApp.Models.CheckingAccountModels
{
    [Table("CheckingAccount")]
    public class CheckingAccountClass : AccountClass
    {
        // Attention : Votre table CheckingAccount a aussi une colonne "Limit".
        // Comme la classe mère "Account" a déjà "Limit", il faut distinguer celle-ci.
        // Je la renomme "CheckingLimit" côté C# pour éviter les conflits, mais elle map sur "Limit" en DB.

        [Column("Limit")]
        public int CheckingLimit { get; set; }

        // Navigation vers les cartes
        public virtual ICollection<CardClass> Cards { get; set; }
    }
}
