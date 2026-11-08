using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Options;
using BookVerse.Infrastructure.Data;
using BookVerse.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookVerse.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        // ----- Hạ tầng -----
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPermissionCacheService, PermissionCacheService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // ----- Xác thực & phân quyền -----
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IHomeService, HomeService>();

        // ----- Catalog sách -----
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookCopyService, BookCopyService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPartnerService, PartnerService>();

        // ----- Bán hàng -----
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<ICustomerService, CustomerService>();

        // ----- Nội dung & storefront -----
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IStorefrontService, StorefrontService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISettingService, SettingService>();

        // ----- Địa giới -----
        services.AddScoped<IProvinceService, ProvinceService>();
        services.AddScoped<ICommuneService, CommuneService>();

        return services;
    }
}
