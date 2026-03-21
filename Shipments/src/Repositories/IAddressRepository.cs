using Shipments.Dto;
using Shipments.Models;

namespace Shipments.Repositories;

public interface IAddressRepository
{
    List<ShippingAddress> GetByUserId(Guid userId);
    ShippingAddress? GetById(Guid userId, Guid addressId);
    ShippingAddress? GetById(Guid addressId);
    ShippingAddress Add(Guid userId, CreateAddressRequest req);
    ShippingAddress? Update(Guid userId, Guid addressId, UpdateAddressRequest req);
    bool Delete(Guid userId, Guid addressId);
}
