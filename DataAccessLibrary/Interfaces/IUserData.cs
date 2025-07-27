using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Interfaces
{
    public interface IUserData
    {
        Task<List<UserModel>> GetUsers();
        Task<List<UserModel>> GetUserByName(string username);
        Task<UserModel> PostUser(UserModel user);
        Task<int> PostUserReturnId(UserModel user);
        Task DeleteUser(UserModel user);
    }
}