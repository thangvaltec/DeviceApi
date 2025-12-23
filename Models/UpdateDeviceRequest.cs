namespace DeviceApi.Models
{
    public class UpdateDeviceRequest
    {
        public string SerialNo { get; set; } = string.Empty;
        public int AuthMode { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
