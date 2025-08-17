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

    private const int AmountOfReconnectTries = 10;
    private const int ReconnectPollingTimerMSeconds = 1 * 1000;
    private const bool IsOwner = true;
    private const bool IsNotOwner = false;
    
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
        await _participationData.PostParticipation(newParticipation);
        
        LobbyUsersMap[session.SessionId] = new LobbyUsers
        {
            Session = session
        };
        LobbyUsersMap[session.SessionId].AddUser(username, IsOwner);
        
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
        
        if (LobbyUsersMap.TryGetValue(lobbyId, out var lobbyUsers))
        {
            if (LobbyUsersMap[lobbyId].UsersDict.ContainsKey(username)) 
                return;
            
            ParticipationModel newParticipation = new()
            {
                SessionId = lobbyId,
                UserId = userId
            };
            await _participationData.PostParticipation(newParticipation);
            
            lobbyUsers.AddUser(username, IsNotOwner);
            OnLobbyUsersChanged(lobbyId);
        }
    }

    public async Task Leave(int lobbyId, string username)
    {
        // users won't be able to leave a draft-lobby if it has started, they can always reconnect
        if (LobbyUsersMap[lobbyId].Session.HasStarted) return;
        
        if (!LobbyUsersMap.ContainsKey(lobbyId)) 
            return;
        
        var allUsers = await _userData.GetUsers();
        
        // if lobby owner leaves, remove everyone participating (also from the database) to close the lobby
        if (LobbyUsersMap[lobbyId].UsersDict[username].IsOwner)
        {
            foreach (var user in LobbyUsersMap[lobbyId].UsersDict.Values)
            {
                var userId = allUsers.Find(userModel => userModel.Username == user.Username)?.UserId ?? -1;
                if (userId == -1)
                    throw new Exception("User not found in database");
                
                LobbyUsersMap[lobbyId].RemoveUser(username);
                ParticipationModel participationToDelete = new()
                {
                    SessionId = lobbyId,
                    UserId = userId
                };
                await _participationData.DeleteParticipation(participationToDelete);
            }
        }
        else
        {
            var userId = allUsers.Find(user => user.Username == username)?.UserId ?? -1;
            if (userId == -1)
                throw new Exception("User not found in database");

            LobbyUsersMap[lobbyId].RemoveUser(username);
            ParticipationModel participationToDelete = new()
            {
                SessionId = lobbyId,
                UserId = userId
            };
            await _participationData.DeleteParticipation(participationToDelete);
        }
        
        if (LobbyUsersMap[lobbyId].UsersDict.Count != 0)
        {
            OnLobbyUsersChanged(lobbyId);
            return;
        }
        
        // deletes the lobby when the last user has left
        LobbyUsersMap.Remove(lobbyId);
        SessionModel sessionToDelete = new()
        {
            SessionId = lobbyId
        };
        await _sessionData.DeleteSession(sessionToDelete);
        OnLobbyUsersChanged(lobbyId);
    }

    public async Task Start(int lobbyId)
    {
        var sessionList = await _sessionData.GetSessionById(lobbyId);
        if (sessionList.Count == 0)
            throw new Exception("Session you wanted to start doesn't exist the the Database.");

        var session = sessionList[0];
        session.HasStarted = true;
        session.StartDate = DateTime.UtcNow;
        await _sessionData.UpdateSession(session);
        LobbyUsersMap[lobbyId].Session = session;
        OnLobbyUsersChanged(lobbyId);
    }

    public async Task End(int lobbyId)
    {
        var sessionList = await _sessionData.GetSessionById(lobbyId);
        if (sessionList.Count == 0)
            throw new Exception("Session you wanted to start doesn't exist the the Database.");

        var session = sessionList[0];
        session.HasStarted = true;
        session.EndDate = DateTime.UtcNow;
        await _sessionData.UpdateSession(session);
        LobbyUsersMap[lobbyId].Session = session;
        OnLobbyUsersChanged(lobbyId);
    }
    
    public void ChangeReadyStateOfUser(int lobbyId, string username)
    {
        LobbyUsersMap.TryGetValue(lobbyId, out var lobbyUsers);
        if (lobbyUsers is null) return;
        lobbyUsers.UsersDict.TryGetValue(username, out var user);
        if (user is null) return;
        user.IsReady = !user.IsReady;
        OnLobbyUsersChanged(lobbyId);
    }
    
    // waits for the user to reconnect before removing from the lobby
    private async Task RemoveUserAfterDelay(string username, int lobbyId)
    {
        // users won't be able to leave a draft-lobby if it has started, they can always reconnect
        if (LobbyUsersMap[lobbyId].Session.HasStarted) return;
        
        LobbyUsersMap[lobbyId].UsersDict[username].IsConnected = false;
        LobbyUsersMap[lobbyId].UsersDict[username].IsReady = false;
        OnLobbyUsersChanged(lobbyId);
        for (int i = 0; i < AmountOfReconnectTries; i++)
        {
            await Task.Delay(ReconnectPollingTimerMSeconds);
            if (!_circuitUserHandlerService.UserCircuitsMap.ContainsKey(username)) continue;
            LobbyUsersMap[lobbyId].UsersDict[username].IsConnected = true;
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
                if (!LobbyUsersMap[lobbyId].UsersDict.ContainsKey(username)) continue;
                _ = RemoveUserAfterDelay(username, lobbyId);
                return;
            }
        }
    }
    
}