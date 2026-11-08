import { CouponListItemResponse, CouponStatus, CouponAudience, DiscountType } from './promotions.models';

export const COUPON_MOCK_ROWS: CouponListItemResponse[] = [
  { id: 'cp-01', code: 'FREESHIPHANOI', name: 'Free ship Hà Nội', description: 'Mien phi van chuyen khu vuc Hà Nội noi bo.',
    discountType: DiscountType.FreeShipping, discountValue: 30000, minOrderValue: 200000, maxDiscountAmount: null,
    usageLimit: 1000, usedCount: 742, perUserUsageLimit: 2, startDate: '2025-01-01', endDate: '2025-12-31',
    audience: CouponAudience.Public, status: CouponStatus.Active, createdAt: '2024-12-20T08:00:00Z' },
  { id: 'cp-02', code: 'VIPPLATINUM15', name: 'VIP Platinum 15%', description: 'Giam 15% cho khach VIP Platinum.',
    discountType: DiscountType.Percent, discountValue: 15, minOrderValue: 500000, maxDiscountAmount: 150000,
    usageLimit: 500, usedCount: 218, perUserUsageLimit: 3, startDate: '2025-06-01', endDate: '2026-05-31',
    audience: CouponAudience.VipOnly, status: CouponStatus.Active, createdAt: '2025-05-20T08:00:00Z' },
  { id: 'cp-03', code: 'BOOKLOVER30K', name: 'Book lover -30k', description: 'Giam truc tiep 30.000 VND cho don tu 300k.',
    discountType: DiscountType.FixedAmount, discountValue: 30000, minOrderValue: 300000, maxDiscountAmount: 30000,
    usageLimit: 2000, usedCount: 1683, perUserUsageLimit: 5, startDate: '2025-03-01', endDate: '2025-11-30',
    audience: CouponAudience.Public, status: CouponStatus.Active, createdAt: '2025-02-20T08:00:00Z' },
  { id: 'cp-04', code: 'WELCOME50', name: 'Chao moi 50k', description: 'Voucher cho khach moi, giam 50k don tu 200k.',
    discountType: DiscountType.FixedAmount, discountValue: 50000, minOrderValue: 200000, maxDiscountAmount: 50000,
    usageLimit: 5000, usedCount: 3210, perUserUsageLimit: 1, startDate: '2025-01-01', endDate: '2026-12-31',
    audience: CouponAudience.NewCustomer, status: CouponStatus.Active, createdAt: '2024-12-30T08:00:00Z' },
  { id: 'cp-05', code: 'FLASH2412', name: 'Flash sale 24/12', description: 'Flash deal 24/12 giam 20% toi da 200k.',
    discountType: DiscountType.Percent, discountValue: 20, minOrderValue: 400000, maxDiscountAmount: 200000,
    usageLimit: 300, usedCount: 0, perUserUsageLimit: 1, startDate: '2025-12-24', endDate: '2025-12-25',
    audience: CouponAudience.Public, status: CouponStatus.Scheduled, createdAt: '2025-10-15T08:00:00Z' }
];
