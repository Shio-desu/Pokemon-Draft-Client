using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;

namespace DataAccessLibrary
{
    public class SessionData : ISessionData
    {
        private readonly ISqlDataAccess _db;
        
        // declaring the names of the columns
        private readonly string _startDateColumnString = "start_date";
        private readonly string _endDateColumnString = "end_date";
        private readonly string _sessionNameColumnString = "session_name";
        private readonly string _sessionTypeColumnString = "session_type";
        private readonly string _hasStartedColumnString = "has_started";
        
        public SessionData(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<List<SessionModel>> GetSessions()
        {
            string sql = "select * from sessions";
            return _db.LoadData<SessionModel, dynamic>(sql, new { });
        }

        public Task PostSession(SessionModel session)
        {
            string sql = $"insert into sessions ({_startDateColumnString}, {_endDateColumnString}, {_sessionNameColumnString}, {_sessionTypeColumnString}, {_hasStartedColumnString}) " +
                         "values (@StartDate, @EndDate, @SessionName, @SessionType, @HasStarted);";
            return _db.SaveData(sql, session);
        }
    }
}