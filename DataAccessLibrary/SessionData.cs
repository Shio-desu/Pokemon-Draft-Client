using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class SessionData(ISqlDataAccess db) : ISessionData
    {
        // declaring the names of the columns
        private readonly string _sessionIdColumnString = "session_id";
        private readonly string _startDateColumnString = "start_date";
        private readonly string _endDateColumnString = "end_date";
        private readonly string _sessionNameColumnString = "session_name";
        private readonly string _sessionTypeColumnString = "session_type";
        private readonly string _hasStartedColumnString = "has_started";

        public Task<List<SessionModel>> GetSessions()
        {
            string sql = "select * from sessions";
            return db.LoadData<SessionModel, dynamic>(sql, new { });
        }

        public Task<SessionModel> PostSession(SessionModel session)
        {
            string sql = $"insert into sessions ({_startDateColumnString}, {_endDateColumnString}, {_sessionNameColumnString}, {_sessionTypeColumnString}, {_hasStartedColumnString}) " +
                         "values (@StartDate, @EndDate, @SessionName, @SessionType, @HasStarted);";
            return db.SaveData(sql, session);
        }
        
        public Task<int> PostSessionReturnId(SessionModel session)
        {
            string sql = $"insert into sessions ({_startDateColumnString}, {_endDateColumnString}, {_sessionNameColumnString}, {_sessionTypeColumnString}, {_hasStartedColumnString}) " +
                         "values (@StartDate, @EndDate, @SessionName, @SessionType, @HasStarted) " +
                         $"returning {_sessionIdColumnString};";
            return db.SaveDataReturnId(sql, session);
        }
        
        public Task DeleteSession(SessionModel session)
        {
            string sql = $"delete from sessions where {_sessionIdColumnString} = @SessionId;";
            return db.SaveData(sql, session);
        }
    }
}