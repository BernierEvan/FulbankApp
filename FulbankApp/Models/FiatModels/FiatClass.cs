using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FulbankApp.Models.FiatModels
{
    [Table("Fiat")]
    public class FiatClass
    {
        [Key]
        public int IdFiat { get; set; }

        [StringLength(50)]
        public string Currency { get; set; }
    }
}
