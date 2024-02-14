using Microsoft.Extensions.Configuration;
using Confluent.Kafka;

//using var host = Host.CreateApplicationBuilder(args).Build();
//var config = host.Services.GetRequiredService<IConfiguration>(); // This works like below EXCEPT the user secrets part.
var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddEnvironmentVariables()
  .AddUserSecrets<Program>()
  .Build();

var consumerConfig = config.GetSection("KafkaConsumer").Get<ConsumerConfig>()!;

Console.ReadLine();