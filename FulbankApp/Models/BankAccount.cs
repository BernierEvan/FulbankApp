using System;

namespace FulbankApp.Models
{
    public class BankAccount
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; }
        public string Iban { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FormattedBalance => $"{Balance:N2} {Currency}";
    }
}
