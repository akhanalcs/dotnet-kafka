// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using var host = Host.CreateApplicationBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();

var kafkaBootstrapServer = config["Kafka:BootstrapServer"];

Console.WriteLine("Bootstrap Server: {0}", kafkaBootstrapServer);
Console.ReadLine();