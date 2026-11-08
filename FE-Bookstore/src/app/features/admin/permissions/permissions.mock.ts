import { PermissionResponse, RoleListItemResponse, StaffMemberResponse } from './permissions.models';

export const PERMISSION_MOCK_ROWS: PermissionResponse[] = [
  { id: 'pm-01', code: 'book.list', name: 'Xem danh sách sách', description: null, groupCode: 'book', groupName: 'Sách', sortOrder: 1, isSystem: true, isActive: true },
  { id: 'pm-02', code: 'book.create', name: 'Tạo sách', description: null, groupCode: 'book', groupName: 'Sách', sortOrder: 2, isSystem: true, isActive: true },
  { id: 'pm-03', code: 'book.edit', name: 'Sửa sách', description: null, groupCode: 'book', groupName: 'Sách', sortOrder: 3, isSystem: true, isActive: true },
  { id: 'pm-04', code: 'book.delete', name: 'Xóa sách', description: null, groupCode: 'book', groupName: 'Sách', sortOrder: 4, isSystem: true, isActive: true },
  { id: 'pm-05', code: 'book.export', name: 'Xuất dữ liệu sách', description: null, groupCode: 'book', groupName: 'Sách', sortOrder: 5, isSystem: true, isActive: true },
  { id: 'pm-06', code: 'category.list', name: 'Xem danh mục', description: null, groupCode: 'category', groupName: 'Danh mục', sortOrder: 1, isSystem: true, isActive: true },
  { id: 'pm-07', code: 'category.create', name: 'Tạo danh mục', description: null, groupCode: 'category', groupName: 'Danh mục', sortOrder: 2, isSystem: true, isActive: true },
  { id: 'pm-08', code: 'category.reorder', name: 'Sắp xếp lại cây danh mục', description: null, groupCode: 'category', groupName: 'Danh mục', sortOrder: 3, isSystem: true, isActive: true },
  { id: 'pm-09', code: 'order.list', name: 'Xem đơn hàng', description: null, groupCode: 'order', groupName: 'Đơn hàng', sortOrder: 1, isSystem: true, isActive: true },
  { id: 'pm-10', code: 'order.process', name: 'Xử lý đơn hàng', description: null, groupCode: 'order', groupName: 'Đơn hàng', sortOrder: 2, isSystem: true, isActive: true },
  { id: 'pm-11', code: 'order.refund', name: 'Duyệt hoàn tiền', description: null, groupCode: 'order', groupName: 'Đơn hàng', sortOrder: 3, isSystem: true, isActive: true },
  { id: 'pm-12', code: 'customer.list', name: 'Xem khách hàng', description: null, groupCode: 'customer', groupName: 'Khách hàng', sortOrder: 1, isSystem: true, isActive: true },
  { id: 'pm-13', code: 'customer.export-pii', name: 'Xuất dữ liệu PII', description: null, groupCode: 'customer', groupName: 'Khách hàng', sortOrder: 2, isSystem: true, isActive: true },
  { id: 'pm-14', code: 'coupon.create', name: 'Tạo voucher', description: null, groupCode: 'coupon', groupName: 'Khuyến mại', sortOrder: 1, isSystem: true, isActive: true },
  { id: 'pm-15', code: 'stock.adjust', name: 'Điều chỉnh tồn kho', description: null, groupCode: 'stock', groupName: 'Tồn kho', sortOrder: 1, isSystem: true, isActive: true },
  { id: 'pm-16', code: 'role.assign_permissions', name: 'Gán quyền', description: null, groupCode: 'role', groupName: 'Hệ thống', sortOrder: 1, isSystem: true, isActive: true }
];

export const ROLE_MOCK_ROWS: RoleListItemResponse[] = [
  { id: 'r-01', code: 'Admin', name: 'Quản trị viên tổng', description: 'Toàn quyền hệ thống',
    isSystem: true, isDefault: false, isActive: true, isDeleted: false, userCount: 2, permissionCount: PERMISSION_MOCK_ROWS.length,
    permissionCodes: PERMISSION_MOCK_ROWS.map((p) => p.code) },
  { id: 'r-02', code: 'Librarian', name: 'Thủ thủ', description: 'Quản lý catalog và tồn kho',
    isSystem: true, isDefault: false, isActive: true, isDeleted: false, userCount: 5,
    permissionCodes: ['book.list', 'book.create', 'book.edit', 'book.export', 'category.list', 'category.create',
      'category.update', 'category.reorder', 'stock.adjust', 'bookcopy.list', 'bookcopy.create', 'author.list', 'publisher.list'],
    permissionCount: 13 },
  { id: 'r-03', code: 'Staff', name: 'Nhân viên bán', description: 'Xử lý đơn hàng và khách hàng',
    isSystem: true, isDefault: false, isActive: true, isDeleted: false, userCount: 12,
    permissionCodes: ['book.list', 'order.list', 'order.view', 'order.create', 'order.process', 'customer.list',
      'customer.view', 'coupon.list', 'review.list'], permissionCount: 9 },
  { id: 'r-04', code: 'Customer', name: 'Khách hàng', description: 'Quyền mua sắm cơ bản',
    isSystem: true, isDefault: true, isActive: true, isDeleted: false, userCount: 1843,
    permissionCodes: ['shop.catalog', 'shop.cart', 'shop.checkout', 'shop.order', 'shop.wishlist', 'shop.review'], permissionCount: 6 }
];

export const STAFF_MOCK_ROWS: StaffMemberResponse[] = [
  { id: 'u-01', userName: 'admin', email: 'admin@bookverse.vn', fullName: 'Alex Nguyen',
    roleCodes: ['Admin'], twoFactorEnabled: true, lastLoginAt: '2025-11-05T08:12:00Z',
    lastLoginIp: '113.161.72.18', lastLoginLocation: 'Hà Nội, VN', status: 'Active' },
  { id: 'u-02', userName: 'thu.thu', email: 'thuthu@bookverse.vn', fullName: 'Mai Chi',
    roleCodes: ['Librarian'], twoFactorEnabled: false, lastLoginAt: '2025-11-05T07:40:00Z',
    lastLoginIp: '14.225.66.201', lastLoginLocation: 'Hồ Chí Minh, VN', status: 'Active' },
  { id: 'u-03', userName: 'nhanvien.ban', email: 'ban@bookverse.vn', fullName: 'Hoang Nam',
    roleCodes: ['Staff'], twoFactorEnabled: false, lastLoginAt: '2025-11-04T15:20:00Z',
    lastLoginIp: '123.24.55.90', lastLoginLocation: 'Da Nang, VN', status: 'Active' }
];
