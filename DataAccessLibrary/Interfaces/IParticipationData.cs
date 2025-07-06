using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary;

public interface IParticipationData
{
    Task<List<ParticipationModel>> GetParticipations();
    Task PostParticipation(ParticipationModel participation);
}