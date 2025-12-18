using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("contract_client")]
    public class ContractClient
    {
        [Key]
        public int Id { get; set; }     // PK

        [Required]
        public string ContractClientCd { get; set; } = string.Empty;

        public string ContractClientName { get; set; } = string.Empty;

        // Soft delete flag: false = active, true = deleted (hidden)
        public bool DelFlg { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
