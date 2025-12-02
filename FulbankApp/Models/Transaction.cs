using System;

namespace FulbankApp.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? AccountId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string RecipientName { get; set; }
        public string RecipientIban { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FormattedAmount => Amount >= 0 ? $"+ {Amount:N2} {Currency}" : $"- {Math.Abs(Amount):N2} {Currency}";
        public string FormattedDate => CreatedAt.ToString("dd/MM/yyyy");
    }
}
