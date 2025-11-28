namespace Emulator
{
    public class AppConfig
    {
        public string QueueUrl { get; set; }
        public List<SensorConfig> Sensors { get; set; }
    }

    public class SensorConfig
    {
        public string DeviceId { get; set; }
        public string SensorType { get; set; }
        public int IntervalMs { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
