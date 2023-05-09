using System.ComponentModel.DataAnnotations;

namespace Core.Data.Models
{
    public class UserState
    {
        [Key]
        public int Id { get; set; }
        public StatusCode Code { get; set; }
        public string? Description { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public enum StatusCode
    {
        Active,
        Blocked
    }
}
