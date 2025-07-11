using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLibrary.Models;

namespace DataAccessLibrary.Interfaces;

public interface IParticipationData
{
    Task<List<ParticipationModel>> GetParticipations();
    Task<ParticipationModel> PostParticipation(ParticipationModel participation);
    Task<int> PostParticipationReturnId(ParticipationModel participation);
    Task DeleteParticipation(ParticipationModel participation);
}