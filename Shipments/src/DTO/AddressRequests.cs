namespace Shipments.Dto;

public record CreateAddressRequest(
    string Label,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);

public record UpdateAddressRequest(
    string? Label,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country
);
