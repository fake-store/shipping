using System.Collections.Concurrent;
using Shipments.Dto;
using Shipments.Models;

namespace Shipments.Repositories;

/// <summary>
/// Stub implementation — ignores userId on reads, all users share the same address pool.
/// Seeded with 4 well-known addresses. Replace with a SQL implementation in v2.
/// </summary>
public class InMemoryAddressRepository : IAddressRepository
{
    private readonly ConcurrentDictionary<Guid, ShippingAddress> _store = new(
        new Dictionary<Guid, ShippingAddress>
        {
            [Guid.Parse("11111111-0000-0000-0000-000000000001")] = new()
            {
                Id       = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                UserId   = Guid.Empty,
                Label    = "The White House",
                Street   = "1600 Pennsylvania Ave NW",
                City     = "Washington",
                State    = "DC",
                PostalCode = "20500",
                Country  = "US"
            },
            [Guid.Parse("11111111-0000-0000-0000-000000000002")] = new()
            {
                Id       = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                UserId   = Guid.Empty,
                Label    = "Empire State Building",
                Street   = "20 W 34th St",
                City     = "New York",
                State    = "NY",
                PostalCode = "10001",
                Country  = "US"
            },
            [Guid.Parse("11111111-0000-0000-0000-000000000003")] = new()
            {
                Id       = Guid.Parse("11111111-0000-0000-0000-000000000003"),
                UserId   = Guid.Empty,
                Label    = "Graceland",
                Street   = "3764 Elvis Presley Blvd",
                City     = "Memphis",
                State    = "TN",
                PostalCode = "38116",
                Country  = "US"
            },
            [Guid.Parse("11111111-0000-0000-0000-000000000004")] = new()
            {
                Id       = Guid.Parse("11111111-0000-0000-0000-000000000004"),
                UserId   = Guid.Empty,
                Label    = "The Alamo",
                Street   = "300 Alamo Plaza",
                City     = "San Antonio",
                State    = "TX",
                PostalCode = "78205",
                Country  = "US"
            }
        });

    public List<ShippingAddress> GetByUserId(Guid userId) =>
        [.. _store.Values];

    public ShippingAddress? GetById(Guid userId, Guid addressId) =>
        _store.GetValueOrDefault(addressId);

    public ShippingAddress? GetById(Guid addressId) =>
        _store.GetValueOrDefault(addressId);

    public ShippingAddress Add(Guid userId, CreateAddressRequest req)
    {
        var address = new ShippingAddress
        {
            UserId     = userId,
            Label      = req.Label,
            Street     = req.Street,
            City       = req.City,
            State      = req.State,
            PostalCode = req.PostalCode,
            Country    = req.Country
        };
        _store[address.Id] = address;
        return address;
    }

    public ShippingAddress? Update(Guid userId, Guid addressId, UpdateAddressRequest req)
    {
        if (!_store.TryGetValue(addressId, out var existing)) return null;

        if (req.Label      is not null) existing.Label      = req.Label;
        if (req.Street     is not null) existing.Street     = req.Street;
        if (req.City       is not null) existing.City       = req.City;
        if (req.State      is not null) existing.State      = req.State;
        if (req.PostalCode is not null) existing.PostalCode = req.PostalCode;
        if (req.Country    is not null) existing.Country    = req.Country;
        return existing;
    }

    public bool Delete(Guid userId, Guid addressId) =>
        _store.TryRemove(addressId, out _);
}
