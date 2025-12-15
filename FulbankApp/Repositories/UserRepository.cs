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
        /// <summary>
        /// Ajoute un nouvel utilisateur dans la base de données
        /// </summary>
        public void Add(UserModel userModel)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = @"INSERT INTO [User] 
                    (id, username, password, name, lastName, email, pin) 
                    VALUES (@id, @username, @password, @name, @lastName, @email, @pin)";

                command.Parameters.Add("@id", SqlDbType.NVarChar).Value = userModel.Id;
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = userModel.Username;
                command.Parameters.Add("@password", SqlDbType.NVarChar).Value = userModel.Password;
                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = userModel.Name;
                command.Parameters.Add("@lastName", SqlDbType.NVarChar).Value = userModel.LastName;
                command.Parameters.Add("@email", SqlDbType.NVarChar).Value = userModel.Email;
                command.Parameters.Add("@pin", SqlDbType.NVarChar).Value = (object)userModel.Pin ?? DBNull.Value;

                command.ExecuteNonQuery();
            }
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
                command.CommandText = "SELECT [password] FROM [User] WHERE username=@username";
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = credential.UserName;

                var storedHash = command.ExecuteScalar() as string;
                if (storedHash != null)
                {
                    string inputHash = HashPassword(credential.Password);
                    validUser = storedHash == inputHash;
                }
            }
            return validUser;
        }

        /// <summary>
        /// Met à jour un utilisateur existant
        /// </summary>
        public void Edit(UserModel userModel)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = @"UPDATE [User] 
                    SET username=@username, 
                        password=@password, 
                        name=@name, 
                        lastName=@lastName, 
                        email=@email,
                        pin=@pin
                    WHERE id=@id";

                command.Parameters.Add("@id", SqlDbType.NVarChar).Value = userModel.Id;
                command.Parameters.Add("@username", SqlDbType.NVarChar).Value = userModel.Username;
                command.Parameters.Add("@password", SqlDbType.NVarChar).Value = userModel.Password;
                command.Parameters.Add("@name", SqlDbType.NVarChar).Value = userModel.Name;
                command.Parameters.Add("@lastName", SqlDbType.NVarChar).Value = userModel.LastName;
                command.Parameters.Add("@email", SqlDbType.NVarChar).Value = userModel.Email;
                command.Parameters.Add("@pin", SqlDbType.NVarChar).Value = (object)userModel.Pin ?? DBNull.Value;

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Récupère un utilisateur par son nom d'utilisateur
        /// </summary>
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
                            Id = reader["id"].ToString(),
                            Username = reader["username"].ToString(),
                            Password = reader["password"].ToString(),
                            Name = reader["name"].ToString(),
                            LastName = reader["lastName"].ToString(),
                            Email = reader["email"].ToString(),
                            Pin = reader["pin"] != DBNull.Value ? reader["pin"].ToString() : null
                        };
                    }
                }
            }
            return user;
        }

        /// <summary>
        /// Récupère un utilisateur par son ID
        /// </summary>
        public UserModel GetById(int id)
        {
            UserModel user = null;
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM [User] WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            Id = reader["id"].ToString(),
                            Username = reader["username"].ToString(),
                            Password = reader["password"].ToString(),
                            Name = reader["name"].ToString(),
                            LastName = reader["lastName"].ToString(),
                            Email = reader["email"].ToString(),
                            Pin = reader["pin"] != DBNull.Value ? reader["pin"].ToString() : null
                        };
                    }
                }
            }
            return user;
        }

        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        public IEnumerable<UserModel> GetByAll()
        {
            var users = new List<UserModel>();
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM [User]";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new UserModel()
                        {
                            Id = reader["id"].ToString(),
                            Username = reader["username"].ToString(),
                            Password = string.Empty, // Ne jamais exposer les mots de passe
                            Name = reader["name"].ToString(),
                            LastName = reader["lastName"].ToString(),
                            Email = reader["email"].ToString(),
                            Pin = null // Ne jamais exposer les PINs
                        };
                        users.Add(user);
                    }
                }
            }
            return users;
        }

        /// <summary>
        /// Supprime un utilisateur
        /// </summary>
        public void Remove(int id)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "DELETE FROM [User] WHERE id=@id";
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Hash un mot de passe avec SHA256
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
    }
}