using System.Collections.Concurrent;
using Shipments.Dto;
using Shipments.Models;

namespace Shipments.Services;

public class AddressService
{
    private readonly ConcurrentDictionary<Guid, List<ShippingAddress>> _store = new();

    public List<ShippingAddress> GetByUserId(Guid userId) =>
        _store.TryGetValue(userId, out var list) ? list : [];

    public ShippingAddress? GetById(Guid userId, Guid addressId) =>
        GetByUserId(userId).FirstOrDefault(a => a.Id == addressId);

    public ShippingAddress Add(Guid userId, CreateAddressRequest req)
    {
        var address = new ShippingAddress
        {
            UserId = userId,
            Label = req.Label,
            Street = req.Street,
            City = req.City,
            State = req.State,
            PostalCode = req.PostalCode,
            Country = req.Country
        };
        _store.AddOrUpdate(userId, [address], (_, list) => { list.Add(address); return list; });
        return address;
    }

    public ShippingAddress? Update(Guid userId, Guid addressId, UpdateAddressRequest req)
    {
        var address = GetById(userId, addressId);
        if (address is null) return null;
        if (req.Label is not null) address.Label = req.Label;
        if (req.Street is not null) address.Street = req.Street;
        if (req.City is not null) address.City = req.City;
        if (req.State is not null) address.State = req.State;
        if (req.PostalCode is not null) address.PostalCode = req.PostalCode;
        if (req.Country is not null) address.Country = req.Country;
        return address;
    }

    public bool Delete(Guid userId, Guid addressId)
    {
        if (!_store.TryGetValue(userId, out var list)) return false;
        var removed = list.RemoveAll(a => a.Id == addressId);
        return removed > 0;
    }

    public ShippingAddress? GetByAddressId(Guid addressId)
    {
        foreach (var list in _store.Values)
        {
            var found = list.FirstOrDefault(a => a.Id == addressId);
            if (found is not null) return found;
        }
        return null;
    }
}
