using System.Collections.Generic;
using FulbankApp.Models.WalletModels;

namespace FulbankApp.Repositories
{
    public interface IWalletRepository
    {
        IEnumerable<WalletClass> GetWalletsByUserId(int userId);
        WalletClass GetById(int id);
        int Add(WalletClass wallet);
        bool Update(WalletClass wallet);
        bool Remove(int id);
    }
}