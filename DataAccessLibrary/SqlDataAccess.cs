using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccessLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

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
            string? connectionString = _config["PostgresConnectionString"];
            
            using IDbConnection connection = new NpgsqlConnection(connectionString);
            var data = await connection.QueryAsync<T>(sql, parameters);
            
            return data.ToList();
        }

        public async Task SaveData<T>(string sql, T parameters)
        {
            string? connectionString = _config["PostgresConnectionString"];

            using IDbConnection connection = new NpgsqlConnection(connectionString);
            await connection.ExecuteScalarAsync(sql, parameters);
        }
        
        public async Task<int> SaveDataReturnId<T>(string sql, T parameters)
        {
            string? connectionString = _config["PostgresConnectionString"];

            using IDbConnection connection = new NpgsqlConnection(connectionString);
            int id = (int)(await connection.ExecuteScalarAsync(sql, parameters) ?? -1);
            return id;
        }
}
}