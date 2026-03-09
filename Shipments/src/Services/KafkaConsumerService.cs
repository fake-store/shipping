using System.Text.Json;
using Confluent.Kafka;
using Shipments.Dto;

namespace Shipments.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly AddressService _addressService;
    private readonly KafkaProducerService _producer;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(
        IConfiguration config,
        AddressService addressService,
        KafkaProducerService producer,
        ILogger<KafkaConsumerService> logger)
    {
        _config = config;
        _addressService = addressService;
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _config["Kafka:BootstrapServers"] ?? "localhost:9091";
        var groupId = _config["Kafka:GroupId"] ?? "shipping-service";
        var inboundTopic = _config["Kafka:InboundTopic"] ?? "orders.shipping.authorized";

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(inboundTopic);
        _logger.LogInformation("Kafka consumer started on topic {Topic}", inboundTopic);

        await Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result?.Message?.Value is null) continue;

                    var msg = JsonSerializer.Deserialize<ShippingAuthorizedMessage>(
                        result.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (msg is null)
                    {
                        _logger.LogWarning("Failed to deserialize shipping authorized message");
                        continue;
                    }

                    HandleShipment(msg);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka message");
                }
            }
        }, stoppingToken);

        consumer.Close();
    }

    private void HandleShipment(ShippingAuthorizedMessage msg)
    {
        var address = Guid.TryParse(msg.ShippingAddressId, out var addrId)
            ? _addressService.GetByAddressId(addrId)
            : null;

        var addressLine = address is not null
            ? $"{address.Street}, {address.City}, {address.State} {address.PostalCode}, {address.Country}"
            : "[address not found]";

        var items = string.Join(", ", msg.Items.Select(i => $"{i.Quantity}x {i.Title}"));
        var tracking = $"FAKE-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        _logger.LogInformation(
            "LABEL PRINTED — Order: {OrderId} | Items: {Items} | Ship to: {Address} | Tracking: {Tracking}",
            msg.OrderId, items, addressLine, tracking);

        _producer.PublishOrderShipped(new OrderShippedMessage(
            msg.OrderId, msg.UserId, tracking, msg.TraceId
        )).GetAwaiter().GetResult();
    }
}
