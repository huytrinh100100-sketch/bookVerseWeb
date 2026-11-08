namespace BookVerse.Core.Models.Setting;

public class SettingItemResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string GroupKey { get; set; } = "general";
    public bool IsActive { get; set; }
}

public class UpdateSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}

/// <summary>FE: GET/PUT /settings/store.</summary>
public class StoreSettingResponse
{
    public string StoreBrandName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string Hotline { get; set; } = string.Empty;
    public string HeadquartersAddress { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "VND";
    public string OperatingTimezone { get; set; } = "Asia/Ho_Chi_Minh";
    public decimal FreeShippingThreshold { get; set; }
    public string MaintenanceNotice { get; set; } = string.Empty;
    public int StaffInactivityTimeoutMinutes { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateStoreSettingRequest
{
    public string StoreBrandName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string Hotline { get; set; } = string.Empty;
    public string HeadquartersAddress { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "VND";
    public string OperatingTimezone { get; set; } = "Asia/Ho_Chi_Minh";
    public decimal FreeShippingThreshold { get; set; }
    public string MaintenanceNotice { get; set; } = string.Empty;
    public int StaffInactivityTimeoutMinutes { get; set; }
}

public class PaymentGatewayResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsTestMode { get; set; }
    public IReadOnlyCollection<string> SupportedMethods { get; set; } = [];
}

public class CarrierResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int EstimatedDays { get; set; }
    public bool IsEnabled { get; set; }
}
