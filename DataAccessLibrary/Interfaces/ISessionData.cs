using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary;

public interface ISessionData
{
    Task<List<SessionModel>> GetSessions();
    Task PostSession(SessionModel session);
}