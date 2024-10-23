using MonitorService.Kafka;
// using MonitorService.Services;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddSingleton<NumberAnalyzerService>();
builder.Services.AddHostedService<KafkaConsumer>();

var app = builder.Build();

app.MapGet("/Ping", () => Results.Ok());

app.Run();
