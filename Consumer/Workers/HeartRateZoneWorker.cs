using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Consumer.Workers;

public class HeartRateZoneWorker(
    ILogger<HeartRateZoneWorker> logger,
    IConsumer<string, Biometrics> consumer) : BackgroundService
{
    private const string BiometricsImportedTopicName = "BiometricsImported";
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe(BiometricsImportedTopicName);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = consumer.Consume(stoppingToken);
            logger.LogInformation("Message Received: {0}", result.Message.Value);
            await Task.CompletedTask;
            
            //await Task.Delay(1000, stoppingToken);
        }
        
        consumer.Close();
    }
    
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("HeartRateZoneWorker is stopping.");
        await base.StopAsync(stoppingToken);
    }
}