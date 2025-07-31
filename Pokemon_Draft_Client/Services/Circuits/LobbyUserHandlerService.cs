using DataAccessLibrary;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;
using Pokemon_Draft_Client.Models;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public class LobbyUserHandlerService : ILobbyUserHandlerService
{
    public Dictionary<int, LobbyUsers> LobbyUsersMap { get; private set; } = new();
    public event EventHandler<int>? LobbyUsersChanged;
    void OnLobbyUsersChanged(int lobbyId) => LobbyUsersChanged?.Invoke(this, lobbyId);

    private const int AmountOfReconnectTries = 30;
    private const int ReconnectPollingTimerMSeconds = 1 * 1000;
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

        var creator = new User
        {
            Username = username,
            IsConnected = true,
            IsOwner = true,
            IsReady = false
        };
        
        LobbyUsersMap[session.SessionId] = new LobbyUsers
        {
            Session = session,
            Users = {[username] = creator}
        };
        
        OnLobbyUsersChanged(session.SessionId);
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
        
        if (LobbyUsersMap.TryGetValue(lobbyId, out var value))
        {
            if (LobbyUsersMap[lobbyId].Users.ContainsKey(username)) 
                return;
            
            ParticipationModel newParticipation = new()
            {
                SessionId = lobbyId,
                UserId = userId
            };
            
            await _participationData.PostParticipation(newParticipation);
            
            var user = new User
            {
                Username = username,
                IsConnected = true,
                IsOwner = false,
                IsReady = false
            };
            value.Users[username] = user;
            OnLobbyUsersChanged(lobbyId);
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
        
        LobbyUsersMap[lobbyId].Users.TryRemove(username, out _);
        ParticipationModel participationToDelete = new()
        {
            SessionId = lobbyId,
            UserId = userId
        };
        await _participationData.DeleteParticipation(participationToDelete);
        OnLobbyUsersChanged(lobbyId);
        
        if (LobbyUsersMap[lobbyId].Users.Count != 0) 
            return;
        
        // deletes the lobby when the last user has left
        LobbyUsersMap.Remove(lobbyId);
        SessionModel sessionToDelete = new()
        {
            SessionId = lobbyId
        };
        await _sessionData.DeleteSession(sessionToDelete);
    }

    // waits for the user to reconnect before removing from the lobby
    private async Task RemoveUserAfterDelay(string username, int lobbyId)
    {
        LobbyUsersMap[lobbyId].Users[username].IsConnected = false;
        OnLobbyUsersChanged(lobbyId);
        for (int i = 0; i < AmountOfReconnectTries; i++)
        {
            await Task.Delay(ReconnectPollingTimerMSeconds);
            if (!_circuitUserHandlerService.UserCircuitsMap.ContainsKey(username)) continue;
            LobbyUsersMap[lobbyId].Users[username].IsConnected = true;
            OnLobbyUsersChanged(lobbyId);
            return;
        }
        
        await Leave(lobbyId, username);
    }
    
    private void HandleUserCircuitsChanged(object? sender, string username)
    {
        if (username.Equals(string.Empty))
            return;
        // if the user id is not part of the user circuits anymore, ergo closed the session / logged out
        if (!_circuitUserHandlerService.UserCircuitsMap.ContainsKey(username))
        {
            foreach (var lobbyId in LobbyUsersMap.Keys)
            {
                if (!LobbyUsersMap[lobbyId].Users.ContainsKey(username)) continue;
                _ = RemoveUserAfterDelay(username, lobbyId);
                return;
            }
        }
    }
    
}