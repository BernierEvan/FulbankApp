/*using FulbankApp.Models.BeneficiariesModels;
using FulbankApp.Models.WalletModels; // Pour AccountClass si besoin
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using FulbankApp.Models.BeneficiariesModels;

namespace FulbankApp.Repositories
{
    public class BeneficiariesRepository : RepositoryBase, IBeneficiariesRepository
    {
        // J'ai renommé le paramètre en userId car on cherche les bénéficiaires D'UN utilisateur
        public IEnumerable<BeneficiaryClass> GetBeneficiaryByUserId(int accountId)
        {
            var list = new List<BeneficiaryClass>();

            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            // CORRECTION 1 : Table Beneficiaries, pas Wallets
            // On sélectionne Id, Name, Fban (et potentiellement une date de création si elle existe en base)
            cmd.CommandText = @"SELECT IdBeneficiaryTable, BeneficiaryName, BeneficiaryBirthDate FROM Beneficiaries;";
            //cmd.Parameters.Add("@accountId", SqlDbType.Int).Value = accountId;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                // CORRECTION 2 : Appel du constructeur exact requis par le modèle
                // Ordre supposé : (int id, AccountClass account, string name, DateOnly createdAt, int fban)

                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                DateTime birthdate = reader.GetDateTime(2);

                // On met 'null' pour AccountClass car on ne fait pas de JOIN ici
                // On met la date du jour par défaut si elle n'est pas en base
                var w = new BeneficiairyClass(
                    id,
                    null,
                    name,
                    DateOnly.FromDateTime(DateTime.Now),
                    fbanBeneficiary
                );

                list.Add(w);
            }

            return list;
        }

        public BeneficiairyClass GetById(int id)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            // CORRECTION : Table Beneficiaries et colonnes correctes
            cmd.CommandText = @"SELECT Id, Name, Fban FROM Beneficiaries WHERE Id = @id";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // CORRECTION : Appel du constructeur avec les bons types
                return new BeneficiairyClass(
                    reader.GetInt32(0), // Id
                    null,               // AccountClass (null pour l'instant)
                    reader.GetString(1),// Name
                    DateOnly.FromDateTime(DateTime.Now), // Date
                    reader.GetInt32(2)  // Fban
                );
            }

            return null;
        }

        // CORRECTION 3 : userId doit être passé en paramètre car 'BeneficiairyClass' ne le contient pas
        public int Add(BeneficiairyClass beneficiary, int ownerUserId)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            // CORRECTION : Table Beneficiaries
            cmd.CommandText = @"INSERT INTO Beneficiaries (Name, Fban, UserId) 
                                OUTPUT INSERTED.Id 
                                VALUES (@name, @fban, @userId)";

            // CORRECTION des paramètres
            cmd.Parameters.Add("@name", SqlDbType.NVarChar, 200).Value = beneficiary.Name ?? string.Empty;
            cmd.Parameters.Add("@fban", SqlDbType.Int).Value = beneficiary.Fban;
            cmd.Parameters.Add("@userId", SqlDbType.Int).Value = ownerUserId; // On utilise l'argument passé

            var inserted = cmd.ExecuteScalar();
            return inserted != null ? Convert.ToInt32(inserted) : 0;
        }

        public bool Update(BeneficiairyClass beneficiary)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            // CORRECTION : Suppression de la virgule en trop avant WHERE
            cmd.CommandText = @"UPDATE Beneficiaries SET Name = @name, Fban = @fban WHERE Id = @id";

            cmd.Parameters.Add("@name", SqlDbType.NVarChar, 200).Value = beneficiary.Name ?? string.Empty;
            cmd.Parameters.Add("@fban", SqlDbType.Int).Value = beneficiary.Fban;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = beneficiary.Id;

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remove(int id)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = @"DELETE FROM Beneficiaries WHERE Id = @id";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
*/