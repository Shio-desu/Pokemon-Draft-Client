using System;

namespace DataAccessLibrary.Models
{
    public enum LobbyType
    {
        Default,
        Auction
    }
    public class SessionModel
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; } = "Draft Lobby";
        public LobbyType SessionType { get; set; }
        public bool HasStarted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}