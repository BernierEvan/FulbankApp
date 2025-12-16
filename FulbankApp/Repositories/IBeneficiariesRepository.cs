using System.Collections.Generic;
using FulbankApp.Models.BeneficiariesModels;
using FulbankApp.Models.WalletModels;

namespace FulbankApp.Repositories
{
    public interface IBeneficiariesRepository
    {
        IEnumerable<BeneficiaryClass> GetBeneficiaryByUserId(int userId);
        BeneficiaryClass GetById(int id);
        int Add(BeneficiaryClass beneficiary, int ownerUserId);
        bool Update(BeneficiaryClass beneficiairy);
        bool Remove(int id);
    }
}