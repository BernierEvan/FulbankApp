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
            _connectionString = SecureConfig.GetConnectionString();
        }

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}