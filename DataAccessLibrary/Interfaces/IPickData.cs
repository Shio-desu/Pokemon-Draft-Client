using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Interfaces;

public interface IPickData
{
    Task<List<PickModel>> GetPicks();
    Task<PickModel> PostPick(PickModel pick);
    Task<int> PostPickReturnId(PickModel pick);
    Task DeletePick(PickModel pick);
}