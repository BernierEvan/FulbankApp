using FulbankApp.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace FulbankApp.Repositories
{
    public class UserRepository : RepositoryBase, IuserRepository
    {
        public void Add(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Authentifie un utilisateur avec mot de passe hashé
        /// </summary>
        public bool AuthenticateUser(NetworkCredential credential)
        {
            bool validUser = false;

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                // ✅ Récupère uniquement le hash stocké
                command.CommandText = "SELECT [password] FROM [User] WHERE username=@username";
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = credential.UserName;

                var storedHash = command.ExecuteScalar() as string;

                if (storedHash != null)
                {
                    // ✅ Compare le hash du mot de passe saisi avec le hash stocké
                    string inputHash = HashPassword(credential.Password);
                    validUser = storedHash == inputHash;
                }
            }

            return validUser;
        }

        /// <summary>
        /// Hash un mot de passe avec SHA256 (à remplacer par bcrypt/Argon2 en production)
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public void Edit(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public UserModel GetByUsername(string username)
        {
            UserModel user = null;

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM [User] WHERE username=@username";
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            Id = reader[0].ToString(),
                            Username = reader[1].ToString(),
                            Password = string.Empty,  // ✅ Ne jamais retourner le mot de passe
                            Name = reader[3].ToString(),
                            LastName = reader[4].ToString(),
                            Email = reader[5].ToString(),
                        };
                    }
                }
            }

            return user;
        }

        public IEnumerable<UserModel> GetByAll()
        {
            throw new NotImplementedException();
        }

        public UserModel GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}