namespace DataAccessLibrary.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Passhash { get; set; }
        public string Salt { get; set; }
    }
}