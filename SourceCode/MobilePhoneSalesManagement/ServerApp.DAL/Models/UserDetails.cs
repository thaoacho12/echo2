namespace ServerApp.DAL.Models
{
    public class UserDetails
    {
        public int UserDetailsId { get; set; }
        public int UserId { get; set; } // Foreign Key to User

        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
