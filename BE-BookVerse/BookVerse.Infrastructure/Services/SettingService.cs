using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Setting;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class SettingService : ISettingService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public SettingService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<IReadOnlyCollection<SettingItemResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.StoreSettings
            .AsNoTracking()
            .OrderBy(x => x.GroupKey)
            .ThenBy(x => x.Key)
            .Select(x => new SettingItemResponse
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value,
                Description = x.Description,
                GroupKey = x.GroupKey,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<SettingItemResponse>>.Success(items, "Lấy cấu hình hệ thống thành công.");
    }

    public async Task<ServiceResult<SettingItemResponse>> UpdateAsync(UpdateSettingRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var setting = await _context.StoreSettings.FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken);
        if (setting is null)
        {
            return ServiceResult<SettingItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy cấu hình.");
        }

        setting.Value = request.Value;
        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = "Cập nhật cấu hình",
            Module = "Setting",
            Note = $"Cập nhật cấu hình {request.Key}.",
            EntityType = "Setting",
            IsSuccess = true
        });

        return ServiceResult<SettingItemResponse>.Success(new SettingItemResponse
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            GroupKey = setting.GroupKey,
            IsActive = setting.IsActive
        }, "Cập nhật cấu hình thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<PaymentGatewayResponse>>> GetPaymentGatewaysAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.PaymentGateways
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsEnabled)
            .OrderBy(x => x.Name)
            .Select(x => new PaymentGatewayResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Provider = x.Provider,
                IsEnabled = x.IsEnabled,
                IsTestMode = x.IsTestMode
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<PaymentGatewayResponse>>.Success(items, "Lấy danh sách cổng thanh toán thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<CarrierResponse>>> GetCarriersAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Carriers
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsEnabled)
            .OrderBy(x => x.Name)
            .Select(x => new CarrierResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Price = x.Price,
                EstimatedDays = x.EstimatedDays,
                IsEnabled = x.IsEnabled
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<CarrierResponse>>.Success(items, "Lấy danh sách đơn vị vận chuyển thành công.");
    }

    // ------------------------------------------------------------------ Cấu hình cửa hàng (FE: /settings/store)

    private const string StoreGroup = "store";

    public async Task<ServiceResult<StoreSettingResponse>> GetStoreAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _context.StoreSettings
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        var map = settings.ToDictionary(x => x.Key, x => x.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        var response = new StoreSettingResponse
        {
            StoreBrandName = GetValue(map, "store.brandName", GetValue(map, "store.name", "BookVerse")),
            SupportEmail = GetValue(map, "store.supportEmail", GetValue(map, "store.email", "support@bookverse.local")),
            Hotline = GetValue(map, "store.hotline", GetValue(map, "store.phone", "1900 1234")),
            HeadquartersAddress = GetValue(map, "store.headquartersAddress", GetValue(map, "store.address", string.Empty)),
            DefaultCurrency = GetValue(map, "store.defaultCurrency", "VND"),
            OperatingTimezone = GetValue(map, "store.operatingTimezone", "Asia/Ho_Chi_Minh"),
            FreeShippingThreshold = ParseDecimal(GetValue(map, "store.freeShippingThreshold", GetValue(map, "shipping.free_threshold", "500000"))),
            MaintenanceNotice = GetValue(map, "store.maintenanceNotice", string.Empty),
            StaffInactivityTimeoutMinutes = ParseInt(GetValue(map, "store.staffInactivityTimeoutMinutes", "30"))
        };

        var updatedAt = settings
            .Where(x => x.GroupKey == StoreGroup)
            .Select(x => (DateTime?)x.UpdatedAt)
            .Max();

        response.UpdatedAt = updatedAt;

        return ServiceResult<StoreSettingResponse>.Success(response, "Lấy cấu hình cửa hàng thành công.");
    }

    public async Task<ServiceResult<StoreSettingResponse>> UpdateStoreAsync(UpdateStoreSettingRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var pairs = new (string Key, string Value)[]
        {
            ("store.brandName", request.StoreBrandName?.Trim() ?? string.Empty),
            ("store.supportEmail", request.SupportEmail?.Trim() ?? string.Empty),
            ("store.hotline", request.Hotline?.Trim() ?? string.Empty),
            ("store.headquartersAddress", request.HeadquartersAddress?.Trim() ?? string.Empty),
            ("store.defaultCurrency", request.DefaultCurrency?.Trim() ?? "VND"),
            ("store.operatingTimezone", request.OperatingTimezone?.Trim() ?? "Asia/Ho_Chi_Minh"),
            ("store.freeShippingThreshold", request.FreeShippingThreshold.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("store.maintenanceNotice", request.MaintenanceNotice?.Trim() ?? string.Empty),
            ("store.staffInactivityTimeoutMinutes", request.StaffInactivityTimeoutMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };

        foreach (var (key, value) in pairs)
        {
            var setting = await _context.StoreSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

            if (setting is null)
            {
                _context.StoreSettings.Add(new StoreSetting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Description = key,
                    GroupKey = StoreGroup,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = operatorId.ToString()
                });

                continue;
            }

            setting.Value = value;
            setting.GroupKey = StoreGroup;
            setting.IsActive = true;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = operatorId.ToString();
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = "Cập nhật cấu hình cửa hàng",
            Module = "Setting",
            Note = "Cập nhật cấu hình cửa hàng.",
            EntityType = "StoreSetting",
            IsSuccess = true
        });

        return await GetStoreAsync(cancellationToken);
    }

    private static string GetValue(IReadOnlyDictionary<string, string> map, string key, string fallback)
    {
        return map.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
    }
}
