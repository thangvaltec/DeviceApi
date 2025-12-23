using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceApi.Models
{
    [Table("devices")]
    public class Device
    {
        [Key]
        public int Id { get; set; }     // 主キー

        [Required]
        public string SerialNo { get; set; } = string.Empty;

        [Required]
        public string DeviceName { get; set; } = string.Empty;

        // 0 = 顔認証, 1 = 静脈認証, 2 = 顔＋静脈認証
        public int AuthMode { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // 論理削除フラグ: false = 有効, true = 削除済み（非表示）
        public bool DelFlg { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
