using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class UserData : IUserData
    {
        private readonly ISqlDataAccess _db;
        
        // declaring the names of the columns
        private readonly string _usernameColumnString = "username";
        private readonly string _passhashColumnString = "passhash";
        private readonly string _saltColumnString = "salt";
        private readonly string _isAdminColumnString = "is_admin";
        public UserData(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<List<UserModel>> GetUsers()
        {
            string sql = "select * from users";
            return _db.LoadData<UserModel, dynamic>(sql, new { });
        }

        public Task PostUser(UserModel user)
        {
            string sql = $"insert into users ({_usernameColumnString}, {_passhashColumnString}, {_saltColumnString}, {_isAdminColumnString})" +
                         "values (@Username, @Passhash, @Salt, @IsAdmin);";
            return _db.SaveData(sql, user);
        }
    }
}