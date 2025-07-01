using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLibrary.Interfaces
{
    public interface ISqlDataAccess
    {
        string ConnectionStringName { get; set; }
        Task<List<T>> LoadData<T, TU>(string sql, TU parameters);
        Task SaveData<T>(string sql, T parameters);
    }
}