namespace DataAccessLibrary.Models
{
    public class SessionModel
    {
        public int SessionId { get; set; }
        public double StartDate { get; set; }
        public double EndDate { get; set; }
        public string SessionName { get; set; }
        public string SessionType { get; set; }
        public bool HasStarted { get; set; }
    }
}