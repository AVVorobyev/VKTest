using System.ComponentModel.DataAnnotations;

namespace Core.Data.Models
{
    public class User : IModel
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime? CreatedDate { get; set; }

        public UserGroup UserGroup { get; set; }
        public UserState UserState { get; set; }

        public User(
            string login,
            string password,
            UserGroup userGroup,
            UserState userState)
        {
            Login = login;
            Password = password;
            UserGroup = userGroup;
            UserState = userState;
        }

        /// <summary>
        /// Default Constructor for EntityFramework
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private User() { throw new NotImplementedException(); }
    }
}
