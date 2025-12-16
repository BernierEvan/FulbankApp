using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace FulbankApp.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly string _connectionString;

        public RepositoryBase()
        {
            _connectionString = "Server=172.16.119.44;Database=FULBANK;User Id=fulbank_user;Password=MonSuperMotDePasse123!;Encrypt=False;";
        }

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}