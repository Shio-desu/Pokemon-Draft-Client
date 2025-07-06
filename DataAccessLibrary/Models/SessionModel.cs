using System;

namespace DataAccessLibrary.Models
{
    public class SessionModel
    {
        public int SessionId { get; set; }
        public string? SessionName { get; set; }
        public string? SessionType { get; set; }
        public bool HasStarted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}