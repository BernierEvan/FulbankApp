using FulbankApp.Models.AccountModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FulbankApp.Models.BeneficiariesModels
{
    public class BeneficiairyClass
    {
        private int Id { get; set; }
        private AccountClass Account { get; set; }

        private string Name { get; set; }

        private DateOnly CreatedAt { get; set; }

        private AccountClass AccountBeneficiairy { get; set; }
        private UserModel UserBeneficiairy { get; set; }

        public BeneficiairyClass(int id, AccountClass account, string name, DateOnly createdAt, AccountClass accountBeneficiairy, UserModel userBeneficiairy)
        {
            Id = id;
            Account = account;
            Name = name;
            CreatedAt = createdAt;
            AccountBeneficiairy = accountBeneficiairy;
            UserBeneficiairy = userBeneficiairy;
        }




    }
}
