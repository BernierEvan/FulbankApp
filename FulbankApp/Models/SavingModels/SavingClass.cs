using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FulbankApp.Models.AccountModels;

namespace FulbankApp.Models.SavingModels
{
    [Table("SavingAccount")]
    public class SavingAccountClass : AccountClass
    {
        // Pas besoin de redéfinir IdAccount, il est hérité
        public decimal Rate { get; set; } // DECIMAL(3,2)
    }
}
