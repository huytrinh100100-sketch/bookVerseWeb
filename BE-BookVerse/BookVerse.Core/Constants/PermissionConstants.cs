namespace BookVerse.Core.Constants;

public static class PermissionConstants
{
    // ===== Trang chủ / Hồ sơ =====
    public const string HomeView = "home.view";
    public const string ProfileView = "profile.view";
    public const string ProfileUpdate = "profile.update";
    public const string ProfileChangePassword = "profile.change_password";

    // ===== Tài khoản =====
    public const string AccountList = "account.list";
    public const string AccountView = "account.view";
    public const string AccountCreate = "account.create";
    public const string AccountUpdate = "account.update";
    public const string AccountLock = "account.lock";
    public const string AccountUnlock = "account.unlock";
    public const string AccountAssignRoles = "account.assign_roles";
    public const string AccountDeleteTemp = "account.delete_temp";
    public const string AccountDelete = "account.delete";
    public const string AccountRestore = "account.restore";

    // ===== Vai trò & quyền =====
    public const string RoleList = "role.list";
    public const string RoleView = "role.view";
    public const string RoleCreate = "role.create";
    public const string RoleUpdate = "role.update";
    public const string RoleAssignPermissions = "role.assign_permissions";
    public const string RoleDeleteTemp = "role.delete_temp";
    public const string RoleDelete = "role.delete";
    public const string RoleRestore = "role.restore";
    public const string PermissionList = "permission.list";
    public const string PermissionView = "permission.view";

    // ===== Danh mục động (Catalog) =====
    public const string CategoryList = "category.list";
    public const string CategoryView = "category.view";
    public const string CategoryCreate = "category.create";
    public const string CategoryUpdate = "category.update";
    public const string CategoryDeleteTemp = "category.delete_temp";
    public const string CategoryDelete = "category.delete";
    public const string CategoryRestore = "category.restore";
    public const string CategoryReorder = "category.reorder";

    // ===== Địa giới =====
    public const string ProvinceList = "province.list";
    public const string ProvinceView = "province.view";
    public const string ProvinceCreate = "province.create";
    public const string ProvinceUpdate = "province.update";
    public const string ProvinceDeleteTemp = "province.delete_temp";
    public const string ProvinceDelete = "province.delete";
    public const string ProvinceRestore = "province.restore";
    public const string ProvinceImport = "province.import";
    public const string CommuneList = "commune.list";
    public const string CommuneView = "commune.view";
    public const string CommuneCreate = "commune.create";
    public const string CommuneUpdate = "commune.update";
    public const string CommuneDeleteTemp = "commune.delete_temp";
    public const string CommuneDelete = "commune.delete";
    public const string CommuneRestore = "commune.restore";
    public const string CommuneImport = "commune.import";

    // ===== Sách =====
    public const string BookList = "book.list";
    public const string BookView = "book.view";
    public const string BookCreate = "book.create";
    public const string BookUpdate = "book.edit";
    public const string BookDeleteTemp = "book.delete_temp";
    public const string BookDelete = "book.delete";
    public const string BookRestore = "book.restore";
    public const string BookExport = "book.export";
    public const string BookImport = "book.import";
    public const string BookAdjustStock = "stock.adjust";

    // ===== Bản sao vật lý =====
    public const string BookCopyList = "bookcopy.list";
    public const string BookCopyCreate = "bookcopy.create";
    public const string BookCopyUpdate = "bookcopy.edit";
    public const string BookCopyDeleteTemp = "bookcopy.delete_temp";
    public const string BookCopyDelete = "bookcopy.delete";

    // ===== Tác giả / NXB =====
    public const string AuthorList = "author.list";
    public const string AuthorView = "author.view";
    public const string AuthorCreate = "author.create";
    public const string AuthorUpdate = "author.edit";
    public const string AuthorDeleteTemp = "author.delete_temp";
    public const string AuthorDelete = "author.delete";
    public const string PublisherList = "publisher.list";
    public const string PublisherView = "publisher.view";
    public const string PublisherCreate = "publisher.create";
    public const string PublisherUpdate = "publisher.edit";
    public const string PublisherDeleteTemp = "publisher.delete_temp";
    public const string PublisherDelete = "publisher.delete";

    // ===== Kho hàng =====
    public const string WarehouseList = "warehouse.list";
    public const string WarehouseCreate = "warehouse.create";
    public const string WarehouseUpdate = "warehouse.update";
    public const string InventoryView = "inventory.view";
    public const string InventoryRestock = "inventory.restock";

    // ===== Đơn hàng =====
    public const string OrderList = "order.list";
    public const string OrderView = "order.view";
    public const string OrderCreate = "order.create";
    public const string OrderProcess = "order.process";
    public const string OrderCancel = "order.cancel";
    public const string OrderRefund = "order.refund";
    public const string OrderExport = "order.export";

    // ===== Khách hàng =====
    public const string CustomerList = "customer.list";
    public const string CustomerView = "customer.view";
    public const string CustomerCreate = "customer.create";
    public const string CustomerUpdate = "customer.edit";
    public const string CustomerExportPii = "customer.export-pii";
    public const string CustomerSendVoucher = "customer.send-voucher";
    public const string CustomerDeleteTemp = "customer.delete_temp";

    // ===== Khuyến mãi =====
    public const string CouponList = "coupon.list";
    public const string CouponView = "coupon.view";
    public const string CouponCreate = "coupon.create";
    public const string CouponUpdate = "coupon.edit";
    public const string CouponDeleteTemp = "coupon.delete_temp";
    public const string CouponDelete = "coupon.delete";
    public const string CampaignList = "campaign.list";
    public const string CampaignView = "campaign.view";
    public const string CampaignCreate = "campaign.create";
    public const string CampaignUpdate = "campaign.edit";
    public const string CampaignDeleteTemp = "campaign.delete_temp";

    // ===== Nội dung =====
    public const string ReviewList = "review.list";
    public const string ReviewView = "review.view";
    public const string ReviewApprove = "review.approve";
    public const string ReviewReject = "review.reject";
    public const string ReviewDelete = "review.delete";
    public const string ArticleList = "article.list";
    public const string ArticleView = "article.view";
    public const string ArticleCreate = "article.create";
    public const string ArticleUpdate = "article.edit";
    public const string ArticlePublish = "article.publish";
    public const string ArticleDelete = "article.delete";

    // ===== Hệ thống =====
    public const string SettingView = "setting.view";
    public const string SettingUpdate = "setting.edit";
    public const string AuditLogList = "auditlog.list";
    public const string DashboardView = "dashboard.view";

    // ===== Storefront (khách mua) =====
    public const string ShopCatalog = "shop.catalog";
    public const string ShopCart = "shop.cart";
    public const string ShopCheckout = "shop.checkout";
    public const string ShopOrder = "shop.order";
    public const string ShopWishlist = "shop.wishlist";
    public const string ShopReview = "shop.review";

    public static IReadOnlyList<PermissionDefinition> GetAll()
    {
        return
        [
            new(HomeView, "Xem trang chủ", "home", "Trang chủ", 10, "Cho phép xem thông tin tổng quan sau đăng nhập"),
            new(ProfileView, "Xem hồ sơ cá nhân", "profile", "Hồ sơ cá nhân", 20),
            new(ProfileUpdate, "Cập nhật hồ sơ cá nhân", "profile", "Hồ sơ cá nhân", 21),
            new(ProfileChangePassword, "Đổi mật khẩu", "profile", "Hồ sơ cá nhân", 22),

            new(AccountList, "Xem danh sách tài khoản", "account", "Tài khoản", 30),
            new(AccountView, "Xem chi tiết tài khoản", "account", "Tài khoản", 31),
            new(AccountCreate, "Tạo tài khoản", "account", "Tài khoản", 32),
            new(AccountUpdate, "Cập nhật tài khoản", "account", "Tài khoản", 33),
            new(AccountLock, "Khóa tài khoản", "account", "Tài khoản", 34),
            new(AccountUnlock, "Mở khóa tài khoản", "account", "Tài khoản", 35),
            new(AccountAssignRoles, "Gán vai trò cho tài khoản", "account", "Tài khoản", 36),
            new(AccountDeleteTemp, "Xóa mềm tài khoản", "account", "Tài khoản", 37),
            new(AccountDelete, "Xóa vĩnh viễn tài khoản", "account", "Tài khoản", 38),
            new(AccountRestore, "Khôi phục tài khoản", "account", "Tài khoản", 39),

            new(RoleList, "Xem danh sách vai trò", "role", "Vai trò", 40),
            new(RoleView, "Xem chi tiết vai trò", "role", "Vai trò", 41),
            new(RoleCreate, "Tạo vai trò", "role", "Vai trò", 42),
            new(RoleUpdate, "Cập nhật vai trò", "role", "Vai trò", 43),
            new(RoleAssignPermissions, "Gán quyền cho vai trò", "role", "Vai trò", 44),
            new(RoleDeleteTemp, "Xóa mềm vai trò", "role", "Vai trò", 45),
            new(RoleDelete, "Xóa vĩnh viễn vai trò", "role", "Vai trò", 46),
            new(RoleRestore, "Khôi phục vai trò", "role", "Vai trò", 47),
            new(PermissionList, "Xem danh sách quyền", "permission", "Quyền", 50),
            new(PermissionView, "Xem chi tiết quyền", "permission", "Quyền", 51),

            new(CategoryList, "Xem danh sách danh mục", "category", "Danh mục Catalog", 60),
            new(CategoryView, "Xem chi tiết danh mục", "category", "Danh mục Catalog", 61),
            new(CategoryCreate, "Tạo danh mục", "category", "Danh mục Catalog", 62),
            new(CategoryUpdate, "Cập nhật danh mục", "category", "Danh mục Catalog", 63),
            new(CategoryDeleteTemp, "Xóa mềm danh mục", "category", "Danh mục Catalog", 64),
            new(CategoryDelete, "Xóa vĩnh viễn danh mục", "category", "Danh mục Catalog", 65),
            new(CategoryRestore, "Khôi phục danh mục", "category", "Danh mục Catalog", 66),
            new(CategoryReorder, "Sắp xếp lại cây danh mục", "category", "Danh mục Catalog", 67),

            new(ProvinceList, "Xem danh mục tỉnh/thành phố", "province", "Danh mục địa giới", 70),
            new(ProvinceView, "Xem chi tiết tỉnh/thành phố", "province", "Danh mục địa giới", 71),
            new(ProvinceCreate, "Tạo tỉnh/thành phố", "province", "Danh mục địa giới", 72),
            new(ProvinceUpdate, "Cập nhật tỉnh/thành phố", "province", "Danh mục địa giới", 73),
            new(ProvinceDeleteTemp, "Xóa mềm tỉnh/thành phố", "province", "Danh mục địa giới", 74),
            new(ProvinceDelete, "Xóa vĩnh viễn tỉnh/thành phố", "province", "Danh mục địa giới", 75),
            new(ProvinceRestore, "Khôi phục tỉnh/thành phố", "province", "Danh mục địa giới", 76),
            new(ProvinceImport, "Import tỉnh/thành phố", "province", "Danh mục địa giới", 77),
            new(CommuneList, "Xem danh mục xã/phường", "commune", "Danh mục địa giới", 80),
            new(CommuneView, "Xem chi tiết xã/phường", "commune", "Danh mục địa giới", 81),
            new(CommuneCreate, "Tạo xã/phường", "commune", "Danh mục địa giới", 82),
            new(CommuneUpdate, "Cập nhật xã/phường", "commune", "Danh mục địa giới", 83),
            new(CommuneDeleteTemp, "Xóa mềm xã/phường", "commune", "Danh mục địa giới", 84),
            new(CommuneDelete, "Xóa vĩnh viễn xã/phường", "commune", "Danh mục địa giới", 85),
            new(CommuneRestore, "Khôi phục xã/phường", "commune", "Danh mục địa giới", 86),
            new(CommuneImport, "Import xã/phường", "commune", "Danh mục địa giới", 87),

            new(BookList, "Xem danh sách sách", "book", "Sách", 100),
            new(BookView, "Xem chi tiết sách", "book", "Sách", 101),
            new(BookCreate, "Tạo sách", "book", "Sách", 102),
            new(BookUpdate, "Cập nhật sách", "book", "Sách", 103),
            new(BookDeleteTemp, "Xóa mềm sách", "book", "Sách", 104),
            new(BookDelete, "Xóa vĩnh viễn sách", "book", "Sách", 105),
            new(BookRestore, "Khôi phục sách", "book", "Sách", 106),
            new(BookExport, "Xuất danh sách sách", "book", "Sách", 107),
            new(BookImport, "Import sách từ file", "book", "Sách", 108),
            new(BookAdjustStock, "Điều chỉnh tồn kho sách", "book", "Sách", 109),

            new(BookCopyList, "Xem danh sách bản sao", "bookcopy", "Bản sao vật lý", 110),
            new(BookCopyCreate, "Tạo bản sao", "bookcopy", "Bản sao vật lý", 111),
            new(BookCopyUpdate, "Cập nhật bản sao", "bookcopy", "Bản sao vật lý", 112),
            new(BookCopyDeleteTemp, "Xóa mềm bản sao", "bookcopy", "Bản sao vật lý", 113),
            new(BookCopyDelete, "Xóa vĩnh viễn bản sao", "bookcopy", "Bản sao vật lý", 114),

            new(AuthorList, "Xem danh sách tác giả", "author", "Tác giả & NXB", 120),
            new(AuthorView, "Xem chi tiết tác giả", "author", "Tác giả & NXB", 121),
            new(AuthorCreate, "Tạo tác giả", "author", "Tác giả & NXB", 122),
            new(AuthorUpdate, "Cập nhật tác giả", "author", "Tác giả & NXB", 123),
            new(AuthorDeleteTemp, "Xóa mềm tác giả", "author", "Tác giả & NXB", 124),
            new(AuthorDelete, "Xóa vĩnh viễn tác giả", "author", "Tác giả & NXB", 125),
            new(PublisherList, "Xem danh sách nhà xuất bản", "publisher", "Tác giả & NXB", 130),
            new(PublisherView, "Xem chi tiết nhà xuất bản", "publisher", "Tác giả & NXB", 131),
            new(PublisherCreate, "Tạo nhà xuất bản", "publisher", "Tác giả & NXB", 132),
            new(PublisherUpdate, "Cập nhật nhà xuất bản", "publisher", "Tác giả & NXB", 133),
            new(PublisherDeleteTemp, "Xóa mềm nhà xuất bản", "publisher", "Tác giả & NXB", 134),
            new(PublisherDelete, "Xóa vĩnh viễn nhà xuất bản", "publisher", "Tác giả & NXB", 135),

            new(WarehouseList, "Xem danh sách kho hàng", "warehouse", "Tồn kho", 140),
            new(WarehouseCreate, "Tạo kho hàng", "warehouse", "Tồn kho", 141),
            new(WarehouseUpdate, "Cập nhật kho hàng", "warehouse", "Tồn kho", 142),
            new(InventoryView, "Xem báo cáo tồn kho", "inventory", "Tồn kho", 143),
            new(InventoryRestock, "Tạo phiếu nhập tồn kho", "inventory", "Tồn kho", 144),

            new(OrderList, "Xem danh sách đơn hàng", "order", "Đơn hàng", 150),
            new(OrderView, "Xem chi tiết đơn hàng", "order", "Đơn hàng", 151),
            new(OrderCreate, "Tạo đơn hàng", "order", "Đơn hàng", 152),
            new(OrderProcess, "Xử lý đơn hàng", "order", "Đơn hàng", 153),
            new(OrderCancel, "Hủy đơn hàng", "order", "Đơn hàng", 154),
            new(OrderRefund, "Duyệt hoàn tiền", "order", "Đơn hàng", 155),
            new(OrderExport, "Xuất danh sách đơn hàng", "order", "Đơn hàng", 156),

            new(CustomerList, "Xem danh sách khách hàng", "customer", "Khách hàng", 160),
            new(CustomerView, "Xem chi tiết khách hàng", "customer", "Khách hàng", 161),
            new(CustomerCreate, "Tạo khách hàng", "customer", "Khách hàng", 162),
            new(CustomerUpdate, "Cập nhật khách hàng", "customer", "Khách hàng", 163),
            new(CustomerExportPii, "Xuất dữ liệu PII khách hàng", "customer", "Khách hàng", 164),
            new(CustomerSendVoucher, "Gửi voucher cho khách hàng", "customer", "Khách hàng", 165),
            new(CustomerDeleteTemp, "Xóa mềm khách hàng", "customer", "Khách hàng", 166),

            new(CouponList, "Xem danh sách voucher", "coupon", "Khuyến mãi", 170),
            new(CouponView, "Xem chi tiết voucher", "coupon", "Khuyến mãi", 171),
            new(CouponCreate, "Tạo voucher", "coupon", "Khuyến mãi", 172),
            new(CouponUpdate, "Cập nhật voucher", "coupon", "Khuyến mãi", 173),
            new(CouponDeleteTemp, "Xóa mềm voucher", "coupon", "Khuyến mãi", 174),
            new(CouponDelete, "Xóa vĩnh viễn voucher", "coupon", "Khuyến mãi", 175),
            new(CampaignList, "Xem danh sách chiến dịch", "campaign", "Khuyến mãi", 180),
            new(CampaignView, "Xem chi tiết chiến dịch", "campaign", "Khuyến mãi", 181),
            new(CampaignCreate, "Tạo chiến dịch", "campaign", "Khuyến mãi", 182),
            new(CampaignUpdate, "Cập nhật chiến dịch", "campaign", "Khuyến mãi", 183),
            new(CampaignDeleteTemp, "Xóa mềm chiến dịch", "campaign", "Khuyến mãi", 184),

            new(ReviewList, "Xem danh sách đánh giá", "review", "Nội dung", 190),
            new(ReviewView, "Xem chi tiết đánh giá", "review", "Nội dung", 191),
            new(ReviewApprove, "Duyệt đánh giá", "review", "Nội dung", 192),
            new(ReviewReject, "Từ chối đánh giá", "review", "Nội dung", 193),
            new(ReviewDelete, "Xóa đánh giá", "review", "Nội dung", 194),
            new(ArticleList, "Xem danh sách bài viết", "article", "Nội dung", 200),
            new(ArticleView, "Xem chi tiết bài viết", "article", "Nội dung", 201),
            new(ArticleCreate, "Tạo bài viết", "article", "Nội dung", 202),
            new(ArticleUpdate, "Cập nhật bài viết", "article", "Nội dung", 203),
            new(ArticlePublish, "Xuất bản bài viết", "article", "Nội dung", 204),
            new(ArticleDelete, "Xóa bài viết", "article", "Nội dung", 205),

            new(SettingView, "Xem cấu hình hệ thống", "setting", "Hệ thống", 210),
            new(SettingUpdate, "Cập nhật cấu hình hệ thống", "setting", "Hệ thống", 211),
            new(AuditLogList, "Xem nhật ký hệ thống", "auditlog", "Hệ thống", 212),
            new(DashboardView, "Xem bảng tổng quan điều hành", "dashboard", "Hệ thống", 213),

            new(ShopCatalog, "Xem danh mục sách (storefront)", "shop", "Storefront", 220),
            new(ShopCart, "Dùng giỏ hàng", "shop", "Storefront", 221),
            new(ShopCheckout, "Thanh toán đơn hàng", "shop", "Storefront", 222),
            new(ShopOrder, "Xem đơn hàng của tôi", "shop", "Storefront", 223),
            new(ShopWishlist, "Dùng danh sách yêu thích", "shop", "Storefront", 224),
            new(ShopReview, "Viết đánh giá sách", "shop", "Storefront", 225)
        ];
    }
}
