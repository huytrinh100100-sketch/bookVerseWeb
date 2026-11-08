using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Customer;
using BookVerse.Core.Models.Order;
using BookVerse.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public CustomerService(AppDbContext context, UserManager<ApplicationUser> userManager, IAuditLogService auditLogService)
    {
        _context = context;
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<CustomerListItemResponse>>> SearchAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = from customer in _context.Users.AsNoTracking()
                    join province in _context.Provinces.AsNoTracking() on customer.ProvinceId equals province.Id into provinceJoin
                    from province in provinceJoin.DefaultIfEmpty()
                    join commune in _context.Communes.AsNoTracking() on customer.CommuneId equals commune.Id into communeJoin
                    from commune in communeJoin.DefaultIfEmpty()
                    select new CustomerListItemResponse
                    {
                        Id = customer.Id,
                        UserName = customer.UserName ?? string.Empty,
                        FullName = customer.FullName,
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        AvatarUrl = customer.AvatarUrl,
                        ProvinceName = province != null ? province.Name : null,
                        CommuneName = commune != null ? commune.Name : null,
                        VipTier = customer.VipTier,
                        Status = customer.CustomerStatus,
                        LifetimeSpend = customer.LifetimeSpend,
                        OrderCount = customer.Orders.Count(o => !o.IsDeleted),
                        CreatedAt = customer.CreatedAt
                    };

        if (request.VipTier.HasValue) query = query.Where(x => x.VipTier == request.VipTier.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.ProvinceId.HasValue) query = query.Where(x => _context.Users.Any(u => u.Id == x.Id && u.ProvinceId == request.ProvinceId.Value));
        if (request.IsDeleted.HasValue) query = query.Where(x => _context.Users.Any(u => u.Id == x.Id && u.IsDeleted == request.IsDeleted.Value));

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(keyword) ||
                (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword)) ||
                x.UserName.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<CustomerListItemResponse>>.Success(new PagedResult<CustomerListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách khách hàng thành công.");
    }

    public async Task<ServiceResult<CustomerDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (customer is null)
        {
            return ServiceResult<CustomerDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy khách hàng.");
        }

        var province = customer.ProvinceId.HasValue
            ? await _context.Provinces.AsNoTracking().FirstOrDefaultAsync(x => x.Id == customer.ProvinceId.Value, cancellationToken)
            : null;
        var commune = customer.CommuneId.HasValue
            ? await _context.Communes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == customer.CommuneId.Value, cancellationToken)
            : null;

        var recentOrders = await _context.Orders
            .AsNoTracking()
            .Where(x => x.CustomerId == id && !x.IsDeleted)
            .OrderByDescending(x => x.OrderDate)
            .Take(10)
            .Select(x => new OrderListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                CustomerId = x.CustomerId,
                CustomerName = customer.FullName,
                CustomerPhone = customer.PhoneNumber ?? string.Empty,
                OrderDate = x.OrderDate,
                OrderStatus = x.OrderStatus,
                PaymentMethod = x.PaymentMethod,
                TotalAmount = x.TotalAmount,
                DiscountAmount = x.DiscountAmount,
                FinalAmount = x.FinalAmount,
                IsPaid = x.IsPaid,
                CouponCode = x.CouponCode,
                TotalItems = x.Details.Count(d => !d.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var addresses = await _context.CustomerAddresses
            .AsNoTracking()
            .Where(x => x.CustomerId == id && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .Select(x => new CustomerAddressResponse
            {
                Id = x.Id,
                RecipientName = x.RecipientName,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province!.Name,
                CommuneId = x.CommuneId,
                CommuneName = x.Commune!.Name,
                IsDefault = x.IsDefault,
                Note = x.Note
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<CustomerDetailResponse>.Success(new CustomerDetailResponse
        {
            Id = customer.Id,
            UserName = customer.UserName ?? string.Empty,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            AvatarUrl = customer.AvatarUrl,
            Address = customer.Address,
            ProvinceId = customer.ProvinceId,
            ProvinceName = province?.Name,
            CommuneId = customer.CommuneId,
            CommuneName = commune?.Name,
            Birthday = customer.Birthday,
            VipTier = customer.VipTier,
            Status = customer.CustomerStatus,
            LifetimeSpend = customer.LifetimeSpend,
            RewardPoints = customer.RewardPoints,
            PreferredCategory = customer.PreferredCategoryId.HasValue
                ? await _context.CommonCategories.Where(x => x.Id == customer.PreferredCategoryId.Value).Select(x => x.Name).FirstOrDefaultAsync(cancellationToken)
                : null,
            Note = customer.Note,
            IsActive = customer.IsActive,
            IsDeleted = customer.IsDeleted,
            CreatedAt = customer.CreatedAt,
            LastLoginAt = customer.LastLoginAt,
            RecentOrders = recentOrders,
            Addresses = addresses
        }, "Lấy chi tiết khách hàng thành công.");
    }

    public async Task<ServiceResult<CustomerDetailResponse>> CreateAsync(CreateCustomerRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (!ServiceHelpers.IsValidEmail(request.Email))
        {
            return ServiceResult<CustomerDetailResponse>.Failure(HttpStatusCode.BadRequest, "Email không hợp lệ.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            ProvinceId = request.ProvinceId,
            CommuneId = request.CommuneId,
            VipTier = request.VipTier,
            CustomerStatus = CustomerStatus.Active,
            Note = request.Note,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult<CustomerDetailResponse>.Failure(
                HttpStatusCode.BadRequest,
                "Không thể tạo khách hàng.",
                createResult.Errors.Select(x => x.Description).ToList());
        }

        if (await _userManager.FindByNameAsync("Customer") is not null)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
        }

        await WriteAuditAsync(operatorId, "Tạo khách hàng", "Customer", $"Tạo khách hàng {user.FullName}.");

        return await GetDetailAsync(user.Id, cancellationToken);
    }

    public async Task<ServiceResult<CustomerDetailResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var customer = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (customer is null)
        {
            return ServiceResult<CustomerDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy khách hàng.");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName)) customer.FullName = request.FullName.Trim();
        if (request.PhoneNumber is not null) customer.PhoneNumber = request.PhoneNumber;
        if (request.Address is not null) customer.Address = request.Address;
        if (request.ProvinceId.HasValue) customer.ProvinceId = request.ProvinceId;
        if (request.CommuneId.HasValue) customer.CommuneId = request.CommuneId;
        if (request.Birthday.HasValue) customer.Birthday = request.Birthday;
        if (request.VipTier.HasValue) customer.VipTier = request.VipTier.Value;
        if (request.Status.HasValue) customer.CustomerStatus = request.Status.Value;
        if (request.PreferredCategoryId.HasValue) customer.PreferredCategoryId = request.PreferredCategoryId;
        if (request.Note is not null) customer.Note = request.Note;
        if (request.RewardPoints.HasValue) customer.RewardPoints = request.RewardPoints.Value;

        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = operatorId.ToString();

        await _userManager.UpdateAsync(customer);

        await WriteAuditAsync(operatorId, "Cập nhật khách hàng", "Customer", $"Cập nhật khách hàng {customer.FullName}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var customer = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (customer is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy khách hàng.");
        }

        customer.IsDeleted = true;
        customer.CustomerStatus = CustomerStatus.Suspended;
        customer.IsActive = false;
        customer.DeletedAt = DateTime.UtcNow;
        customer.DeletedBy = operatorId.ToString();
        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedBy = operatorId.ToString();

        await _userManager.UpdateAsync(customer);

        await WriteAuditAsync(operatorId, "Xóa mềm khách hàng", "Customer", $"Xóa mềm khách hàng {customer.FullName}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm khách hàng thành công.");
    }

    public async Task<ServiceResult<bool>> SendVoucherAsync(SendVoucherRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.CustomerId && !x.IsDeleted, cancellationToken);
        if (customer is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy khách hàng.");
        }

        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Code == request.CouponCode.Trim().ToUpperInvariant() && !x.IsDeleted, cancellationToken);
        if (coupon is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Voucher không tồn tại.");
        }

        // Ghi nhận bằng audit log (thay thế cho gửi email/SMS thật)
        await WriteAuditAsync(operatorId, "Gửi voucher", "Customer", $"Gửi voucher {coupon.Code} cho khách hàng {customer.FullName}. Ghi chú: {request.Note}");

        return ServiceResult<bool>.Success(true, "Gửi voucher thành công.");
    }

    // ------------------------------------------------------------------ Địa chỉ

    public async Task<ServiceResult<IReadOnlyCollection<CustomerAddressResponse>>> GetAddressesAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await _context.CustomerAddresses
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new CustomerAddressResponse
            {
                Id = x.Id,
                RecipientName = x.RecipientName,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province!.Name,
                CommuneId = x.CommuneId,
                CommuneName = x.Commune!.Name,
                IsDefault = x.IsDefault,
                Note = x.Note
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<CustomerAddressResponse>>.Success(items, "Lấy danh sách địa chỉ thành công.");
    }

    public async Task<ServiceResult<CustomerAddressResponse>> CreateAddressAsync(Guid customerId, CreateCustomerAddressRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var address = new CustomerAddress
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            RecipientName = request.RecipientName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim(),
            ProvinceId = request.ProvinceId,
            CommuneId = request.CommuneId,
            IsDefault = request.IsDefault,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        if (address.IsDefault)
        {
            await _context.CustomerAddresses
                .Where(x => x.CustomerId == customerId && x.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDefault, false), cancellationToken);
        }

        _context.CustomerAddresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo địa chỉ", "Customer", $"Thêm địa chỉ cho khách hàng {customerId}.");

        var result = await _context.CustomerAddresses
            .AsNoTracking()
            .Where(x => x.Id == address.Id)
            .Select(x => new CustomerAddressResponse
            {
                Id = x.Id,
                RecipientName = x.RecipientName,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province!.Name,
                CommuneId = x.CommuneId,
                CommuneName = x.Commune!.Name,
                IsDefault = x.IsDefault,
                Note = x.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<CustomerAddressResponse>.Success(result!, "Tạo địa chỉ thành công.");
    }

    public async Task<ServiceResult<CustomerAddressResponse>> UpdateAddressAsync(Guid addressId, UpdateCustomerAddressRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var address = await _context.CustomerAddresses.FirstOrDefaultAsync(x => x.Id == addressId, cancellationToken);
        if (address is null)
        {
            return ServiceResult<CustomerAddressResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy địa chỉ.");
        }

        if (!string.IsNullOrWhiteSpace(request.RecipientName)) address.RecipientName = request.RecipientName.Trim();
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) address.PhoneNumber = request.PhoneNumber.Trim();
        if (!string.IsNullOrWhiteSpace(request.Address)) address.Address = request.Address.Trim();
        if (request.ProvinceId.HasValue) address.ProvinceId = request.ProvinceId.Value;
        if (request.CommuneId.HasValue) address.CommuneId = request.CommuneId.Value;
        if (request.Note is not null) address.Note = request.Note;

        if (request.IsDefault.HasValue && request.IsDefault.Value && !address.IsDefault)
        {
            await _context.CustomerAddresses
                .Where(x => x.CustomerId == address.CustomerId && x.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDefault, false), cancellationToken);
            address.IsDefault = true;
        }
        else if (request.IsDefault.HasValue)
        {
            address.IsDefault = request.IsDefault.Value;
        }

        address.UpdatedAt = DateTime.UtcNow;
        address.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.CustomerAddresses
            .AsNoTracking()
            .Where(x => x.Id == addressId)
            .Select(x => new CustomerAddressResponse
            {
                Id = x.Id,
                RecipientName = x.RecipientName,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province!.Name,
                CommuneId = x.CommuneId,
                CommuneName = x.Commune!.Name,
                IsDefault = x.IsDefault,
                Note = x.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<CustomerAddressResponse>.Success(result!, "Cập nhật địa chỉ thành công.");
    }

    public async Task<ServiceResult<bool>> DeleteAddressAsync(Guid addressId, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var address = await _context.CustomerAddresses.FirstOrDefaultAsync(x => x.Id == addressId, cancellationToken);
        if (address is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy địa chỉ.");
        }

        address.IsDeleted = true;
        address.DeletedAt = DateTime.UtcNow;
        address.DeletedBy = operatorId.ToString();
        address.UpdatedAt = DateTime.UtcNow;
        address.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa địa chỉ", "Customer", $"Xóa địa chỉ {addressId}.");

        return ServiceResult<bool>.Success(true, "Xóa địa chỉ thành công.");
    }

    private async Task WriteAuditAsync(Guid? operatorId, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = action,
            Module = module,
            Note = note,
            EntityType = module,
            IsSuccess = true
        });
    }

    // ------------------------------------------------------------------ PII & Voucher (FE: /customers/*)

    public async Task<ServiceResult<bool>> SendVoucherToCustomerAsync(Guid customerId, string couponCode, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Mã voucher không được để trống.");
        }

        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == customerId && !x.IsDeleted, cancellationToken);
        if (customer is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy khách hàng.");
        }

        var code = couponCode.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);
        if (coupon is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Voucher không tồn tại.");
        }

        if (coupon.Status != CouponStatus.Active)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Voucher không còn hiệu lực.");
        }

        await WriteAuditAsync(operatorId, "Gửi voucher", "Customer", $"Gửi voucher {coupon.Code} cho khách hàng {customer.FullName}.");

        return ServiceResult<bool>.Success(true, "Gửi voucher thành công.");
    }

    public async Task<ServiceResult<ExportPiiResponse>> ExportPiiAsync(ExportPiiRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var ids = request.Ids?.Where(x => x != Guid.Empty).Distinct().ToList() ?? [];

        var query = from customer in _context.Users.AsNoTracking()
                    join province in _context.Provinces.AsNoTracking() on customer.ProvinceId equals province.Id into provinceJoin
                    from province in provinceJoin.DefaultIfEmpty()
                    join commune in _context.Communes.AsNoTracking() on customer.CommuneId equals commune.Id into communeJoin
                    from commune in communeJoin.DefaultIfEmpty()
                    where !customer.IsDeleted
                    select new CustomerPiiRow
                    {
                        Id = customer.Id,
                        FullName = customer.FullName,
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        Address = customer.Address,
                        ProvinceName = province != null ? province.Name : null,
                        CommuneName = commune != null ? commune.Name : null,
                        Birthday = customer.Birthday
                    };

        if (ids.Count > 0)
        {
            query = query.Where(x => ids.Contains(x.Id));
        }

        var rows = await query.OrderBy(x => x.FullName).ToListAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xuất dữ liệu PII", "Customer", $"Xuất dữ liệu PII của {rows.Count} khách hàng.");

        return ServiceResult<ExportPiiResponse>.Success(new ExportPiiResponse
        {
            TotalCustomers = rows.Count,
            Rows = rows,
            ExportedAt = DateTime.UtcNow
        }, "Xuất dữ liệu PII thành công.");
    }
}
