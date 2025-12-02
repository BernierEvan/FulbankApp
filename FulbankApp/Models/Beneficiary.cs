using System;

namespace FulbankApp.Models
{
    public class Beneficiary
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Iban { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
