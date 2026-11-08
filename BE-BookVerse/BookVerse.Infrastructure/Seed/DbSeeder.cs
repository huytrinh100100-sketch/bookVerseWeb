using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookVerse.Infrastructure.Seed;

public static class DbSeeder
{
    /// <summary>
    /// Thứ tự seed (bắt buộc tuân thủ thứ tự phụ thuộc):
    /// 1. Migrate database
    /// 2. Roles (Admin, WarehouseManager, Customer)
    /// 3. Permissions (từ PermissionConstants)
    /// 4. Admin user
    /// 5. RolePermissions (Admin <- tất cả permissions)
    /// 6. UserInRole
    /// 7. Danh mục động (CommonCategories: book-category, article-category)
    /// 8. Địa giới (Provinces / Communes mẫu)
    /// 9. Tác giả / Nhà xuất bản
    /// 10. Kho hàng
    /// 11. Sách + bản sao
    /// 12. Voucher + chiến dịch
    /// 13. Bài viết
    /// 14. Cổng thanh toán / đơn vị vận chuyển / cấu hình
    /// 15. Khách hàng mẫu + đơn hàng mẫu
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await context.Database.MigrateAsync(cancellationToken);

        var adminRole = await EnsureAdminRoleAsync(roleManager);
        var customerRole = await EnsureCustomerRoleAsync(roleManager);
        var sellerRole = await EnsureSellerRoleAsync(roleManager);
        await EnsurePermissionsAsync(context, cancellationToken);
        await EnsureAdminUserAsync(userManager, configuration);
        await EnsureAdminRolePermissionsAsync(context, adminRole.Id, cancellationToken);
        await EnsureCustomerRolePermissionsAsync(context, customerRole.Id, cancellationToken);
        await EnsureSellerRolePermissionsAsync(context, sellerRole.Id, cancellationToken);
        await EnsureAdminUserInRoleAsync(userManager, roleManager, cancellationToken);

        await EnsureCategoriesAsync(context, cancellationToken);
        await EnsureGeographyAsync(context, cancellationToken);
        await EnsurePartnersAsync(context, cancellationToken);
        await EnsureWarehousesAsync(context, cancellationToken);
        await EnsureBooksAsync(context, cancellationToken);
        await EnsureCouponsAsync(context, cancellationToken);
        await EnsureArticlesAsync(context, cancellationToken);
        await EnsureSystemConfigsAsync(context, cancellationToken);
        await EnsureSampleCustomersAsync(context, userManager, cancellationToken);
        await EnsureSampleOrdersAsync(context, cancellationToken);
    }

    // ------------------------------------------------------------------ Roles

    private static async Task<ApplicationRole> EnsureAdminRoleAsync(RoleManager<ApplicationRole> roleManager)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(x => x.Name == "Admin");
        if (role is not null)
        {
            if (!role.IsActive || !role.IsSystem)
            {
                role.IsActive = true;
                role.IsSystem = true;
                role.Description = "Vai trò quản trị hệ thống";
                role.UpdatedAt = DateTime.UtcNow;
                await roleManager.UpdateAsync(role);
            }

            return role;
        }

        role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            NormalizedName = "ADMIN",
            Description = "Vai trò quản trị hệ thống",
            IsSystem = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Không thể tạo vai trò Admin: {string.Join("; ", result.Errors.Select(x => x.Description))}");
        }

        return role;
    }

    private static async Task<ApplicationRole> EnsureCustomerRoleAsync(RoleManager<ApplicationRole> roleManager)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(x => x.Name == "Customer");
        if (role is not null)
        {
            return role;
        }

        role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = "Customer",
            NormalizedName = "CUSTOMER",
            Description = "Khách hàng mua hàng trên cửa hàng",
            IsSystem = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Không thể tạo vai trò Customer: {string.Join("; ", result.Errors.Select(x => x.Description))}");
        }

        return role;
    }

    // ------------------------------------------------------------------ Permissions

    private static async Task EnsurePermissionsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var definitions = PermissionSeedData.GetAll();
        var existingByCode = await context.Permissions
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var definition in definitions)
        {
            if (existingByCode.TryGetValue(definition.Code, out var existing))
            {
                existing.Name = definition.Name;
                existing.GroupCode = definition.GroupCode;
                existing.GroupName = definition.GroupName;
                existing.Description = definition.Description;
                existing.SortOrder = definition.SortOrder;
                existing.IsActive = true;
                existing.UpdatedAt = now;
                continue;
            }

            context.Permissions.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Code = definition.Code,
                Name = definition.Name,
                GroupCode = definition.GroupCode,
                GroupName = definition.GroupName,
                Description = definition.Description,
                SortOrder = definition.SortOrder,
                IsActive = true,
                CreatedAt = now
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var adminPassword = configuration["SeedAdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Thiếu cấu hình SeedAdminPassword để seed tài khoản admin.");
        }

        var adminUser = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == "admin");
        if (adminUser is not null)
        {
            if (!adminUser.IsActive || adminUser.IsDeleted)
            {
                adminUser.IsActive = true;
                adminUser.IsDeleted = false;
                adminUser.UpdatedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(adminUser);
            }

            return;
        }

        adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@bookverse.local",
            NormalizedEmail = "ADMIN@BOOKVERSE.LOCAL",
            EmailConfirmed = true,
            FullName = "Quản trị hệ thống",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Không thể tạo tài khoản admin: {string.Join("; ", createResult.Errors.Select(x => x.Description))}");
        }
    }

    private static async Task EnsureAdminRolePermissionsAsync(AppDbContext context, Guid adminRoleId, CancellationToken cancellationToken)
    {
        var permissionIds = await context.Permissions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = (await context.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == adminRoleId)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        foreach (var permissionId in permissionIds.Where(id => !existingPermissionIds.Contains(id)))
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRoleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Vai trò người bán hàng: được vào back-office nhưng chỉ thấy các mục
    /// được cấp quyền (sách, đơn hàng, khách hàng, tồn kho).
    /// </summary>
    private static async Task<ApplicationRole> EnsureSellerRoleAsync(RoleManager<ApplicationRole> roleManager)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(x => x.Name == "Seller");
        if (role is not null)
        {
            return role;
        }

        role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = "Seller",
            NormalizedName = "SELLER",
            Description = "Nhân viên bán hàng, xử lý đơn hàng và danh mục sách",
            IsSystem = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "seed"
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Không thể tạo vai trò Seller: {string.Join("; ", result.Errors.Select(x => x.Description))}");
        }

        return role;
    }

    /// <summary>
    /// Khách hàng chỉ được các quyền storefront + xem đơn của chính mình.
    /// </summary>
    private static async Task EnsureCustomerRolePermissionsAsync(AppDbContext context, Guid customerRoleId, CancellationToken cancellationToken)
    {
        var shopCodes = new List<string>
        {
            PermissionConstants.ShopCatalog,
            PermissionConstants.ShopCart,
            PermissionConstants.ShopCheckout,
            PermissionConstants.ShopOrder,
            PermissionConstants.ShopWishlist,
            PermissionConstants.ShopReview,
            PermissionConstants.HomeView,
            PermissionConstants.ProfileView,
            PermissionConstants.ProfileUpdate,
            PermissionConstants.ProfileChangePassword
        };

        var permissionIds = await context.Permissions
            .AsNoTracking()
            .Where(x => shopCodes.Contains(x.Code))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = (await context.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == customerRoleId)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        foreach (var permissionId in permissionIds.Where(id => !existingPermissionIds.Contains(id)))
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = customerRoleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Người bán hàng: xem và xử lý sách, đơn hàng, khách hàng, tồn kho.
    /// Không có quyền quản trị (tài khoản, phân quyền, cấu hình, xóa vĩnh viễn).
    /// </summary>
    private static async Task EnsureSellerRolePermissionsAsync(AppDbContext context, Guid sellerRoleId, CancellationToken cancellationToken)
    {
        var sellerCodes = new List<string>
        {
            PermissionConstants.DashboardView,
            PermissionConstants.HomeView,
            PermissionConstants.ProfileView,
            PermissionConstants.ProfileUpdate,
            PermissionConstants.ProfileChangePassword,

            PermissionConstants.BookList,
            PermissionConstants.BookView,
            PermissionConstants.BookCreate,
            PermissionConstants.BookUpdate,
            PermissionConstants.BookExport,
            PermissionConstants.BookAdjustStock,

            PermissionConstants.CategoryList,
            PermissionConstants.CategoryView,

            PermissionConstants.AuthorList,
            PermissionConstants.AuthorView,
            PermissionConstants.PublisherList,
            PermissionConstants.PublisherView,

            PermissionConstants.OrderList,
            PermissionConstants.OrderView,
            PermissionConstants.OrderCreate,
            PermissionConstants.OrderProcess,
            PermissionConstants.OrderCancel,

            PermissionConstants.CustomerList,
            PermissionConstants.CustomerView,
            PermissionConstants.CustomerCreate,
            PermissionConstants.CustomerUpdate,

            PermissionConstants.BookCopyList,
            PermissionConstants.InventoryView,

            PermissionConstants.CouponList,
            PermissionConstants.CouponView,
            PermissionConstants.ReviewList,
            PermissionConstants.ReviewView,
            PermissionConstants.ArticleList,
            PermissionConstants.ArticleView,

            PermissionConstants.ShopCatalog,
            PermissionConstants.ShopCart,
            PermissionConstants.ShopCheckout,
            PermissionConstants.ShopOrder,
            PermissionConstants.ShopWishlist,
            PermissionConstants.ShopReview
        };

        var permissionIds = await context.Permissions
            .AsNoTracking()
            .Where(x => sellerCodes.Contains(x.Code))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = (await context.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == sellerRoleId)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        foreach (var permissionId in permissionIds.Where(id => !existingPermissionIds.Contains(id)))
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = sellerRoleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureAdminUserInRoleAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        CancellationToken cancellationToken)
    {
        var adminUser = await userManager.Users.FirstAsync(x => x.UserName == "admin", cancellationToken);
        var adminRole = await roleManager.Roles.FirstAsync(x => x.Name == "Admin", cancellationToken);

        if (!await userManager.IsInRoleAsync(adminUser, adminRole.Name!))
        {
            var result = await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Không thể gán vai trò Admin cho tài khoản admin: {string.Join("; ", result.Errors.Select(x => x.Description))}");
            }
        }
    }

    // ------------------------------------------------------------------ Danh mục

    private static async Task EnsureCategoriesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.CommonCategories.AnyAsync(x => x.Code == "book-category", cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Gốc: danh mục sách
        var bookCategoryRoot = new CommonCategory
        {
            Id = Guid.NewGuid(),
            Code = "book-category",
            Name = "Danh mục sách",
            Slug = "danh-muc-sach",
            RootId = Guid.Empty,
            SortOrder = 10,
            Description = "Cây danh mục dùng để phân loại sách trong cửa hàng",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = "seed"
        };
        bookCategoryRoot.RootId = bookCategoryRoot.Id;
        context.CommonCategories.Add(bookCategoryRoot);

        var bookCategories = new (string Code, string Name, int Sort)[]
        {
            ("fiction", "Văn học", 10),
            ("science", "Khoa học", 20),
            ("history", "Lịch sử", 30),
            ("business", "Kinh doanh", 40),
            ("children", "Thiếu nhi", 50),
            ("foreign", "Ngoại ngữ", 60),
            ("art", "Nghệ thuật & Thiết kế", 70),
            ("tech", "Công nghệ", 80)
        };

        foreach (var (code, name, sort) in bookCategories)
        {
            context.CommonCategories.Add(new CommonCategory
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Slug = name.ToLowerInvariant().Replace(" ", "-"),
                ParentId = bookCategoryRoot.Id,
                RootId = bookCategoryRoot.Id,
                SortOrder = sort,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "seed"
            });
        }

        // Gốc: danh mục bài viết
        var articleRoot = new CommonCategory
        {
            Id = Guid.NewGuid(),
            Code = "article-category",
            Name = "Chủ đề bài viết",
            Slug = "chu-de-bai-viet",
            RootId = Guid.Empty,
            SortOrder = 20,
            Description = "Cây danh mục dùng để phân loại bài viết blog",
            IsActive = true,
            CreatedAt = now,
            CreatedBy = "seed"
        };
        articleRoot.RootId = articleRoot.Id;
        context.CommonCategories.Add(articleRoot);

        foreach (var (code, name, sort) in new (string, string, int)[]
        {
            ("review", "Review sách", 10),
            ("news", "Tin tức", 20),
            ("guide", "Hướng dẫn", 30),
            ("author", "Tác giả", 40)
        })
        {
            context.CommonCategories.Add(new CommonCategory
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Slug = name.ToLowerInvariant().Replace(" ", "-"),
                ParentId = articleRoot.Id,
                RootId = articleRoot.Id,
                SortOrder = sort,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Địa giới

    private static async Task EnsureGeographyAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Provinces.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var provinces = new (string Code, string Name, string Kind)[]
        {
            ("HN", "Hà Nội", "Thành phố"),
            ("HCM", "Hồ Chí Minh", "Thành phố"),
            ("DN", "Đà Nẵng", "Thành phố"),
            ("HP", "Hải Phòng", "Thành phố"),
            ("CT", "Cần Thơ", "Thành phố")
        };

        foreach (var (code, name, kind) in provinces)
        {
            var province = new Province
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Kind = kind,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "seed"
            };
            context.Provinces.Add(province);

            context.Communes.Add(new Commune
            {
                Id = Guid.NewGuid(),
                ProvinceId = province.Id,
                Code = $"{code}-01",
                Name = $"Phường 1, {name}",
                Kind = "Phường",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Tác giả / NXB

    private static async Task EnsurePartnersAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        if (!await context.Authors.AnyAsync(cancellationToken))
        {
            var authors = new (string Code, string Name, string Country)[]
            {
                ("AUTH001", "Nguyễn Văn A", "Việt Nam"),
                ("AUTH002", "Trần Thị B", "Việt Nam"),
                ("AUTH003", "Lê Văn C", "Việt Nam"),
                ("AUTH004", "Haruki Murakami", "Nhật Bản"),
                ("AUTH005", "Paulo Coelho", "Brazil"),
                ("AUTH006", "Nguyễn Du", "Việt Nam")
            };

            foreach (var (code, name, country) in authors)
            {
                context.Authors.Add(new Author
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Name = name,
                    Country = country,
                    AgreementStatus = AgreementStatus.Active,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = "seed"
                });
            }
        }

        if (!await context.Publishers.AnyAsync(cancellationToken))
        {
            var publishers = new (string Code, string Name, string Country)[]
            {
                ("PUB001", "Nhà xuất bản Trẻ", "Việt Nam"),
                ("PUB002", "Nhà xuất bản Kim Đồng", "Việt Nam"),
                ("PUB003", "Nhà xuất bản Văn Học", "Việt Nam"),
                ("PUB004", "Thanh Niên Books", "Việt Nam")
            };

            foreach (var (code, name, country) in publishers)
            {
                context.Publishers.Add(new Publisher
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Name = name,
                    Country = country,
                    AgreementStatus = AgreementStatus.Active,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = "seed"
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Kho hàng

    private static async Task EnsureWarehousesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Warehouses.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var province = await context.Provinces.FirstOrDefaultAsync(cancellationToken);

        context.Warehouses.Add(new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = "KHO01",
            Name = "Kho hàng trung tâm",
            Address = "Số 1, đường Nguyễn Huệ",
            ProvinceId = province?.Id,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = "seed"
        });

        context.Warehouses.Add(new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = "KHO02",
            Name = "Kho chi nhánh phía Nam",
            Address = "Số 2, đường Lê Lợi",
            ProvinceId = province?.Id,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = "seed"
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Sách

    private static async Task EnsureBooksAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Books.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var authors = await context.Authors.ToListAsync(cancellationToken);
        var publishers = await context.Publishers.ToListAsync(cancellationToken);
        var categories = await context.CommonCategories.Where(x => x.Root!.Code == "book-category" && x.ParentId != null).ToListAsync(cancellationToken);
        var warehouse = await context.Warehouses.FirstOrDefaultAsync(cancellationToken);

        var books = new (string Code, string Title, string Author, string Publisher, string Category, decimal PriceSell, decimal PriceRent, int Pages, int Year, string Desc)[]
        {
            ("BK-000001", "Đại Vệ chí quốc gia", "AUTH006", "PUB003", "fiction", 85000m, 15000m, 320, 2019, "Tác phẩm kinh điển của Nguyễn Du, tác phẩm về hành trình và tâm hồn của nhân vật Tư Đầm."),
            ("BK-000002", "Lợi ích nhỏ", "AUTH001", "PUB003", "fiction", 92000m, 18000m, 280, 2020, "Câu chuyện về lòng tốt và những điều nhỏ bé thay đổi cuộc đời một con người."),
            ("BK-000003", "Cà phê cùng thiên hạ", "AUTH005", "PUB001", "fiction", 78000m, 14000m, 240, 2021, "Hành trình tìm kiếm bản thân qua những cuộc gặp gỡ tình cờ trên đường."),
            ("BK-000004", "Nguồn cơn khoa học", "AUTH002", "PUB002", "science", 125000m, 22000m, 450, 2022, "Nhập môn dễ hiểu về vật lý, hóa học và sinh học dành cho mọi đối tượng."),
            ("BK-000005", "Lịch sử Việt Nam", "AUTH003", "PUB003", "history", 110000m, 20000m, 520, 2018, "Tóm lược lịch sử Việt Nam qua các giai đoạn lịch sử trọng đại."),
            ("BK-000006", "Kinh doanh khởi nghiệp", "AUTH001", "PUB004", "business", 99000m, 17000m, 300, 2023, "Cẩm nang khởi nghiệp cho doanh nhân trẻ: mô hình, tài chính, marketing."),
            ("BK-000007", "Truyện cổ tích Việt", "AUTH006", "PUB002", "children", 65000m, 12000m, 180, 2017, "Tuyển tập truyện cổ tích dân gian Việt Nam với hình minh họa sinh động."),
            ("BK-000008", "Tiếng Anh thương mại", "AUTH003", "PUB001", "foreign", 135000m, 25000m, 400, 2023, "Bộ từ vựng và kỹ năng giao tiếp tiếng Anh trong môi trường doanh nghiệp."),
            ("BK-000009", "Thiết kế đồ hoạ", "AUTH002", "PUB004", "art", 158000m, 28000m, 260, 2024, "Nguyên lý thiết kế, bố cục, màu sắc và hệ thống thị giác trong đồ hoạ."),
            ("BK-000010", "Lập trình C# hiện đại", "AUTH001", "PUB004", "tech", 175000m, 32000m, 560, 2024, "Từ ngôn ngữ C# đến kiến trúc .NET hiện đại, kèm ví dụ thực tế."),
            ("BK-000011", "1Q84", "AUTH004", "PUB003", "fiction", 98000m, 18000m, 380, 2016, "Tiểu thuyết phi thuật về một thế giới song song và những mối quan hệ kỳ lạ."),
            ("BK-000012", "Nhà giả kim", "AUTH005", "PUB001", "fiction", 88000m, 16000m, 240, 2020, "Hành trình đi tìm vận mệnh của cậu bé nông trân từ Tây Ban Nha đến Ai Cập.")
        };

        foreach (var item in books)
        {
            var author = authors.FirstOrDefault(x => x.Code == item.Author);
            var publisher = publishers.FirstOrDefault(x => x.Code == item.Publisher);
            var category = categories.FirstOrDefault(x => x.Code == item.Category);

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Code = item.Code,
                Title = item.Title,
                Slug = item.Title.ToLowerInvariant().Replace(" ", "-"),
                Description = item.Desc,
                CoverImageUrl = $"https://placehold.co/400x560/{item.Category}/ffffff?text={Uri.EscapeDataString(item.Title)}",
                Isbn = $"978{1000000000 + int.Parse(item.Code[^3..]) * 7 + 123456789}",
                CategoryId = category?.Id,
                AuthorId = author?.Id,
                PublisherId = publisher?.Id,
                PriceForSell = item.PriceSell,
                PriceForRent = item.PriceRent,
                PageCount = item.Pages,
                PublicationYear = item.Year,
                Language = "Tiếng Việt",
                Format = "Bìa cứng",
                Status = BookStatus.Published,
                IsActive = true,
                IsFeatured = item.Code is "BK-000001" or "BK-000004" or "BK-000010" or "BK-000012",
                TotalQuantity = 20,
                StockForSell = 18,
                StockForRent = 8,
                AverageRating = 4.5m,
                ReviewCount = 12,
                SoldCount = 100 - int.Parse(item.Code[^3..]) * 7,
                CreatedAt = now.AddDays(-int.Parse(item.Code[^3..])),
                CreatedBy = "seed"
            };

            context.Books.Add(book);

            for (var i = 1; i <= 20; i++)
            {
                context.BookCopies.Add(new BookCopy
                {
                    Id = Guid.NewGuid(),
                    BookId = book.Id,
                    Barcode = $"BC-{book.Id.ToString("N")[..6].ToUpperInvariant()}-{i:D4}",
                    ConditionStatus = i <= 15 ? "Mới" : "Tốt",
                    ShelfCode = $"A-{i % 10:D1}-{i % 5:D1}",
                    WarehouseId = warehouse?.Id,
                    AcquiredDate = now.AddMonths(-3),
                    Status = i <= 8 ? BookCopyStatus.Available : (i <= 12 ? BookCopyStatus.Lent : BookCopyStatus.Available),
                    CreatedAt = now,
                    CreatedBy = "seed"
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Voucher

    private static async Task EnsureCouponsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Coupons.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Code = "CAMP001",
            Name = "Chiến dịch khai trương BookVerse",
            Description = "Chuỗi voucher ưu đãi dịp ra mắt cửa hàng",
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(60),
            IsActive = true,
            CreatedAt = now,
            CreatedBy = "seed"
        };
        context.Campaigns.Add(campaign);

        var coupons = new (string Code, string Name, DiscountType Type, decimal Value, decimal Min, decimal? Max, int Limit, CouponAudience Audience, CouponStatus Status)[]
        {
            ("WELCOME10", "Chào mừng giảm 10%", DiscountType.Percent, 10m, 0m, 50000m, 1000, CouponAudience.Public, CouponStatus.Active),
            ("FREESHIP", "Miễn phí vận chuyển", DiscountType.FreeShipping, 0m, 300000m, null, 500, CouponAudience.Public, CouponStatus.Active),
            ("SAVE50K", "Giảm trực tiếp 50.000đ", DiscountType.FixedAmount, 50000m, 500000m, null, 300, CouponAudience.Public, CouponStatus.Active),
            ("VIP20", "VIP giảm 20%", DiscountType.Percent, 20m, 0m, 200000m, 100, CouponAudience.VipOnly, CouponStatus.Active),
            ("NEWCUST", "Khách mới giảm 15%", DiscountType.Percent, 15m, 100000m, 80000m, 500, CouponAudience.NewCustomer, CouponStatus.Scheduled)
        };

        foreach (var item in coupons)
        {
            context.Coupons.Add(new Coupon
            {
                Id = Guid.NewGuid(),
                Code = item.Code,
                Name = item.Name,
                Description = item.Name,
                DiscountType = item.Type,
                DiscountValue = item.Value,
                MinOrderValue = item.Min,
                MaxDiscountAmount = item.Max,
                UsageLimit = item.Limit,
                UsedCount = 0,
                PerUserUsageLimit = 1,
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(90),
                Audience = item.Audience,
                Status = item.Status,
                CampaignId = campaign.Id,
                CreatedAt = now,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Bài viết

    private static async Task EnsureArticlesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Articles.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var author = await context.Authors.FirstOrDefaultAsync(cancellationToken);
        var categories = await context.CommonCategories.Where(x => x.Root!.Code == "article-category" && x.ParentId != null).ToListAsync(cancellationToken);

        var articles = new (string Code, string Title, string Summary, string Category)[]
        {
            ("ART001", "Top 10 cuốn sách hay nhất 2026", "Danh sách gợi ý những cuốn sách đáng đọc được cập nhật hàng quý.", "review"),
            ("ART002", "Hướng dẫn chọn sách tặng người thân", "Những nguyên tắc chọn sách quà tặng phù hợp với từng đối tượng.", "guide"),
            ("ART003", "BookVerse chính thức ra mắt", "Cửa hàng sách trực tuyến với kho hơn 10.000 cuốn sách.", "news")
        };

        foreach (var item in articles)
        {
            var category = categories.FirstOrDefault(x => x.Code == item.Category);

            context.Articles.Add(new Article
            {
                Id = Guid.NewGuid(),
                Code = item.Code,
                Title = item.Title,
                Slug = item.Title.ToLowerInvariant().Replace(" ", "-"),
                Summary = item.Summary,
                Content = $"{item.Summary}\n\nNội dung chi tiết của bài viết.",
                CoverImageUrl = "https://placehold.co/1200x630/article/ffffff?text=BookVerse",
                CategoryId = category?.Id,
                AuthorId = author?.Id ?? Guid.Empty,
                LinkedBookIds = string.Empty,
                SeoTitle = item.Title,
                SeoDescription = item.Summary,
                Status = ArticleStatus.Published,
                PublishedAt = now,
                CreatedAt = now,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Cấu hình hệ thống

    private static async Task EnsureSystemConfigsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        if (!await context.PaymentGateways.AnyAsync(cancellationToken))
        {
            var gateways = new (string Code, string Name, string Provider, string Methods)[]
            {
                ("COD", "Thanh toán khi nhận hàng", "BookVerse", "COD"),
                ("MOMO", "Ví MoMo", "MoMo", "MoMo"),
                ("VNPAY", "VNPay", "VNPay", "VNPay,QR"),
                ("ONEPAY", "OnePay", "OnePay", "OnePay,ATM")
            };

            foreach (var (code, name, provider, methods) in gateways)
            {
                context.PaymentGateways.Add(new PaymentGateway
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Name = name,
                    Provider = provider,
                    SupportedMethods = methods,
                    IsEnabled = true,
                    IsTestMode = true,
                    CreatedAt = now,
                    CreatedBy = "seed"
                });
            }
        }

        if (!await context.Carriers.AnyAsync(cancellationToken))
        {
            var carriers = new (string Code, string Name, decimal Price, int Days)[]
            {
                ("GHN", "Giao Hàng Nhanh", 30000m, 2),
                ("GHTK", "Giao Hàng Tiết Kiệm", 25000m, 4),
                ("VNPOST", "Bưu điện Việt Nam", 20000m, 6),
                ("STOREXPRESS", "Giao hàng nhanh BookVerse", 35000m, 2)
            };

            foreach (var (code, name, price, days) in carriers)
            {
                context.Carriers.Add(new Carrier
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Name = name,
                    Price = price,
                    EstimatedDays = days,
                    Description = $"Giao hàng qua {name}",
                    IsEnabled = true,
                    CreatedAt = now,
                    CreatedBy = "seed"
                });
            }
        }

        if (!await context.StoreSettings.AnyAsync(cancellationToken))
        {
            var settings = new (string Key, string? Value, string? Desc, string Group)[]
            {
                ("store.name", "BookVerse", "Tên cửa hàng", "general"),
                ("store.slogan", "Kho tri thức cho mọi người", "Khẩu hiệu", "general"),
                ("store.email", "support@bookverse.local", "Email hỗ trợ", "general"),
                ("store.phone", "1900 1234", "Số điện thoại hỗ trợ", "general"),
                ("store.address", "Số 1 Nguyễn Huệ, Hà Nội", "Địa chỉ cửa hàng", "general"),
                ("shipping.free_threshold", "500000", "Ngưỡng miễn phí vận chuyển", "shipping"),
                ("shipping.standard_fee", "30000", "Phí vận chuyển tiêu chuẩn", "shipping"),
                ("banner.khai-truong", "Khai trương giảm 10% toàn cửa hàng", "Banner khai trương", "banner"),
                ("banner.mua-1-tang-1", "Mua 1 tặng 1 cho đơn trên 500K", "Banner chương trình mua 1 tặng 1", "banner")
            };

            foreach (var (key, value, desc, group) in settings)
            {
                context.StoreSettings.Add(new StoreSetting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Description = desc,
                    GroupKey = group,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = "seed"
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Khách hàng & đơn hàng mẫu

    private static async Task EnsureSampleCustomersAsync(AppDbContext context, UserManager<ApplicationUser> userManager, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(x => x.UserName == "customer01", cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var province = await context.Provinces.FirstOrDefaultAsync(cancellationToken);
        var commune = await context.Communes.FirstOrDefaultAsync(cancellationToken);

        var samples = new (string UserName, string FullName, string Email, string Phone, Core.Constants.VipTier Tier, decimal Spend)[]
        {
            ("customer01", "Nguyễn Thị Lan", "lan.nguyen@bookverse.local", "0912345678", VipTier.Gold, 2500000m),
            ("customer02", "Trần Văn Bình", "binh.tran@bookverse.local", "0923456789", VipTier.Silver, 800000m),
            ("customer03", "Lê Thị Cúc", "cuc.le@bookverse.local", "0934567890", VipTier.Standard, 250000m)
        };

        foreach (var sample in samples)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = sample.UserName,
                NormalizedUserName = sample.UserName.ToUpperInvariant(),
                Email = sample.Email,
                NormalizedEmail = sample.Email.ToUpperInvariant(),
                EmailConfirmed = true,
                FullName = sample.FullName,
                PhoneNumber = sample.Phone,
                ProvinceId = province?.Id,
                CommuneId = commune?.Id,
                Address = "Số 10, đường Nguyễn Trãi",
                VipTier = sample.Tier,
                CustomerStatus = CustomerStatus.Active,
                LifetimeSpend = sample.Spend,
                RewardPoints = (int)(sample.Spend / 10000m),
                IsActive = true,
                CreatedAt = now.AddDays(-30),
                CreatedBy = "seed"
            };

            var result = await userManager.CreateAsync(user, "Customer@123");
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, "Customer");

            context.CustomerAddresses.Add(new CustomerAddress
            {
                Id = Guid.NewGuid(),
                CustomerId = user.Id,
                RecipientName = sample.FullName,
                PhoneNumber = sample.Phone,
                Address = "Số 10, đường Nguyễn Trãi",
                ProvinceId = province?.Id ?? Guid.Empty,
                CommuneId = commune?.Id ?? Guid.Empty,
                IsDefault = true,
                CreatedAt = now,
                CreatedBy = "seed"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureSampleOrdersAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Orders.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var customers = await context.Users.Where(x => x.UserName != null && x.UserName.StartsWith("customer")).ToListAsync(cancellationToken);
        var books = await context.Books.Where(x => x.IsActive).ToListAsync(cancellationToken);
        var coupon = await context.Coupons.FirstOrDefaultAsync(x => x.Code == "WELCOME10", cancellationToken);

        if (customers.Count == 0 || books.Count == 0)
        {
            return;
        }

        var statuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Paid,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Completed
        };

        var sequence = 1;

        foreach (var customer in customers)
        {
            for (var i = 0; i < 3; i++)
            {
                var orderDate = now.AddDays(-(i * 7 + 1));
                var status = statuses[(sequence - 1) % statuses.Length];
                var isPaid = status != OrderStatus.Pending;

                var selectedBooks = books.Skip(sequence % 5).Take(2).ToList();

                var total = selectedBooks.Sum(b => b.PriceForSell);
                var discount = coupon is not null ? Math.Round(total * 0.10m, 2) : 0m;
                var shipping = total - discount >= 500000m ? 0m : 30000m;

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    Code = $"OD-{orderDate:yyyyMMdd}-{sequence:D4}",
                    CustomerId = customer.Id,
                    CouponId = coupon?.Id,
                    CouponCode = coupon?.Code,
                    OrderDate = orderDate,
                    OrderStatus = status,
                    PaymentMethod = PaymentMethod.COD,
                    TotalAmount = total,
                    DiscountAmount = discount,
                    ShippingAmount = shipping,
                    FinalAmount = total - discount + shipping,
                    IsPaid = isPaid,
                    PaidAt = isPaid ? orderDate.AddHours(1) : null,
                    ShippingAddress = customer.Address,
                    ProvinceId = customer.ProvinceId,
                    CommuneId = customer.CommuneId,
                    CreatedAt = orderDate,
                    CreatedBy = "seed"
                };

                context.Orders.Add(order);

                foreach (var book in selectedBooks)
                {
                    context.OrderDetails.Add(new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        BookId = book.Id,
                        UnitType = UnitType.Sell,
                        Quantity = 1,
                        UnitPrice = book.PriceForSell,
                        DiscountAmount = 0,
                        LineTotal = book.PriceForSell,
                        BookTitle = book.Title,
                        IsbnSnapshot = book.Isbn,
                        CoverImageUrl = book.CoverImageUrl,
                        CreatedAt = orderDate,
                        CreatedBy = "seed"
                    });
                }

                context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    FromStatus = OrderStatus.Pending,
                    ToStatus = status,
                    Note = "Seed dữ liệu mẫu.",
                    ChangedAt = orderDate
                });

                sequence++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
