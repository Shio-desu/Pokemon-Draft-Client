using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public interface IUserData
    {
        Task<List<UserModel>> GetUsers();
        Task PostUser(UserModel user);
    }
}