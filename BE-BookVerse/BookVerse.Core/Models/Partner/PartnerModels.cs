using BookVerse.Core.Constants;
using BookVerse.Core.Models.Book;

namespace BookVerse.Core.Models.Partner;

public class PartnerSearchRequest
{
    public string? Keyword { get; set; }
    public PartnerType? Type { get; set; }
    public AgreementStatus? AgreementStatus { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>Danh sách tác giả (giữ route /authors cho tương thích).</summary>
public class PartnerListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PartnerType Type { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Biography { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LogoUrl { get; set; }
    public int BookCount { get; set; }
    public AgreementStatus AgreementStatus { get; set; }
    public DateTime? AgreementExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PartnerDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PartnerType Type { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Biography { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LogoUrl { get; set; }
    public int BookCount { get; set; }
    public AgreementStatus AgreementStatus { get; set; }
    public DateTime? AgreementExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyCollection<BookListItemResponse> Books { get; set; } = [];
}

public class CreatePartnerRequest
{
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public PartnerType Type { get; set; } = PartnerType.Author;
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Biography { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LogoUrl { get; set; }
    public AgreementStatus AgreementStatus { get; set; } = AgreementStatus.Draft;
    public DateTime? AgreementExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePartnerRequest
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Biography { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? LogoUrl { get; set; }
    public AgreementStatus? AgreementStatus { get; set; }
    public DateTime? AgreementExpiresAt { get; set; }
    public bool? IsActive { get; set; }
}

// ---- Alias cho tac gia -----
public class AuthorListItemResponse : PartnerListItemResponse { }
public class AuthorDetailResponse : PartnerDetailResponse { }
public class CreateAuthorRequest : CreatePartnerRequest { }
public class UpdateAuthorRequest : UpdatePartnerRequest { }

// ---- Alias cho nha xuat ban -----
public class PublisherListItemResponse : PartnerListItemResponse { }
public class PublisherDetailResponse : PartnerDetailResponse { }
public class CreatePublisherRequest : CreatePartnerRequest { }
public class UpdatePublisherRequest : UpdatePartnerRequest { }
