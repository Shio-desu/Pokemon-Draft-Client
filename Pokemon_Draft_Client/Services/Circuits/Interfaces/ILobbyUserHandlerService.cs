using DataAccessLibrary.Models;
using Pokemon_Draft_Client.Models;

namespace Pokemon_Draft_Client.Services.Circuits;

public interface ILobbyUserHandlerService
{
    Dictionary<int, LobbyUsers> LobbyUsersMap { get; }
    static event EventHandler? LobbyUsersChanged;
    Task CreateLobby(string lobbyName, LobbyType lobbyType, int userId);
    Task Join(int lobbyId, int userId);
    Task Leave(int lobbyId, int userId);
}