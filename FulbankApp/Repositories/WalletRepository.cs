/*using FulbankApp.Models.WalletModels;
using Microsoft.Data.SqlClient;
using FulbankApp.Models.CryptoModels;
using System;
using System.Collections.Generic;
using System.Data;

namespace FulbankApp.Repositories
{
    public class WalletRepository : RepositoryBase, IWalletRepository
    {
        public IEnumerable<WalletClass> GetWalletsByUserId(int userId)
        {
            var list = new List<WalletClass>();

            var listCryptos = new Dictionary<CryptoClass, int> { };

            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = @"SELECT IdWallet, Label, Balance, IdUtilisateur FROM Wallets WHERE UserId = @userId";
            cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var w = new WalletClass
                {
                    IdWallet = reader.GetInt32(0),
                    Label = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Balance = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                    UserId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                };
                list.Add(w);
            }

            return list;
        }

        public WalletClass GetById(int id)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = @"SELECT Id, Label, Balance, UserId FROM Wallets WHERE Id = @id";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new WalletClass(
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                    reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                );
            }

            return null;
        }

        public int Add(WalletClass wallet)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = @"INSERT INTO Wallets (Label, Balance, UserId) OUTPUT INSERTED.Id VALUES (@label, @balance, @userId)";
            cmd.Parameters.Add("@label", SqlDbType.NVarChar, 200).Value = wallet.Label ?? string.Empty;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = wallet.Balance;
            cmd.Parameters["@balance"].Scale = 2;
            cmd.Parameters["@balance"].Precision = 18;
            cmd.Parameters.Add("@userId", SqlDbType.Int).Value = wallet.UserId;

            var inserted = cmd.ExecuteScalar();
            return inserted != null ? Convert.ToInt32(inserted) : 0;
        }

        public bool Update(WalletClass wallet)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = @"UPDATE Wallets SET Label = @label, Balance = @balance WHERE Id = @id";
            cmd.Parameters.Add("@label", SqlDbType.NVarChar, 200).Value = wallet.Label ?? string.Empty;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = wallet.Balance;
            cmd.Parameters["@balance"].Scale = 2;
            cmd.Parameters["@balance"].Precision = 18;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = wallet.Id;

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remove(int id)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = @"DELETE FROM Wallets WHERE Id = @id";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}*/