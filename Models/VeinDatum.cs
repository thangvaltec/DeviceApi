using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("vain_datum")]
    public class VeinDatum
    {
        [Key]
        public int Id { get; set; }     // PK

        [Required]
        public int SensorType { get; set; } = 0;

        [Required]
        public int DataType { get; set; } = 0;

        [Required]
        public int EmployeeId { get; set; } = 0;

        [Required]
        public bool VeinData { get; set; } = true;

        // Soft delete flag: false = active, true = deleted (hidden)
        public bool DelFlg { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
