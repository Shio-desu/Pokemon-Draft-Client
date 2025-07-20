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
    private readonly IUserData _userData;
    private readonly ICircuitUserHandlerService _circuitUserHandlerService;
    
    public LobbyUserHandlerService(ISessionData sessionData, IParticipationData participationData, IUserData userData, ICircuitUserHandlerService circuitUserHandlerService)
    {
        _sessionData = sessionData;
        _participationData = participationData;
        _userData = userData;
        _circuitUserHandlerService = circuitUserHandlerService;
        CircuitUserHandlerService.UserCircuitsChanged += HandleUserCircuitsChanged;
    }
    
    public async Task<int> CreateLobby(string lobbyName, LobbyType lobbyType, string username)
    {
        if (username.Equals(string.Empty)) 
            throw new ArgumentException("Username cannot be empty");
        
        var users = await _userData.GetUsers();
        var userId = users.Find(user => user.Username == username)?.UserId ?? -1;
        if (userId == -1)
            throw new Exception("User not found in database");
        
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
            Usernames = [username]
        };
        
        OnLobbyUsersChanged();
        return session.SessionId;
    }
    
    public async Task Join(int lobbyId, string username)
    {
        if (username.Equals(string.Empty)) 
            return;
        
        var users = await _userData.GetUsers();
        var userId = users.Find(user => user.Username == username)?.UserId ?? -1;
        if (userId == -1)
            throw new Exception("User not found in database");
        
        if (LobbyUsersMap.ContainsKey(lobbyId))
        {
            ParticipationModel newParticipation = new()
            {
                SessionId = lobbyId,
                UserId = userId
            };
            var participation = await _participationData.PostParticipation(newParticipation);
            LobbyUsersMap[lobbyId].Usernames.Add(username);
            OnLobbyUsersChanged();
        }
    }

    public async Task Leave(int lobbyId, string username)
    {
        if (!LobbyUsersMap.ContainsKey(lobbyId)) 
            return;
        
        var users = await _userData.GetUsers();
        var userId = users.Find(user => user.Username == username)?.UserId ?? -1;
        if (userId == -1)
            throw new Exception("User not found in database");
        
        LobbyUsersMap[lobbyId].Usernames.Remove(username);
        ParticipationModel participationToDelete = new()
        {
            SessionId = lobbyId,
            UserId = userId
        };
        await _participationData.DeleteParticipation(participationToDelete);
        OnLobbyUsersChanged();
        
        if (LobbyUsersMap[lobbyId].Usernames.Count != 0) 
            return;
        
        // deletes the lobby when the last user has left
        LobbyUsersMap.Remove(lobbyId);
        SessionModel sessionToDelete = new()
        {
            SessionId = lobbyId
        };
        await _sessionData.DeleteSession(sessionToDelete);
    }

    private void HandleUserCircuitsChanged(object? sender, string username)
    {
        if (username.Equals(string.Empty))
            return;
        // if the user id is not part of the user circuits anymore, ergo closed the session / logged out
        if (_circuitUserHandlerService.UserConnectionStatesMap.ContainsKey(username)) return;
        foreach (var lobbyId in LobbyUsersMap.Keys)
        {
            if (!LobbyUsersMap[lobbyId].Usernames.Contains(username)) continue;
            _ = Leave(lobbyId, username);
            OnLobbyUsersChanged();
            return;
        }
    }
    
}