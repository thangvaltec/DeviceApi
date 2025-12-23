using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("contract_client")]
    public class ContractClient
    {
        [Key]
        public int Id { get; set; }     // 主キー

        [Required]
        public string ContractClientCd { get; set; } = string.Empty;

        public string ContractClientName { get; set; } = string.Empty;

        // 論理削除フラグ: false = 有効, true = 削除済み（非表示）
        public bool DelFlg { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
