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
        
        // Methods that take a CancellationToken as an argument periodically check the token to see if they should cancel their operation and return.
        while (!stoppingToken.IsCancellationRequested)
        {
            // If there are no more messages to process, the Consume method will not return null or exit immediately;
            // instead, it will continue to wait for new messages (blocking behavior).
            // While the Consume method does block the current thread it's running on (waiting for new messages),
            // it does not block other threads in your application or make the entire application slow.
            // Other background services or tasks will continue to run normally in separate threads.
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
        // Send the current offsets (the position in the topic/partition it has consumed up to) to the transaction.
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