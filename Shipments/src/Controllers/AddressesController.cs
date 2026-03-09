using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipments.Dto;
using Shipments.Services;

namespace Shipments.Controllers;

[ApiController]
[Route("api/shipping/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly AddressService _addressService;

    public AddressesController(AddressService addressService)
    {
        _addressService = addressService;
    }

    private Guid UserId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public IActionResult GetAll() => Ok(_addressService.GetByUserId(UserId));

    [HttpPost]
    public IActionResult Create([FromBody] CreateAddressRequest req)
    {
        var address = _addressService.Add(UserId, req);
        return CreatedAtAction(nameof(GetAll), new { }, address);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] UpdateAddressRequest req)
    {
        var updated = _addressService.Update(UserId, id, req);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var deleted = _addressService.Delete(UserId, id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
