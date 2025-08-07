using DataAccessLibrary.Models;
using Pokemon_Draft_Client.Models;

namespace Pokemon_Draft_Client.Services.Circuits;

public interface ILobbyUserHandlerService
{
    Dictionary<int, LobbyUsers> LobbyUsersMap { get; }
    event EventHandler<int>? LobbyUsersChanged;
    Task<int> CreateLobby(string lobbyName, LobbyType lobbyType, string username);
    Task Join(int lobbyId, string username);
    Task Leave(int lobbyId, string username);
    Task Start(int lobbyId);
    void ChangeReadyStateOfUser(int lobbyId, string username);
}