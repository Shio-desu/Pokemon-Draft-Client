using DataAccessLibrary.Models;

namespace Pokemon_Draft_Client.Models.ViewModels;

public class LobbyViewModel
{
    public int LobbyId { get; set; }
    public required LobbyUsers LobbyUsers { get; set; }
}