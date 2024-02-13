using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Confluent.Kafka;

using var host = Host.CreateApplicationBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

var consumerConfig = config.GetSection("KafkaConsumer").Get<ConsumerConfig>();

Console.ReadLine();