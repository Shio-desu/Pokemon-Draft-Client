using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class ParticipationData : IParticipationData
    {
        private readonly ISqlDataAccess _db;
        
        // declaring the names of the columns
        private readonly string _userIdColumnString = "user_id";
        private readonly string _sessionIdColumnString = "session_id";
        
        public ParticipationData(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<List<ParticipationModel>> GetParticipations()
        {
            string sql = "select * from participations";
            return _db.LoadData<ParticipationModel, dynamic>(sql, new { });
        }

        public Task PostParticipation(ParticipationModel participation)
        {
            string sql = $"insert into participations ({_userIdColumnString}, {_sessionIdColumnString}) " +
                         "values (@UserId, @SessionId);";
            return _db.SaveData(sql, participation);
        }
    }
}