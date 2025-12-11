using System.ComponentModel.DataAnnotations;

namespace DeviceApi.Models
{
    public class DeviceLog
    {
        [Key]
        public int Id { get; set; }

        public string SerialNo { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
