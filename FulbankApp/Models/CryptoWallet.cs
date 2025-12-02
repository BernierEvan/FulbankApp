using System;

namespace FulbankApp.Models
{
    public class CryptoWallet
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CryptoCode { get; set; }
        public decimal Amount { get; set; }
        public string WalletAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string FormattedAmount => $"{Amount:N8} {CryptoCode}";
    }
}
