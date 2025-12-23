using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("device_routing")]
    public class DeviceRouting
    {
        [Key]
        public string SerialNo { get; set; } = string.Empty;

        [Required]
        public string ContractClientCd { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
