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
            var result = consumer.Consume(stoppingToken); // The OFFSET of this message can be viewed using: result.TopicPartitionOffset
            logger.LogInformation("Message Received: {0}", result.Message.Value);
            await HandleMessage(result.Message.Value, stoppingToken);
            
            //await Task.CompletedTask;
            //await Task.Delay(1000, stoppingToken);
        }
        
        consumer.Close();
    }

    private async Task HandleMessage(Biometrics metrics, CancellationToken stoppingToken)
    {
        // offsets contain the next offset to be processed (i.e., the current position plus one)
        var offsets = consumer.Assignment.Select(topicPartition => 
            new TopicPartitionOffset(topicPartition,
                // Returns the current position (offset) of the consumer for a specific topic / partition.
                // The offset field of each requested partition will be set to the offset of the last consumed message + 1.
                consumer.Position(topicPartition)));
        
        producer.BeginTransaction();
        // Send the offsets of next consume positions to the transaction, indicating where the consumer is ready to consume next. 
        // For eg: When you COMMIT OFFSET 3, you're telling Kafka that your consumer has successfully processed the record upto offset 2 and is ready to consume the record at offset 3.
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