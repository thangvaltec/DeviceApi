using System.ComponentModel.DataAnnotations;

namespace DeviceApi.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }     // PK

        [Required]
        public string SerialNo { get; set; } = string.Empty;

        [Required]
        public string DeviceName { get; set; } = string.Empty;

        // 0 = Face, 1 = Vein, 2 = FaceAndVein
        public int AuthMode { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool DelFlg { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
