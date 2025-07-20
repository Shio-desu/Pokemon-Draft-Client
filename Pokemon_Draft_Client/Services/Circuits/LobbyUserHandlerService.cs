using DataAccessLibrary;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Models;
using Pokemon_Draft_Client.Models;
using Pokemon_Draft_Client.Services.Circuits.Interfaces;

namespace Pokemon_Draft_Client.Services.Circuits;

public static class ConnectionStates
{
    public const string Connected = "Connected";
    public const string Disconnected = "Disconnected";
    public const int AmountOfReconnectTries = 30;
    public const int ReconnectPollingTimerMSeconds = 1 * 1000;
}

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
            Users = {[username] = ConnectionStates.Connected}
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
            LobbyUsersMap[lobbyId].Users[username] = ConnectionStates.Connected;
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
        
        LobbyUsersMap[lobbyId].Users.Remove(username, out _);
        ParticipationModel participationToDelete = new()
        {
            SessionId = lobbyId,
            UserId = userId
        };
        await _participationData.DeleteParticipation(participationToDelete);
        OnLobbyUsersChanged();
        
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
        LobbyUsersMap[lobbyId].Users[username] = ConnectionStates.Disconnected;
        
        for (int i = 0; i < ConnectionStates.AmountOfReconnectTries; i++)
        {
            await Task.Delay(ConnectionStates.ReconnectPollingTimerMSeconds);
            if (!_circuitUserHandlerService.UserCircuitsMap.ContainsKey(username)) continue;
            LobbyUsersMap[lobbyId].Users[username] = ConnectionStates.Connected;
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
                OnLobbyUsersChanged();
                return;
            }
        }
    }
    
}