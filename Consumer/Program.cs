using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Consumer.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Consumer.Domain;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

//using var host = Host.CreateApplicationBuilder(args).Build();
//var config = host.Services.GetRequiredService<IConfiguration>(); // This works like below EXCEPT the user secrets part.
var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddEnvironmentVariables()
  .AddUserSecrets<Program>()
  .Build();

// Populate config objects
builder.Services.Configure<ConsumerConfig>(config.GetSection("KafkaConsumer"));
builder.Services.Configure<ProducerConfig>(config.GetSection("KafkaProducer"));
builder.Services.Configure<SchemaRegistryConfig>(config.GetSection("KafkaSchemaRegistry"));

// Consumer
builder.Services.AddSingleton<IConsumer<string, Biometrics>>(sp =>
{
  var consumerConfig = sp.GetRequiredService<IOptions<ConsumerConfig>>();
  return new ConsumerBuilder<string, Biometrics>(consumerConfig.Value)
    .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
    .Build();
});

// Producer
builder.Services.AddSingleton<IProducer<string, HeartRateZoneReached>>(sp =>
{
  var producerConfig = sp.GetRequiredService<IOptions<ProducerConfig>>();
  var schemaRegClient = sp.GetRequiredService<ISchemaRegistryClient>();
  return new ProducerBuilder<string, HeartRateZoneReached>(producerConfig.Value)
    // Provide a Serializer to the Producer
    .SetValueSerializer(new JsonSerializer<HeartRateZoneReached>(schemaRegClient))
    .Build();
});

// Schema Registry client
builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
  var schemaRegConfig = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();
  return new CachedSchemaRegistryClient(schemaRegConfig.Value);
});

builder.Services.AddHostedService<HeartRateZoneWorker>();

// A host is an object that encapsulates an app's resources and lifetime functionality, such as DI, Logging, Configuration, App shutdown, IHostedService implementations etc.
// The host and console app are not the same thing - the host is just a part that runs inside your console application.
using IHost host = builder.Build();

// https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#create-logs-in-main
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Host created. Now running it.");

await host.RunAsync();