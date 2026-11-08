using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/customers")]
[Authorize]
public class CustomerController : BaseApiController
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.CustomerList)]
    public async Task<IActionResult> Search([FromQuery] CustomerSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CustomerView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.CustomerCreate)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CustomerUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CustomerDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _customerService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("send-voucher")]
    [Authorize(Policy = PermissionConstants.CustomerSendVoucher)]
    public async Task<IActionResult> SendVoucher([FromBody] SendVoucherRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.SendVoucherAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gửi voucher cho một khách hàng cụ thể (FE: POST /customers/{id}/voucher).</summary>
    [HttpPost("{id:guid}/voucher")]
    [Authorize(Policy = PermissionConstants.CustomerSendVoucher)]
    public async Task<IActionResult> SendVoucherToCustomer(Guid id, [FromBody] SendVoucherToCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.SendVoucherToCustomerAsync(id, request.CouponCode, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Xuất dữ liệu PII của các khách hàng (FE: POST /customers/export-pii).</summary>
    [HttpPost("export-pii")]
    [Authorize(Policy = PermissionConstants.CustomerExportPii)]
    public async Task<IActionResult> ExportPii([FromBody] ExportPiiRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.ExportPiiAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    // ----- Sổ địa chỉ -----

    [HttpGet("{customerId:guid}/addresses")]
    [Authorize(Policy = PermissionConstants.CustomerView)]
    public async Task<IActionResult> GetAddresses(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _customerService.GetAddressesAsync(customerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}/addresses")]
    [Authorize(Policy = PermissionConstants.CustomerUpdate)]
    public async Task<IActionResult> CreateAddress(Guid customerId, [FromBody] CreateCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateAddressAsync(customerId, request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("addresses/{addressId:guid}")]
    [Authorize(Policy = PermissionConstants.CustomerUpdate)]
    public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] UpdateCustomerAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _customerService.UpdateAddressAsync(addressId, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("addresses/{addressId:guid}")]
    [Authorize(Policy = PermissionConstants.CustomerUpdate)]
    public async Task<IActionResult> DeleteAddress(Guid addressId, CancellationToken cancellationToken)
    {
        var result = await _customerService.DeleteAddressAsync(addressId, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
