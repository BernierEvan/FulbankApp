using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FulbankApp.Models.CardCategoryModels
{
    [Table("CardCategory")]
    public class CardCategory
    {
        [Key]
        public int IdCardCategory { get; set; }

        [StringLength(50)]
        public string Label { get; set; }
    }
}
