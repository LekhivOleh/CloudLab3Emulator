using DotNetEnv;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        Env.Load();

        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        string queueUrl = config["QUEUE_URL"]
            ?? throw new ArgumentNullException("QueueUrl not configured");

        var sensorConfigs = new List<SensorConfig>();
        config.GetSection("Sensors").Bind(sensorConfigs);

        using var http = new HttpClient();
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("Stopping emulator...");
            e.Cancel = true;
            cts.Cancel();
        };

        var tasks = new List<Task>();

        foreach (var s in sensorConfigs)
        {
            var worker = new SensorWorker(
                s.DeviceId,
                s.SensorType,
                s.IntervalMs,
                s.Latitude,
                s.Longitude,
                queueUrl,
                http);

            tasks.Add(worker.RunAsync(cts.Token));
        }

        Console.WriteLine("Sensor emulator running. Press Ctrl+C to stop.");

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelation requested.");
        }
    }
}

public class SensorConfig
{
    public string DeviceId { get; set; } = default!;
    public string SensorType { get; set; } = default!;
    public int IntervalMs { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
