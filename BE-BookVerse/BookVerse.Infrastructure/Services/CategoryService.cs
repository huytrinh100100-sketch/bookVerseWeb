using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Category;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public CategoryService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<CategoryListItemResponse>>> SearchAsync(CategorySearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = from category in _context.CommonCategories.AsNoTracking()
                    join parent in _context.CommonCategories.AsNoTracking() on category.ParentId equals parent.Id into parentJoin
                    from parent in parentJoin.DefaultIfEmpty()
                    join root in _context.CommonCategories.AsNoTracking() on category.RootId equals root.Id
                    select new CategoryListItemResponse
                    {
                        Id = category.Id,
                        Code = category.Code,
                        Name = category.Name,
                        Slug = category.Slug,
                        RootId = category.RootId,
                        RootCode = root.Code,
                        RootName = root.Name,
                        Type = root.Code,
                        TypeLabel = root.Name,
                        ParentId = category.ParentId,
                        ParentName = parent != null ? parent.Name : null,
                        SortOrder = category.SortOrder,
                        Description = category.Description,
                        IsMegaMenu = category.IsMegaMenu,
                        IsActive = category.IsActive,
                        IsDeleted = category.IsDeleted,
                        CreatedAt = category.CreatedAt
                    };

        if (!string.IsNullOrWhiteSpace(request.RootCode))
        {
            query = query.Where(x => x.RootCode == request.RootCode);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.IsDeleted.HasValue)
        {
            query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(keyword) ||
                x.Name.ToLower().Contains(keyword) ||
                x.RootName.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.RootName)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<CategoryListItemResponse>>.Success(new PagedResult<CategoryListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách danh mục thành công.");
    }

    public async Task<ServiceResult<CategoryDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.CommonCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy danh mục.");
        }

        var root = await _context.CommonCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == category.RootId, cancellationToken);
        var parent = category.ParentId.HasValue
            ? await _context.CommonCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == category.ParentId.Value, cancellationToken)
            : null;

        return ServiceResult<CategoryDetailResponse>.Success(new CategoryDetailResponse
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Slug = category.Slug,
            RootId = category.RootId,
            RootCode = root?.Code,
            RootName = root?.Name,
            ParentId = category.ParentId,
            ParentName = parent?.Name,
            SortOrder = category.SortOrder,
            Description = category.Description,
            IconUrl = category.IconUrl,
            IsMegaMenu = category.IsMegaMenu,
            IsActive = category.IsActive,
            IsDeleted = category.IsDeleted,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        }, "Lấy chi tiết danh mục thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<CategoryTreeNodeResponse>>> GetTreeAsync(string? rootCode = null, CancellationToken cancellationToken = default)
    {
        var query = from category in _context.CommonCategories.AsNoTracking()
                    join root in _context.CommonCategories.AsNoTracking() on category.RootId equals root.Id
                    select new { category, root };

        if (!string.IsNullOrWhiteSpace(rootCode))
        {
            var normalizedRootCode = rootCode.Trim();
            query = query.Where(x => x.root.Code == normalizedRootCode);
        }

        var nodes = await query
            .OrderBy(x => x.root.SortOrder)
            .ThenBy(x => x.root.Name)
            .ThenBy(x => x.category.SortOrder)
            .ThenBy(x => x.category.Name)
            .Select(x => new CategoryTreeNodeResponse
            {
                Id = x.category.Id,
                Code = x.category.Code,
                Name = x.category.Name,
                Slug = x.category.Slug,
                RootId = x.category.RootId,
                RootCode = x.root.Code,
                RootName = x.root.Name,
                Type = x.root.Code,
                TypeLabel = x.root.Name,
                ParentId = x.category.ParentId,
                SortOrder = x.category.SortOrder,
                IsMegaMenu = x.category.IsMegaMenu,
                IsActive = x.category.IsActive,
                IsDeleted = x.category.IsDeleted
            })
            .ToListAsync(cancellationToken);

        // So sach theo tung danh muc (chi tinh sach chua xoa).
        var bookCounts = await _context.Books
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.CategoryId != null)
            .GroupBy(x => x.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        foreach (var node in nodes)
        {
            node.BookCount = bookCounts.TryGetValue(node.Id, out var count) ? count : 0;
        }

        return ServiceResult<IReadOnlyCollection<CategoryTreeNodeResponse>>.Success(BuildTree(nodes), "Lấy cây danh mục thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<CategoryRootResponse>>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        var roots = await _context.CommonCategories
            .AsNoTracking()
            .Where(x => x.ParentId == null && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CategoryRootResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                SortOrder = x.SortOrder
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<CategoryRootResponse>>.Success(roots, "Lấy danh sách gốc danh mục thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<CategoryListItemResponse>>> GetItemsByRootCodeAsync(string rootCode, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = from category in _context.CommonCategories.AsNoTracking()
                    join root in _context.CommonCategories.AsNoTracking() on category.RootId equals root.Id
                    where root.Code == rootCode
                    select new CategoryListItemResponse
                    {
                        Id = category.Id,
                        Code = category.Code,
                        Name = category.Name,
                        Slug = category.Slug,
                        RootId = category.RootId,
                        RootCode = root.Code,
                        RootName = root.Name,
                        Type = root.Code,
                        TypeLabel = root.Name,
                        ParentId = category.ParentId,
                        SortOrder = category.SortOrder,
                        Description = category.Description,
                        IsMegaMenu = category.IsMegaMenu,
                        IsActive = category.IsActive,
                        IsDeleted = category.IsDeleted,
                        CreatedAt = category.CreatedAt
                    };

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive && !x.IsDeleted);
        }

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<CategoryListItemResponse>>.Success(items, "Lấy danh mục theo nhóm thành công.");
    }

    public async Task<ServiceResult<CategoryDetailResponse>> CreateAsync(CreateCategoryRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        Guid rootId;
        Guid? parentId = request.ParentId;

        if (parentId.HasValue)
        {
            var parent = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == parentId.Value, cancellationToken);
            if (parent is null)
            {
                return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.BadRequest, "Danh mục cha không tồn tại.");
            }

            rootId = parent.RootId;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RootCode))
            {
                return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.BadRequest, "Cần cung cấp RootCode khi tạo danh mục gốc.");
            }

            var rootCode = request.RootCode.Trim();
            var existingRoot = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Code == rootCode && x.ParentId == null, cancellationToken);
            if (existingRoot is not null)
            {
                return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã danh mục gốc đã tồn tại.");
            }

            var root = new CommonCategory
            {
                Id = Guid.NewGuid(),
                Code = rootCode,
                Name = request.Name.Trim(),
                Slug = ServiceHelpers.Slugify(request.Name),
                ParentId = null,
                RootId = Guid.Empty,
                SortOrder = request.SortOrder,
                Description = request.Description,
                IconUrl = request.IconUrl,
                IsMegaMenu = request.IsMegaMenu,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId.ToString()
            };

            root.RootId = root.Id;
            _context.CommonCategories.Add(root);
            await _context.SaveChangesAsync(cancellationToken);

            await WriteAuditAsync(operatorId, "Tạo danh mục gốc", "Category", $"Tạo danh mục gốc {root.Name} ({root.Code}).");

            return await GetDetailAsync(root.Id, cancellationToken);
        }

        var duplicate = await _context.CommonCategories.AnyAsync(x => x.RootId == rootId && x.Code == request.Code.Trim() && !x.IsDeleted, cancellationToken);
        if (duplicate)
        {
            return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã danh mục đã tồn tại trong cùng nhóm.");
        }

        var category = new CommonCategory
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Slug = string.IsNullOrWhiteSpace(request.Slug) ? ServiceHelpers.Slugify(request.Name) : request.Slug.Trim(),
            ParentId = parentId,
            RootId = rootId,
            SortOrder = request.SortOrder,
            Description = request.Description,
            IconUrl = request.IconUrl,
            IsMegaMenu = request.IsMegaMenu,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.CommonCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo danh mục", "Category", $"Tạo danh mục {category.Name} ({category.Code}).");

        return await GetDetailAsync(category.Id, cancellationToken);
    }

    public async Task<ServiceResult<CategoryDetailResponse>> UpdateAsync(Guid id, UpdateCategoryRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var category = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy danh mục.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            category.Name = request.Name.Trim();
        }

        if (request.Slug is not null)
        {
            category.Slug = request.Slug.Trim();
        }

        if (request.SortOrder.HasValue) category.SortOrder = request.SortOrder.Value;
        if (request.Description is not null) category.Description = request.Description;
        if (request.IconUrl is not null) category.IconUrl = request.IconUrl;
        if (request.IsMegaMenu.HasValue) category.IsMegaMenu = request.IsMegaMenu.Value;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật danh mục", "Category", $"Cập nhật danh mục {category.Name}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<CategoryDetailResponse>> MoveAsync(Guid id, MoveCategoryRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var category = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy danh mục.");
        }

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
            {
                return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.BadRequest, "Không thể di chuyển danh mục vào chính nó.");
            }

            var newParent = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == request.ParentId.Value, cancellationToken);
            if (newParent is null)
            {
                return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.BadRequest, "Danh mục cha không tồn tại.");
            }

            if (await IsDescendantAsync(category.Id, newParent.RootId, cancellationToken))
            {
                return ServiceResult<CategoryDetailResponse>.Failure(HttpStatusCode.BadRequest, "Không thể di chuyển danh mục vào nhánh con của chính nó.");
            }

            category.ParentId = newParent.Id;
            category.RootId = newParent.RootId;
        }
        else
        {
            category.ParentId = null;
            category.RootId = id;
        }

        if (request.SortOrder.HasValue) category.SortOrder = request.SortOrder.Value;

        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Di chuyển danh mục", "Category", $"Di chuyển danh mục {category.Name}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, UpdateCategoryStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var category = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy danh mục.");
        }

        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Đổi trạng thái danh mục", "Category", $"Đổi trạng thái danh mục {category.Name}.");

        return ServiceResult<bool>.Success(true, "Cập nhật trạng thái danh mục thành công.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var category = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy danh mục.");
        }

        var hasChildren = await _context.CommonCategories.AnyAsync(x => x.ParentId == id && !x.IsDeleted, cancellationToken);
        if (hasChildren)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Không thể xóa danh mục đang có danh mục con.");
        }

        category.IsDeleted = true;
        category.IsActive = false;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = operatorId.ToString();
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa mềm danh mục", "Category", $"Xóa mềm danh mục {category.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm danh mục thành công.");
    }

    public async Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var category = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy danh mục.");
        }

        var isUsed = await _context.Books.AnyAsync(x => x.CategoryId == id, cancellationToken);
        if (isUsed)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Danh mục đang được sử dụng bởi sách, không thể xóa vĩnh viễn.");
        }

        _context.CommonCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa vĩnh viễn danh mục", "Category", $"Xóa vĩnh viễn danh mục {category.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa vĩnh viễn danh mục thành công.");
    }

    public async Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var category = await _context.CommonCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy danh mục.");
        }

        category.IsDeleted = false;
        category.DeletedAt = null;
        category.DeletedBy = null;
        category.IsActive = true;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Khôi phục danh mục", "Category", $"Khôi phục danh mục {category.Name}.");

        return ServiceResult<bool>.Success(true, "Khôi phục danh mục thành công.");
    }

    public async Task<ServiceResult<bool>> ReorderAsync(ReorderCategoriesRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Danh sách sắp xếp rỗng.");
        }

        var ids = request.Items.Select(x => x.Id).ToList();
        var categories = await _context.CommonCategories.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            var category = categories.FirstOrDefault(x => x.Id == item.Id);
            if (category is null)
            {
                continue;
            }

            category.SortOrder = item.SortOrder;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = operatorId.ToString();
        }

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Sắp xếp danh mục", "Category", $"Sắp xếp lại {request.Items.Count} danh mục.");

        return ServiceResult<bool>.Success(true, "Sắp xếp danh mục thành công.");
    }

    private async Task<bool> IsDescendantAsync(Guid categoryId, Guid candidateRootId, CancellationToken cancellationToken = default)
    {
        if (categoryId == candidateRootId)
        {
            return true;
        }

        var category = await _context.CommonCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
        if (category is null)
        {
            return false;
        }

        var children = await _context.CommonCategories.AsNoTracking()
            .Where(x => x.ParentId == categoryId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var childId in children)
        {
            if (await IsDescendantAsync(childId, candidateRootId, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyCollection<CategoryTreeNodeResponse> BuildTree(IReadOnlyCollection<CategoryTreeNodeResponse> nodes)
    {
        var map = nodes.ToDictionary(x => x.Id, x => new CategoryTreeNodeResponse
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Slug = x.Slug,
            RootId = x.RootId,
            RootCode = x.RootCode,
            RootName = x.RootName,
            Type = x.Type,
            TypeLabel = x.TypeLabel,
            ParentId = x.ParentId,
            SortOrder = x.SortOrder,
            IsMegaMenu = x.IsMegaMenu,
            IsActive = x.IsActive,
            IsDeleted = x.IsDeleted,
            BookCount = x.BookCount
        });

        var roots = new List<CategoryTreeNodeResponse>();

        foreach (var node in nodes)
        {
            if (node.ParentId.HasValue && map.TryGetValue(node.ParentId.Value, out var parent))
            {
                parent.Children = parent.Children.Append(map[node.Id]).ToList();
            }
            else
            {
                roots.Add(map[node.Id]);
            }
        }

        // Node gốc hiển thị tổng số sách của toàn bộ nhánh con.
        foreach (var root in roots)
        {
            root.BookCount = root.BookCount + root.Children.Sum(child => child.BookCount);
        }

        return roots;
    }

    private async Task WriteAuditAsync(Guid? operatorId, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = action,
            Module = module,
            Note = note,
            EntityType = "Category",
            IsSuccess = true
        });
    }
}
