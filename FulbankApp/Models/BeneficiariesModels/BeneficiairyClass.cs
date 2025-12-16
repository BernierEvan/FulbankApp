using FulbankApp.Models.AccountModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FulbankApp.Models.BeneficiariesModels
{
    [Table("Beneficiary")]
    public class BeneficiaryClass
    {
        [Key]
        public int IdBeneficiaryTable { get; set; }

        [StringLength(50)]
        public string BeneficaryName { get; set; }

        public DateTime? BeneficaryBirthDate { get; set; }
        public DateTime? CreatedAt { get; set; }

        public int IdAccount { get; set; }

        [ForeignKey("IdAccount")]
        public virtual AccountClass Account { get; set; }
    }
}
