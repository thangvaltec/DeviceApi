using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("auth_logs")]
    public class AuthLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SerialNo { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        // 0: 顔認証, 1: 静脈認証, 2: 顔＋静脈認証
        [Required]
        public int AuthMode { get; set; }

        [Required]
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
