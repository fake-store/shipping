using System.Text.Json;
using Confluent.Kafka;
using Shipments.Dto;

namespace Shipments.Services;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _outboundTopic;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _outboundTopic = config["Kafka:OutboundTopic"] ?? "shipping.order.shipped";
        var bootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9091";

        _producer = new ProducerBuilder<Null, string>(
            new ProducerConfig { BootstrapServers = bootstrapServers }
        ).Build();
    }

    public async Task PublishOrderShipped(OrderShippedMessage message)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await _producer.ProduceAsync(_outboundTopic, new Message<Null, string> { Value = json });
        _logger.LogInformation("Published order shipped: orderId={OrderId} tracking={Tracking}",
            message.OrderId, message.TrackingNumber);
    }

    public void Dispose() => _producer.Dispose();
}
