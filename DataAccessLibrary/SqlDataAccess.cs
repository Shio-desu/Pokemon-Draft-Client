using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccessLibrary.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataAccessLibrary
{
    public class SqlDataAccess : ISqlDataAccess
    {
        public string ConnectionStringName { get; set; } = "Default";

        readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<T>> LoadData<T, TU>(string sql, TU parameters)
        {
            string? connectionString = _config.GetConnectionString(ConnectionStringName);

            using IDbConnection connection = new SqlConnection(connectionString);
            var data = await connection.QueryAsync<T>(sql, parameters);
            return data.ToList();
        }

        public async Task SaveData<T>(string sql, T parameters)
        {
            string? connectionString = _config.GetConnectionString(ConnectionStringName);

            using IDbConnection connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(sql, parameters);
        }
}
}