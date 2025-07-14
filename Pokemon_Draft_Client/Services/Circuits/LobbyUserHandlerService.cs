using DataAccessLibrary;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;
using Pokemon_Draft_Client.Models;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class LobbyUserHandlerService : ILobbyUserHandlerService
{
    public Dictionary<int, LobbyUsers> LobbyUsersMap { get; private set; } = new();
    public static event EventHandler? LobbyUsersChanged;
    void OnLobbyUsersChanged() => LobbyUsersChanged?.Invoke(this, EventArgs.Empty);

    private readonly ISessionData _sessionData;
    private readonly IParticipationData _participationData;
    private readonly ICircuitUserHandlerService _circuitUserHandlerService;
    
    public LobbyUserHandlerService(ISessionData sessionData, IParticipationData participationData, ICircuitUserHandlerService circuitUserHandlerService)
    {
        _sessionData = sessionData;
        _participationData = participationData;
        _circuitUserHandlerService = circuitUserHandlerService;
        CircuitUserHandlerService.UserCircuitsChanged += HandleUserCircuitsChanged;
    }
    
    public async Task CreateLobby(string lobbyName, LobbyType lobbyType, int userId)
    {
        if (userId == -1) 
            return;
        
        SessionModel newSession = new()
        {
            HasStarted = false,
            SessionName = lobbyName,
            SessionType = lobbyType
        };
        var session = await _sessionData.PostSession(newSession);

        ParticipationModel newParticipation = new()
        {
            SessionId = session.SessionId,
            UserId = userId
        };
        var participation = await _participationData.PostParticipation(newParticipation);
        
        LobbyUsersMap[session.SessionId] = new LobbyUsers
        {
            Session = session,
            UserIds = [userId]
        };
        
        OnLobbyUsersChanged();
    }
    
    public async Task Join(int lobbyId, int userId)
    {
        if (userId == -1) 
            return;
        
        if (LobbyUsersMap.ContainsKey(lobbyId))
        {
            ParticipationModel newParticipation = new()
            {
                SessionId = lobbyId,
                UserId = userId
            };
            var participation = await _participationData.PostParticipation(newParticipation);
            LobbyUsersMap[lobbyId].UserIds.Add(userId);
            OnLobbyUsersChanged();
        }
    }

    public async Task Leave(int lobbyId, int userId)
    {
        if (!LobbyUsersMap.ContainsKey(lobbyId)) 
            return;
        
        LobbyUsersMap[lobbyId].UserIds.Remove(userId);
        ParticipationModel participationToDelete = new()
        {
            SessionId = lobbyId,
            UserId = userId
        };
        await _participationData.DeleteParticipation(participationToDelete);
        OnLobbyUsersChanged();
        
        if (LobbyUsersMap[userId].UserIds.Count != 0) 
            return;
        // deletes the lobby when the last user has left
        LobbyUsersMap.Remove(lobbyId);
        SessionModel sessionToDelete = new()
        {
            SessionId = lobbyId
        };
        await _sessionData.DeleteSession(sessionToDelete);
    }

    private void HandleUserCircuitsChanged(object? sender, int userId)
    {
        if (userId == -1)
            return;
        // if the user id is not part of the user circuits anymore, ergo closed the session / logged out
        if (!_circuitUserHandlerService.UserCircuitsMap.ContainsKey(userId))
        {
            foreach (var lobbyId in LobbyUsersMap.Keys)
            {
                if (LobbyUsersMap[lobbyId].UserIds.Contains(userId))
                {
                    _ = Leave(lobbyId, userId);
                    OnLobbyUsersChanged();
                    return;
                }
            }
        }
    }
    
}