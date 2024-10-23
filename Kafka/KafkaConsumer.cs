using Confluent.Kafka;
// using MonitorService.Services;

namespace MonitorService.Kafka;

public class KafkaConsumer : IHostedService
{
    private readonly IConfiguration _config;
    // private readonly NumberAnalyzerService _numberAnalyzerService;

    public KafkaConsumer()//NumberAnalyzerService numberAnalyzerService)
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddIniFile("kafkaclient.properties", false)
            .Build();
        _config["group.id"] = "monitor-service";
        _config["auto.offset.reset"] = "earliest";

        // _numberAnalyzerService = numberAnalyzerService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => ConsumeAsync(cancellationToken));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {   
        return Task.CompletedTask;
    }

    private bool CheckIsSuspicious(int number)
    {
        // intentionally inefficient way of checking if a number is "suspicious"
        for (int i = 2; i < number; i++)
        {
            if (number % i == 0)
            {
                return false;
            }
        }
        return true;
    }

    private void ConsumeSaveNumberMessageAsync(Message<string, string> saveNumberMessage)
    {
        if (CheckIsSuspicious(int.Parse(saveNumberMessage.Value)))
        {
            KafkaProducer.Produce("numbers", "flagNumber", saveNumberMessage.Value);
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        string topic = "numbers"; 
        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };

        // creates a new consumer instance
        using (var consumer = new ConsumerBuilder<string, string>(_config.AsEnumerable()).Build()) {
            consumer.Subscribe(topic);
            try
            {
                while (!cancellationToken.IsCancellationRequested) {
                    ConsumeResult<string, string> consumeResult = consumer.Consume(cts.Token);
                    Console.WriteLine($"Consuming event from topic {topic}: key = {consumeResult.Message.Key,-10} value = {consumeResult.Message.Value}");
                    switch (consumeResult.Message.Key)
                    {
                        case "saveNumber":
                            ConsumeSaveNumberMessageAsync(consumeResult.Message);
                            break;
                        default:
                            break;
                            // throw new Exception($"Unknown key {consumeResult.Message.Key}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ctrl-C was pressed
            }
            finally
            {
                consumer.Close();
            }

            // return Task.CompletedTask;
        }
    }
}