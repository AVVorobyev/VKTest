using System.ComponentModel.DataAnnotations;

namespace Core.Data.Models
{
    public class UserGroup
    {
        [Key]
        public int Id { get; set; }
        public GroupCode Code { get; set; }
        public string? Description { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public enum GroupCode
    {
        Admin,
        User
    }
}
