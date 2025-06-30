using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class UserData : IUserData
    {
        private readonly ISqlDataAccess _db;

        public UserData(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<List<UserModel>> GetUsers()
        {
            string sql = "select * from User";
            return _db.LoadData<UserModel, dynamic>(sql, new { });
        }

        public Task PostUser(UserModel user)
        {
            string sql = @"insert into User (username, passhash, salt) 
                            values (@Username, @Passhash, @Salt);";
            return _db.SaveData(sql, user);
        }
    }
}