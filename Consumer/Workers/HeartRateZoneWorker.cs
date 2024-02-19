using Confluent.Kafka;
using Consumer.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Consumer.Workers;

public class HeartRateZoneWorker(
    ILogger<HeartRateZoneWorker> logger,
    IConsumer<string, Biometrics> consumer,
    IProducer<string, HeartRateZoneReached> producer) : BackgroundService
{
    private const string BiometricsImportedTopicName = "BiometricsImported";
    private const string HeartRateZoneReachedTopicName = "HeartRateZoneReached";
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        producer.InitTransactions(_defaultTimeout);
        consumer.Subscribe(BiometricsImportedTopicName);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = consumer.Consume(stoppingToken);
            logger.LogInformation("Message Received: {0}", result.Message.Value);
            await HandleMessage(result.Message.Value, stoppingToken);
            
            //await Task.CompletedTask;
            //await Task.Delay(1000, stoppingToken);
        }
        
        consumer.Close();
    }

    private async Task HandleMessage(Biometrics metrics, CancellationToken stoppingToken)
    {
        var offsets = consumer.Assignment.Select(topicPartition => 
            new TopicPartitionOffset(topicPartition,
                consumer.Position(topicPartition))); // consumer.Position gets the current position (offset) for the specified topic / partition.
        
        producer.BeginTransaction();
        producer.SendOffsetsToTransaction(offsets, consumer.ConsumerGroupMetadata, _defaultTimeout);
        
        try
        {
            var heartRates = metrics.HeartRates.Where(hr => hr.GetHeartRateZone(metrics.MaxHeartRate) != HeartRateZone.None);

            var produceTasks = heartRates.Select(hr =>
            {
                var zone = hr.GetHeartRateZone(metrics.MaxHeartRate);
                var heartRateZoneReached = new HeartRateZoneReached(metrics.DeviceId,
                    zone,
                    hr.DateTime,
                    hr.Value,
                    metrics.MaxHeartRate);
                
                var message = new Message<string, HeartRateZoneReached>
                {
                    Key = metrics.DeviceId.ToString(),
                    Value = heartRateZoneReached
                };

                return producer.ProduceAsync(HeartRateZoneReachedTopicName, message, stoppingToken);
            });

            await Task.WhenAll(produceTasks); // <-- Waits for all the tasks to complete
            producer.CommitTransaction();
        }
        catch (Exception e)
        {
            producer.AbortTransaction();
            throw new Exception($"{nameof(HandleMessage)} failed", e);
        }
    }
    
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("HeartRateZoneWorker is stopping.");
        await base.StopAsync(stoppingToken);
    }
}