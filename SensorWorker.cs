using System.Text;
using System.Text.Json;

public record SensorMessage(
    string DeviceId,
    string SensorType,
    double Value,
    double Latitude,
    double Longitude,
    DateTime Timestamp);

public class SensorWorker
{
    private readonly string _deviceId;
    private readonly string _sensorType;
    private readonly int _intervalMs;
    private readonly double _lat;
    private readonly double _lon;
    private readonly string _queueUrl;
    private readonly HttpClient _http;

    private readonly Random _rnd = new();

    public SensorWorker(
        string deviceId,
        string sensorType,
        int intervalMs,
        double latitude,
        double longitude,
        string queueUrl,
        HttpClient http)
    {
        _deviceId = deviceId;
        _sensorType = sensorType;
        _intervalMs = intervalMs;
        _lat = latitude;
        _lon = longitude;
        _queueUrl = queueUrl;
        _http = http;
    }

    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            double value = GenerateValue();

            var msg = new SensorMessage(
                _deviceId,
                _sensorType,
                value,
                _lat,
                _lon,
                DateTime.UtcNow);

            string json = JsonSerializer.Serialize(msg);

            string payloadXml =
                $"<QueueMessage><MessageText>{System.Convert.ToBase64String(Encoding.UTF8.GetBytes(json))}</MessageText></QueueMessage>";

            var content = new StringContent(payloadXml, Encoding.UTF8, "application/xml");

            var response = await _http.PostAsync(_queueUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[{_deviceId}] Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }
            else
            {
                Console.WriteLine($"[{_deviceId}] Sent {json}");
            }

            await Task.Delay(_intervalMs, token);
        }
    }

    private double GenerateValue()
    {
        return _sensorType switch
        {
            "temperature" => 18 + _rnd.NextDouble() * 10,
            "humidity" => 40 + _rnd.NextDouble() * 30,
            _ => _rnd.NextDouble() * 100
        };
    }
}
