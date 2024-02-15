using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string biometricsImportedTopicName = "RawBiometricsImported";
var producerConfig = builder.Configuration.GetSection("KafkaProducer").Get<ProducerConfig>();
builder.Services.AddSingleton(new ProducerBuilder<string, string>(producerConfig).Build());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Logger.LogInformation("Adding Routes");
app.MapPost("/biometrics", async (string metrics, IProducer<string, string> producer) =>
    {
        app.Logger.LogInformation("Accepted Biometrics");
        var message = new Message<string, string> { Value = metrics };
        var result = await producer.ProduceAsync(biometricsImportedTopicName, message);
        producer.Flush();

        return TypedResults.Accepted("", metrics);
    })
    .WithName("RecordMeasurements")
    .WithOpenApi();

app.Logger.LogInformation("Starting the app");
app.Run();

record Biometrics(Guid DeviceId, List<HeartRate> HeartRates);
record HeartRate(DateTime DateTime, int Value);