using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class PickData(ISqlDataAccess db) : IPickData
    {
        // declaring the names of the columns
        private readonly string _pickIdColumnString = "pick_id";
        private readonly string _participationIdColumnString = "participation_id";
        private readonly string _pokemonColumnString = "pokemon";

        public Task<List<PickModel>> GetPicks()
        {
            string sql = $"select {_pickIdColumnString} as PickId," +
                         $"{_participationIdColumnString} as ParticipationId," +
                         $"{_pokemonColumnString} as Pokemon from picks;";
            return db.LoadData<PickModel, dynamic>(sql, new { });
        }

        public Task<PickModel> PostPick(PickModel pick)
        {
            string sql = $"insert into picks ({_participationIdColumnString}, {_pokemonColumnString}) " +
                         "values (@ParticipationId, @Pokemon) " +
                         $"returning {_pickIdColumnString} as PickId," +
                         $"{_participationIdColumnString} as ParticipationId," +
                         $"{_pokemonColumnString} as  Pokemon;";
            return db.SaveDataReturnObject(sql, pick);
        }

        public Task<int> PostPickReturnId(PickModel pick)
        {
            string sql = $"insert into picks ({_participationIdColumnString}, {_pokemonColumnString}) " +
                         "values (@ParticipationId, @Pokemon) " +
                         $"returning {_pickIdColumnString} as PickId;";
            return db.SaveDataReturnId(sql, pick);
        }

        public Task DeletePick(PickModel pick)
        {
            string sql = $"delete from picks where {_pickIdColumnString} = @PickId;";
            return db.SaveData(sql, pick);
        }
    }
}