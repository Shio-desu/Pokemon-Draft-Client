using DataAccessLibrary;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;
using Pokemon_Draft_Client.Models;

namespace Pokemon_Draft_Client.Services.Circuits;

public class LobbyUserHandlerService(ISessionData sessionData, IParticipationData participationData)
{
    public Dictionary<int, LobbyUsers> LobbyUsersMap { get; private set; } = new();
    public event EventHandler LobbyUsersChanged;
    void OnLobbyUsersChanged() => LobbyUsersChanged?.Invoke(this, EventArgs.Empty);

    public async Task CreateLobby(string lobbyName, LobbyType lobbyType, int userId)
    {
        SessionModel newSession = new()
        {
            HasStarted = false,
            SessionName = lobbyName,
            SessionType = lobbyType
        };
        var session = await sessionData.PostSession(newSession);

        ParticipationModel newParticipation = new()
        {
            SessionId = session.SessionId,
            UserId = userId
        };
        var participation = await participationData.PostParticipation(newParticipation);
        
        LobbyUsersMap[session.SessionId] = new LobbyUsers
        {
            Session = session,
            UserIds = [userId]
        };
        
        OnLobbyUsersChanged();
    }
    
    public async Task Join(int lobbyId, int userId)
    {
        if (LobbyUsersMap.ContainsKey(lobbyId))
        {
            ParticipationModel newParticipation = new()
            {
                SessionId = lobbyId,
                UserId = userId
            };
            var participation = await participationData.PostParticipation(newParticipation);
            LobbyUsersMap[lobbyId].UserIds.Add(userId);
            OnLobbyUsersChanged();
        }
    }

    public async Task Leave(int lobbyId, int userId)
    {
        if (!LobbyUsersMap.ContainsKey(lobbyId)) return;
        
        LobbyUsersMap[lobbyId].UserIds.Remove(userId);
        ParticipationModel participationToDelete = new()
        {
            SessionId = lobbyId,
            UserId = userId
        };
        await participationData.DeleteParticipation(participationToDelete);
        OnLobbyUsersChanged();
        
        if (LobbyUsersMap[userId].UserIds.Count != 0) return;
        // deletes the lobby when the last user has left
        LobbyUsersMap.Remove(lobbyId);
        SessionModel sessionToDelete = new()
        {
            SessionId = lobbyId
        };
        await sessionData.DeleteSession(sessionToDelete);
    }

}