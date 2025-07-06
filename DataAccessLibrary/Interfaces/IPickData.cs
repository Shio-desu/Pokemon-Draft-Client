using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary;

public interface IPickData
{
    Task<List<PickModel>> GetPicks();
    Task PostPick(PickModel pick);
}