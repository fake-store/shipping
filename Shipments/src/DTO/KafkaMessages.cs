namespace Shipments.Dto;

public record OrderItem(string ProductId, string Title, int Quantity);

public record ShippingAuthorizedMessage(
    string OrderId,
    string UserId,
    string ShippingAddressId,
    List<OrderItem> Items,
    string? TraceId
);

public record OrderShippedMessage(
    string OrderId,
    string UserId,
    string TrackingNumber,
    string? TraceId
);
