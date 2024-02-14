using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var producerConfig = builder.Configuration.GetSection("KafkaProducer").Get<ProducerConfig>();
using var producer = new ProducerBuilder<string, Biometrics>(producerConfig).Build();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Logger.LogInformation("Adding Routes");
app.MapPost("/biometrics", (Biometrics metrics) =>
    {
        var message = new Message<string, Biometrics>
        {
            Key = metrics.DeviceId.ToString(),
            Value = metrics
        };
        
        return TypedResults.Accepted($"/biometrics/{metrics.DeviceId}");
    })
    .WithName("RecordMeasurements")
    .WithOpenApi();

app.Logger.LogInformation("Starting the app");
app.Run();

record Biometrics(Guid DeviceId, List<HeartRate> HeartRates);
record HeartRate(DateTime DateTime, int Value);