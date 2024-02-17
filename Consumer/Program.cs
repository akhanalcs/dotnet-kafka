using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
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

// Populate consumer config
builder.Services.Configure<ConsumerConfig>(config.GetSection("KafkaConsumer"));

builder.Services.AddSingleton<IConsumer<string, Biometrics>>(sp =>
{
  var consumerConfig = sp.GetRequiredService<IOptions<ConsumerConfig>>();
  return new ConsumerBuilder<string, Biometrics>(consumerConfig.Value)
    .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
    .Build();
});

builder.Services.AddHostedService<HeartRateZoneWorker>();

using IHost host = builder.Build();
// https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#create-logs-in-main
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Host created. Now running it.");

await host.RunAsync();