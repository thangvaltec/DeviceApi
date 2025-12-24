using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("vain_datum")]
    public class VeinDatum
    {
        [Key]
        [Column("recId")]
        public int RecId { get; set; }     // PK

        [Column("sensorType")]
        public int SensorType { get; set; } = 0;

        [Column("dataType")]
        public int DataType { get; set; } = 0;

        [Column("id")]
        public string EmployeeId { get; set; } = string.Empty;

        [Column("veinData", TypeName = "bytea")]
        public byte[] VeinData { get; set; } = Array.Empty<byte>();

        [Column("DelFlg")]
        public bool DelFlg { get; set; } = false;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
