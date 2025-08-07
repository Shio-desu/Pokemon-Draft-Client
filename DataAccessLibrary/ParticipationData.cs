using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class ParticipationData(ISqlDataAccess db) : IParticipationData
    {
        // declaring the names of the columns
        private readonly string _participationIdColumnString = "participation_id";
        private readonly string _userIdColumnString = "user_id";
        private readonly string _sessionIdColumnString = "session_id";

        public Task<List<ParticipationModel>> GetParticipations()
        {
            string sql = $"select {_participationIdColumnString} as ParticipationId, " +
                         $"{_userIdColumnString} as UserId, " +
                         $"{_sessionIdColumnString} as SessionId from participations;";
            return db.LoadData<ParticipationModel, dynamic>(sql, new { });
        }

        public Task<List<ParticipationModel>> GetParticipationsFromSession(int sessionId)
        {
            string sql = $"select {_participationIdColumnString} as ParticipationId, " +
                         $"{_userIdColumnString} as UserId, " +
                         $"{_sessionIdColumnString} as SessionId from participations " +
                         $"where {_sessionIdColumnString} = {sessionId};";
            return db.LoadData<ParticipationModel, dynamic>(sql, new { });
        }
        
        public Task<ParticipationModel> PostParticipation(ParticipationModel participation)
        {
            string sql = $"insert into participations ({_userIdColumnString}, {_sessionIdColumnString}) " +
                         "values (@UserId, @SessionId) " +
                         $"returning {_participationIdColumnString} as ParticipationId, " +
                         $"{_userIdColumnString} as UserId, " +
                         $"{_sessionIdColumnString} as SessionId;";
            return db.SaveDataReturnObject(sql, participation);
        }

        public Task<int> PostParticipationReturnId(ParticipationModel participation)
        {
            string sql = $"insert into participations ({_userIdColumnString}, {_sessionIdColumnString}) " +
                         "values (@UserId, @SessionId) " +
                         $"returning {_participationIdColumnString} as ParticipationId;";
            return db.SaveDataReturnId(sql, participation);
        }
        
        public Task DeleteParticipation(ParticipationModel participation)
        {
            string sql = $"delete from participations where {_sessionIdColumnString} = @SessionId and {_userIdColumnString} = @UserId;";
            return db.SaveData(sql, participation);
        }
    }
}