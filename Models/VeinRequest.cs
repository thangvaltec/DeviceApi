using System.Text.Json.Serialization;

namespace DeviceApi.Models
{
    public class VeinRequest
    {
        [JsonPropertyName("id")]
        public String Id { get; set; } = string.Empty;
        
        [JsonPropertyName("sensortype")]
        public int SensorType { get; set; } = 0;

        [JsonPropertyName("datatype")]
        public int DataType { get; set; } = 0;

        [JsonPropertyName("veindata")]
        public string VeinDataBase64 { get; set; } = string.Empty;
    }
}
