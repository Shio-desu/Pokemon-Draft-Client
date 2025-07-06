using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class PickData : IPickData
    {
        private readonly ISqlDataAccess _db;
        
        // declaring the names of the columns
        private readonly string _participationIdColumnString = "participation_id";
        private readonly string _pokemonColumnString = "pokemon";
        
        public PickData(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<List<PickModel>> GetPicks()
        {
            string sql = "select * from picks";
            return _db.LoadData<PickModel, dynamic>(sql, new { });
        }

        public Task PostPick(PickModel pick)
        {
            string sql = $"insert into picks ({_participationIdColumnString}, {_pokemonColumnString}) " +
                         "values (@ParticipationId, @Pokemon);";
            return _db.SaveData(sql, pick);
        }
    }
}