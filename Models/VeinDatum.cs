using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("vain_datum")]
    public class VeinDatum
    {
        [Key]
        [Column("recid")]
        public int RecId { get; set; }     // PK

        [Column("sensortype")]
        public int SensorType { get; set; } = 0;

        [Column("datatype")]
        public int DataType { get; set; } = 0;

        [Column("id")]
        public string EmployeeId { get; set; } = string.Empty;

        [Column("veindata", TypeName = "bytea")]
        public byte[] VeinData { get; set; } = Array.Empty<byte>();

        // Soft delete flag: false = active, true = deleted (hidden)
        // public bool DelFlg { get; set; } = false;

        // public DateTime CreatedAt { get; set; } = DateTime.Now;

        // public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
