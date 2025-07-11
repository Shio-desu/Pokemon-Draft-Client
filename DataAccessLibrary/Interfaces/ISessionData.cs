using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Interfaces;

public interface ISessionData
{
    Task<List<SessionModel>> GetSessions();
    Task<SessionModel> PostSession(SessionModel session);
    Task<int> PostSessionReturnId(SessionModel session);
    Task DeleteSession(SessionModel session);
}