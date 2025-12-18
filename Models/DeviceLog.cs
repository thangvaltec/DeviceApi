using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("device_logs")]
    public class DeviceLog
    {
        [Key]
        public int Id { get; set; }

        public string SerialNo { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
