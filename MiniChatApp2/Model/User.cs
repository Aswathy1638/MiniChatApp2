using System.ComponentModel.DataAnnotations;

namespace MiniChatApp2.Model
{
    public class User
    {
        public int Id { get; set; }
       
        public string? Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string? Token { get; set; }

    }
}
