using System;

namespace FulbankApp.Models
{
    /// <summary>
    /// Représente un compte bancaire dans le système.
    /// Contient les informations relatives au compte, au solde et au propriétaire.
    /// </summary>
    public class BankAccount
    {
        /// <summary>
        /// Identifiant unique du compte bancaire.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur propriétaire du compte.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Numéro de compte bancaire.
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Code IBAN (International Bank Account Number).
        /// </summary>
        public string Iban { get; set; }

        /// <summary>
        /// Type de compte (ex: Courant, Épargne).
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Solde actuel du compte.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Devise du compte (ex: EUR, USD).
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Date de création du compte.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Retourne le solde formaté avec la devise, ex: "1 000.00 EUR".
        /// </summary>
        public string FormattedBalance => $"{Balance:N2} {Currency}";
    }
}
