import { CustomerListItemResponse, CustomerStatus, VipTier } from './customers.models';

export const CUSTOMER_MOCK_ROWS: CustomerListItemResponse[] = [
  { id: 'cu-01', code: 'KH-0001', fullName: 'Trần Bảo Linh', email: 'linh.tran@bookverse.vn', phoneNumber: '0901234567',
    avatarUrl: null, provinceId: 'p-01', provinceName: 'Hà Nội', communeId: 'cm-01', communeName: 'Ly Nam De',
    address: 'So 12 Ly Nam De, So 5', vipTier: VipTier.Platinum, orderCount: 18, lifetimeSpend: 12580000,
    preferredCategory: 'Văn học & Văn học đẳng', status: CustomerStatus.Active, createdAt: '2024-03-11T08:00:00Z',
    lastOrderDate: '2025-11-03T09:12:00Z' },
  { id: 'cu-02', code: 'KH-0002', fullName: 'Nguyen Thi Hoa', email: 'hoa.nguyen@gmail.com', phoneNumber: '0912233445',
    avatarUrl: null, provinceId: 'p-02', provinceName: 'Hồ Chí Minh', communeId: 'cm-12', communeName: 'Phường 12',
    address: '45 Nguyen Van Linh, P12', vipTier: VipTier.Gold, orderCount: 9, lifetimeSpend: 6400000,
    preferredCategory: 'Kinh doanh & Tài chính', status: CustomerStatus.Active, createdAt: '2024-06-02T08:00:00Z',
    lastOrderDate: '2025-11-03T07:40:00Z' },
  { id: 'cu-03', code: 'KH-0003', fullName: 'Le Van Minh', email: 'minh.le@yahoo.com', phoneNumber: '0987654321',
    avatarUrl: null, provinceId: 'p-03', provinceName: 'Da Nang', communeId: 'cm-20', communeName: 'Hai Chau',
    address: '88 Hai Ba Trung', vipTier: VipTier.Silver, orderCount: 4, lifetimeSpend: 2100000,
    preferredCategory: 'Công nghệ & Kinh doanh', status: CustomerStatus.Active, createdAt: '2024-11-20T08:00:00Z',
    lastOrderDate: '2025-11-02T16:05:00Z' },
  { id: 'cu-04', code: 'KH-0004', fullName: 'Phạm Quỳnh Anh', email: 'quynhanh.pham@gmail.com', phoneNumber: '0968877665',
    avatarUrl: null, provinceId: 'p-02', provinceName: 'Hồ Chí Minh', communeId: 'cm-13', communeName: 'Phường 6',
    address: '170 Điện Biên Phủ, Q3', vipTier: VipTier.Platinum, orderCount: 23, lifetimeSpend: 18900000,
    preferredCategory: 'Văn học & Văn học đẳng', status: CustomerStatus.Active, createdAt: '2023-12-05T08:00:00Z',
    lastOrderDate: '2025-11-02T11:22:00Z' },
  { id: 'cu-05', code: 'KH-0005', fullName: 'Đỗ Anh Tuấn', email: 'tuan.do@gmail.com', phoneNumber: '0933445566',
    avatarUrl: null, provinceId: 'p-01', provinceName: 'Hà Nội', communeId: 'cm-30', communeName: 'Tran Phu',
    address: '9 Tran Phu', vipTier: VipTier.Standard, orderCount: 1, lifetimeSpend: 195000,
    preferredCategory: null, status: CustomerStatus.Suspended, createdAt: '2025-08-14T08:00:00Z',
    lastOrderDate: '2025-11-01T18:30:00Z' }
];
