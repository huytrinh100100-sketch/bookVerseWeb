namespace BookVerse.Core.Models.Geography;

public class ProvinceSearchRequest
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class ProvinceListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Kind { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CommuneCount { get; set; }
}

public class ProvinceDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Kind { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProvinceRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Kind { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProvinceRequest
{
    public string? Name { get; set; }
    public string? Kind { get; set; }
    public bool? IsActive { get; set; }
}

public class CommuneSearchRequest
{
    public string? Keyword { get; set; }
    public Guid? ProvinceId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class CommuneListItemResponse
{
    public Guid Id { get; set; }
    public Guid ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Kind { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class CommuneDetailResponse
{
    public Guid Id { get; set; }
    public Guid ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Kind { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCommuneRequest
{
    public Guid ProvinceId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Kind { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCommuneRequest
{
    public string? Name { get; set; }
    public string? Kind { get; set; }
    public bool? IsActive { get; set; }
}
