/**
 * Ma quyen (policy code) cua BookVerse - khop 1-1 voi BE PermissionConstants.
 * Nhom theo module, dung cho: menu, route guard (permissionGuard), directive *hasPermission.
 *
 * LUU Y: ma code phai giong het BE, vi BE kiem tra policy bang chuoi
 * (`options.AddPolicy(definition.Code, ...)`). Code sai se khong bao gio duoc cap quyen.
 */
export const PermissionConstants = {
  // ===== Trang chu / Ho so =====
  HomeView: 'home.view',
  ProfileView: 'profile.view',
  ProfileUpdate: 'profile.update',
  ProfileChangePassword: 'profile.change_password',

  // ===== Tai khoan =====
  AccountList: 'account.list',
  AccountView: 'account.view',
  AccountCreate: 'account.create',
  AccountUpdate: 'account.update',
  AccountLock: 'account.lock',
  AccountUnlock: 'account.unlock',
  AccountAssignRoles: 'account.assign_roles',
  AccountDeleteTemp: 'account.delete_temp',
  AccountDelete: 'account.delete',
  AccountRestore: 'account.restore',

  // ===== Vai tro & quyen =====
  RoleList: 'role.list',
  RoleView: 'role.view',
  RoleCreate: 'role.create',
  RoleUpdate: 'role.update',
  RoleAssignPermissions: 'role.assign_permissions',
  RoleDeleteTemp: 'role.delete_temp',
  RoleDelete: 'role.delete',
  RoleRestore: 'role.restore',
  PermissionList: 'permission.list',
  PermissionView: 'permission.view',

  // ===== Danh mục Catalog =====
  CategoryList: 'category.list',
  CategoryView: 'category.view',
  CategoryCreate: 'category.create',
  CategoryUpdate: 'category.update',
  CategoryDeleteTemp: 'category.delete_temp',
  CategoryDelete: 'category.delete',
  CategoryRestore: 'category.restore',
  CategoryReorder: 'category.reorder',

  // ===== Dia gioi =====
  ProvinceList: 'province.list',
  ProvinceView: 'province.view',
  ProvinceCreate: 'province.create',
  ProvinceUpdate: 'province.update',
  ProvinceDeleteTemp: 'province.delete_temp',
  ProvinceDelete: 'province.delete',
  ProvinceRestore: 'province.restore',
  ProvinceImport: 'province.import',
  CommuneList: 'commune.list',
  CommuneView: 'commune.view',
  CommuneCreate: 'commune.create',
  CommuneUpdate: 'commune.update',
  CommuneDeleteTemp: 'commune.delete_temp',
  CommuneDelete: 'commune.delete',
  CommuneRestore: 'commune.restore',
  CommuneImport: 'commune.import',

  // ===== Sach =====
  BookList: 'book.list',
  BookView: 'book.view',
  BookCreate: 'book.create',
  BookEdit: 'book.edit',
  BookDeleteTemp: 'book.delete_temp',
  BookDelete: 'book.delete',
  BookRestore: 'book.restore',
  BookExport: 'book.export',
  BookImport: 'book.import',
  StockAdjust: 'stock.adjust',

  // ===== Ban sao vat ly =====
  BookCopyList: 'bookcopy.list',
  BookCopyCreate: 'bookcopy.create',
  BookCopyEdit: 'bookcopy.edit',
  BookCopyDeleteTemp: 'bookcopy.delete_temp',
  BookCopyDelete: 'bookcopy.delete',

  // ===== Tác giả / NXB =====
  AuthorList: 'author.list',
  AuthorView: 'author.view',
  AuthorCreate: 'author.create',
  AuthorEdit: 'author.edit',
  AuthorDeleteTemp: 'author.delete_temp',
  AuthorDelete: 'author.delete',
  PublisherList: 'publisher.list',
  PublisherView: 'publisher.view',
  PublisherCreate: 'publisher.create',
  PublisherEdit: 'publisher.edit',
  PublisherDeleteTemp: 'publisher.delete_temp',
  PublisherDelete: 'publisher.delete',

  // ===== Kho hang =====
  WarehouseList: 'warehouse.list',
  WarehouseCreate: 'warehouse.create',
  WarehouseUpdate: 'warehouse.update',
  InventoryView: 'inventory.view',
  InventoryRestock: 'inventory.restock',

  // ===== Đơn hàng =====
  OrderList: 'order.list',
  OrderView: 'order.view',
  OrderCreate: 'order.create',
  OrderProcess: 'order.process',
  OrderCancel: 'order.cancel',
  OrderRefund: 'order.refund',
  OrderExport: 'order.export',

  // ===== Khach hang =====
  CustomerList: 'customer.list',
  CustomerView: 'customer.view',
  CustomerCreate: 'customer.create',
  CustomerEdit: 'customer.edit',
  CustomerExportPii: 'customer.export-pii',
  CustomerSendVoucher: 'customer.send-voucher',
  CustomerDeleteTemp: 'customer.delete_temp',

  // ===== Khuyen mai =====
  CouponList: 'coupon.list',
  CouponView: 'coupon.view',
  CouponCreate: 'coupon.create',
  CouponEdit: 'coupon.edit',
  CouponDeleteTemp: 'coupon.delete_temp',
  CouponDelete: 'coupon.delete',
  CampaignList: 'campaign.list',
  CampaignView: 'campaign.view',
  CampaignCreate: 'campaign.create',
  CampaignEdit: 'campaign.edit',
  CampaignDeleteTemp: 'campaign.delete_temp',

  // ===== Noi dung =====
  ReviewList: 'review.list',
  ReviewView: 'review.view',
  ReviewApprove: 'review.approve',
  ReviewReject: 'review.reject',
  ReviewDelete: 'review.delete',
  ArticleList: 'article.list',
  ArticleView: 'article.view',
  ArticleCreate: 'article.create',
  ArticleEdit: 'article.edit',
  ArticlePublish: 'article.publish',
  ArticleDelete: 'article.delete',

  // ===== He thong =====
  SettingView: 'setting.view',
  SettingEdit: 'setting.edit',
  AuditLogList: 'auditlog.list',
  DashboardView: 'dashboard.view',

  // ===== Storefront (khach mua) =====
  ShopCatalog: 'shop.catalog',
  ShopCart: 'shop.cart',
  ShopCheckout: 'shop.checkout',
  ShopOrder: 'shop.order',
  ShopWishlist: 'shop.wishlist',
  ShopReview: 'shop.review'
} as const;

export type PermissionCode = (typeof PermissionConstants)[keyof typeof PermissionConstants];

export const PERMISSION_GROUPS: { label: string; codes: PermissionCode[] }[] = [
  { label: 'Sách', codes: [PermissionConstants.BookList, PermissionConstants.BookView, PermissionConstants.BookCreate, PermissionConstants.BookEdit, PermissionConstants.BookDelete, PermissionConstants.BookExport, PermissionConstants.BookImport] },
  { label: 'Danh mục', codes: [PermissionConstants.CategoryList, PermissionConstants.CategoryCreate, PermissionConstants.CategoryUpdate, PermissionConstants.CategoryDelete, PermissionConstants.CategoryReorder] },
  { label: 'Tác giả / NXB', codes: [PermissionConstants.AuthorList, PermissionConstants.AuthorCreate, PermissionConstants.AuthorEdit, PermissionConstants.PublisherList, PermissionConstants.PublisherCreate, PermissionConstants.PublisherEdit] },
  { label: 'Tồn kho', codes: [PermissionConstants.BookCopyList, PermissionConstants.BookCopyCreate, PermissionConstants.BookCopyEdit, PermissionConstants.StockAdjust] },
  { label: 'Đơn hàng', codes: [PermissionConstants.OrderList, PermissionConstants.OrderView, PermissionConstants.OrderCreate, PermissionConstants.OrderProcess, PermissionConstants.OrderCancel, PermissionConstants.OrderRefund] },
  { label: 'Khách hàng', codes: [PermissionConstants.CustomerList, PermissionConstants.CustomerView, PermissionConstants.CustomerEdit, PermissionConstants.CustomerExportPii, PermissionConstants.CustomerSendVoucher] },
  { label: 'Khuyến mại', codes: [PermissionConstants.CouponList, PermissionConstants.CouponCreate, PermissionConstants.CouponEdit, PermissionConstants.CouponDelete, PermissionConstants.CampaignList] },
  { label: 'Nội dung', codes: [PermissionConstants.ReviewList, PermissionConstants.ReviewApprove, PermissionConstants.ReviewReject, PermissionConstants.ArticleList, PermissionConstants.ArticleCreate, PermissionConstants.ArticlePublish] },
  { label: 'Hệ thống', codes: [PermissionConstants.PermissionView, PermissionConstants.PermissionList, PermissionConstants.RoleList, PermissionConstants.RoleAssignPermissions, PermissionConstants.SettingEdit, PermissionConstants.AuditLogList] }
];
