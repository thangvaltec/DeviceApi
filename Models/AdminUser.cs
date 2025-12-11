using System.ComponentModel.DataAnnotations;

namespace DeviceApi.Models
{
    public class AdminUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "admin";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
