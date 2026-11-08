import { InventoryRowResponse, WarehouseResponse } from './inventory.models';

type WarehouseMock = Omit<WarehouseResponse, 'provinceId' | 'isDeleted' | 'copyCount' | 'createdAt'>;
type InventoryMock = Omit<InventoryRowResponse, 'coverImageUrl' | 'authorId' | 'publisherId' | 'categoryId' | 'categoryName' | 'priceForSell' | 'priceForRent' | 'availableCopies' | 'threshold' | 'isLowStock'>;

const WAREHOUSE_MOCK_RAW: WarehouseMock[] = [
  { id: 'w-01', code: 'HN', name: 'Kho Hanoi Chinh', address: 'So 1 Duong Vong, Hoang Mai, Hà Nội', provinceName: 'Hà Nội', isActive: true },
  { id: 'w-02', code: 'DN', name: 'Kho Da Nang', address: 'Duong 2/9, Hai Chau, Da Nang', provinceName: 'Da Nang', isActive: true },
  { id: 'w-03', code: 'HCM', name: 'Kho HCM Nam', address: '12 Nguyen Van Linh, Quan 4, Hồ Chí Minh', provinceName: 'Hồ Chí Minh', isActive: true }
];

export const WAREHOUSE_MOCK: WarehouseResponse[] = WAREHOUSE_MOCK_RAW.map((w) => ({
  ...w,
  provinceId: null,
  isDeleted: false,
  copyCount: 0,
  createdAt: '2025-01-01T00:00:00Z'
}));

const INVENTORY_MOCK_RAW: InventoryMock[] = [
  { bookId: 'b-01', title: 'Rừng Na Uy (Norwegian Wood)', isbn: '978-8499-1234-1', authorName: 'Haruki Murakami',
    publisherName: 'Nha Nam', warehouseId: 'w-01', warehouseName: 'Kho Hanoi Chinh', totalQuantity: 48,
    stockForSell: 41, stockForRent: 7, reservedQuantity: 2, lentQuantity: 4, velocity: 'fast', safetyStock: 10, reorderPoint: 15 },
  { bookId: 'b-02', title: 'Sapiens: Lược Sử Loài Người', isbn: '978-8499-1234-2', authorName: 'Yuval Noah Harari',
    publisherName: 'NXB Tre', warehouseId: 'w-02', warehouseName: 'Kho Da Nang', totalQuantity: 132,
    stockForSell: 128, stockForRent: 4, reservedQuantity: 6, lentQuantity: 1, velocity: 'fast', safetyStock: 20, reorderPoint: 40 },
  { bookId: 'b-03', title: 'Clean Code: A Handbook of Agile Software Craftsmanship', isbn: '978-0132350884',
    authorName: 'Robert C. Martin', publisherName: "O'Reilly Media", warehouseId: 'w-01', warehouseName: 'Kho Hanoi Chinh',
    totalQuantity: 76, stockForSell: 76, stockForRent: 0, reservedQuantity: 0, lentQuantity: 0, velocity: 'medium',
    safetyStock: 15, reorderPoint: 25 },
  { bookId: 'b-08', title: 'Đời Mâu Chiến Muôn Cai', isbn: '978-8499-1234-8', authorName: 'Nguyễn Văn A',
    publisherName: 'NXB Tre', warehouseId: 'w-03', warehouseName: 'Kho HCM Nam', totalQuantity: 9,
    stockForSell: 6, stockForRent: 3, reservedQuantity: 1, lentQuantity: 0, velocity: 'slow', safetyStock: 10, reorderPoint: 15 },
  { bookId: 'b-09', title: 'Atomic Habits', isbn: '978-0735211292', authorName: 'James Clear',
    publisherName: "O'Reilly Media", warehouseId: 'w-01', warehouseName: 'Kho Hanoi Chinh', totalQuantity: 5,
    stockForSell: 5, stockForRent: 0, reservedQuantity: 0, lentQuantity: 0, velocity: 'slow', safetyStock: 12, reorderPoint: 20 },
  { bookId: 'b-10', title: 'No Longer Human', isbn: '978-8499-1234-0', authorName: 'Tatsuhiro Takato',
    publisherName: 'Kim Đồng', warehouseId: 'w-03', warehouseName: 'Kho HCM Nam', totalQuantity: 0,
    stockForSell: 0, stockForRent: 0, reservedQuantity: 0, lentQuantity: 0, velocity: 'slow', safetyStock: 8, reorderPoint: 12 }
];

export const INVENTORY_MOCK_ROWS: InventoryRowResponse[] = INVENTORY_MOCK_RAW.map((r) => ({
  ...r,
  coverImageUrl: null,
  authorId: null,
  publisherId: null,
  categoryId: null,
  categoryName: null,
  priceForSell: 0,
  priceForRent: 0,
  availableCopies: r.stockForSell,
  threshold: r.reorderPoint,
  isLowStock: r.stockForSell <= r.reorderPoint
}));
