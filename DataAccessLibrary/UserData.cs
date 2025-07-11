using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class UserData(ISqlDataAccess db) : IUserData
    {
        // declaring the names of the columns
        private readonly string _userIdColumnString = "user_id";
        private readonly string _usernameColumnString = "username";
        private readonly string _passhashColumnString = "passhash";
        private readonly string _saltColumnString = "salt";
        private readonly string _isAdminColumnString = "is_admin";

        public Task<List<UserModel>> GetUsers()
        {
            string sql = "select * from users";
            return db.LoadData<UserModel, dynamic>(sql, new { });
        }

        public Task<UserModel> PostUser(UserModel user)
        {
            string sql = $"insert into users ({_usernameColumnString}, {_passhashColumnString}, {_saltColumnString}, {_isAdminColumnString})" +
                         "values (@Username, @Passhash, @Salt, @IsAdmin);";
            return db.SaveData(sql, user);
        }

        public Task<int> PostUserReturnId(UserModel user)
        {
            string sql = $"insert into users ({_usernameColumnString}, {_passhashColumnString}, {_saltColumnString}, {_isAdminColumnString})" +
                         "values (@Username, @Passhash, @Salt, @IsAdmin) " +
                         $"returning {_userIdColumnString};";
            return db.SaveDataReturnId(sql, user);
        }

        public Task DeleteUser(UserModel user)
        {
            string sql = $"delete from users where {_userIdColumnString} = @UserId";
            return db.SaveData(sql, user);
        }
    }
}