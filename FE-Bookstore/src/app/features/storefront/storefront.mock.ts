import { BOOK_MOCK_ROWS } from '../admin/books/books.mock';
import { CustomerAddress, CartItem, CatalogItemResponse, CatalogResult, ShopCustomerProfile, ShopOrderSummary } from './storefront.models';
import { UnitType } from '../admin/books/books.models';
import { OrderStatus, PaymentMethod } from '../admin/orders/orders.models';
import { VipTier } from '../admin/customers/customers.models';

/**
 * Chuyen danh sach sach mock sang shape CatalogItemResponse cua BE
 * (CatalogItemResponse co them cac truong danh gia, gia goc va trang thai moi).
 */
const toCatalogItems = (rows: typeof BOOK_MOCK_ROWS): CatalogItemResponse[] =>
  rows.map((book, index) => ({
    ...book,
    pageCount: book.pageCount ?? 0,
    originalPrice: book.priceForSell,
    discountPercent: 0,
    language: 'Tiếng Việt',
    format: 'Paperback',
    averageRating: 4.5,
    reviewCount: 12 + index,
    soldCount: 40 - index * 3,
    isNew: index < 3
  }));

export const CATALOG_MOCK_RESULT: CatalogResult = {
  items: toCatalogItems(BOOK_MOCK_ROWS.filter((b) => b.status === 'Published')),
  totalItems: 214,
  page: 1,
  pageSize: 24,
  facets: {
    categories: [
      { categoryId: 'c-01', categoryName: 'Văn học & Văn học đẳng', count: 214 },
      { categoryId: 'c-03', categoryName: 'Công nghệ & Kinh doanh', count: 96 },
      { categoryId: 'c-02', categoryName: 'Khoa học & Xã hội', count: 68 },
      { categoryId: 'c-05', categoryName: 'Tự nhân & Phát triển bản thân', count: 50 },
      { categoryId: 'c-06', categoryName: 'Manga & Comic', count: 50 },
      { categoryId: 'c-04', categoryName: 'Kinh doanh & Tài chính', count: 12 }
    ],
    publishers: [
      { categoryId: 'p-01', categoryName: 'Nhã Nam', count: 142 },
      { categoryId: 'p-02', categoryName: 'Kim Đồng', count: 98 },
      { categoryId: 'p-03', categoryName: 'NXB Trẻ', count: 85 },
      { categoryId: 'p-04', categoryName: "O'Reilly Media", count: 34 }
    ],
    languages: [
      { categoryId: 'vi', categoryName: 'Tiếng Việt (Vietnamese)', count: 812 },
      { categoryId: 'en', categoryName: 'English (Original Imports)', count: 254 },
      { categoryId: 'bi', categoryName: 'Bilingual Editions', count: 76 }
    ],
    formats: [
      { categoryId: 'f-hard', categoryName: "Hardcover Collector's", count: 174 },
      { categoryId: 'f-paper', categoryName: 'Paperback Standard', count: 210 },
      { categoryId: 'f-box', categoryName: 'Boxset & Slipcase', count: 32 },
      { categoryId: 'f-signed', categoryName: 'Signed / First Edition', count: 12 }
    ],
    priceRanges: [
      { label: 'Dưới 150k', min: 0, max: 150000, count: 320 },
      { label: '150k - 300k', min: 150000, max: 300000, count: 486 },
      { label: 'Trên 300k', min: 300000, max: 0, count: 336 }
    ]
  }
};

export const CART_MOCK_ITEMS: CartItem[] = [
  { id: 'ci-01', bookId: 'b-01', title: 'Rừng Na Uy (Norwegian Wood)', coverImageUrl: BOOK_MOCK_ROWS[0].coverImageUrl,
    unitType: UnitType.Sell, unitPrice: 165000, quantity: 1, lineTotal: 165000, isWishlisted: true, availableStock: 41 },
  { id: 'ci-02', bookId: 'b-05', title: 'Những Ngày Ở Hiệu Sách Morisaki', coverImageUrl: null,
    unitType: UnitType.Sell, unitPrice: 98000, quantity: 1, lineTotal: 98000, isWishlisted: false, availableStock: 52 },
  { id: 'ci-03', bookId: 'b-03', title: 'Clean Code: A Handbook of Agile Software Craftsmanship', coverImageUrl: null,
    unitType: UnitType.Sell, unitPrice: 420000, quantity: 1, lineTotal: 420000, isWishlisted: false, availableStock: 76 }
];

export const ADDRESS_MOCK_ROWS: CustomerAddress[] = [
  { id: 'ad-01', recipientName: 'Trần Bảo Linh', phoneNumber: '0901234567',
    address: 'Số 12 Lý Nam Đế, Số 5', provinceId: 'p-01', provinceName: 'Hà Nội',
    communeId: 'cm-01', communeName: 'Lý Nam Đế', isDefault: true, note: 'Giao sau 18h' },
  { id: 'ad-02', recipientName: 'Trần Bảo Linh (VP)', phoneNumber: '0901234567',
    address: 'Tòa nhà Keangnam, Phường Trung, Nam Từ Liêm', provinceId: 'p-01', provinceName: 'Hà Nội',
    communeId: 'cm-45', communeName: 'Phường Trung', isDefault: false, note: null }
];

export const SHOP_ORDER_HISTORY_MOCK: ShopOrderSummary[] = [
  { id: 'o-1041', orderId: 'o-1041', code: 'BK-1041', orderCode: 'BK-1041', orderDate: '2025-11-03T09:12:00Z',
    totalAmount: 795000, finalAmount: 795000, shippingAmount: 0, discountAmount: 0,
    paymentMethod: PaymentMethod.MoMo, orderStatus: OrderStatus.Processing, itemCount: 3,
    previewTitles: ['Rừng Na Uy (Norwegian Wood)', 'Những Ngày Ở Hiệu Sách Morisaki'],
    trackingCode: 'GHN123456789VN', paymentUrl: null, message: 'Đơn hàng đã được tiếp nhận.' },
  { id: 'o-1038', orderId: 'o-1038', code: 'BK-1038', orderCode: 'BK-1038', orderDate: '2025-11-02T11:22:00Z',
    totalAmount: 1120000, finalAmount: 952000, shippingAmount: 0, discountAmount: 168000,
    paymentMethod: PaymentMethod.OnePay, orderStatus: OrderStatus.Completed, itemCount: 4,
    previewTitles: ['Sapiens: Lược Sử Loài Người'], trackingCode: 'GHTK987654321VN', paymentUrl: null, message: 'Đơn hàng đã hoàn thành.' },
  { id: 'o-1037', orderId: 'o-1037', code: 'BK-1037', orderCode: 'BK-1037', orderDate: '2025-11-01T18:30:00Z',
    totalAmount: 165000, finalAmount: 195000, shippingAmount: 30000, discountAmount: 0,
    paymentMethod: PaymentMethod.COD, orderStatus: OrderStatus.Cancelled, itemCount: 1,
    previewTitles: ['Rừng Na Uy (Norwegian Wood)'], trackingCode: null, paymentUrl: null, message: 'Đơn hàng đã bị hủy.' }
];

export const SHOP_PROFILE_MOCK: ShopCustomerProfile = {
  id: 'cu-01', fullName: 'Trần Bảo Linh', email: 'linh.tran@bookverse.vn', phoneNumber: '0901234567',
  avatarUrl: null, vipTier: VipTier.Platinum, rewardPoints: 2840, wishlistBookCount: 5, addressCount: 2
};
