import { BookCopyResponse, BookCopyStatus, BookListItemResponse, BookStatus } from './books.models';

type BookMock = Omit<BookListItemResponse, 'reviewCount' | 'soldCount' | 'isDeleted'>;
type BookCopyMock = Omit<BookCopyResponse, 'isDeleted' | 'currentOrderId'>;

const BOOK_MOCK_RAW: BookMock[] = [
  {
    id: 'b-01', code: 'BK-0001', title: 'Rừng Na Uy (Norwegian Wood)', subtitle: 'Bản Bia Cung - Phiên Bản giới hạn',
    slug: 'norwegianwood', isbn: '978-8499-1234-1', categoryId: 'c-01', categoryName: 'Văn học & Văn học đẳng',
    authorId: 'a-01', authorName: 'Haruki Murakami', publisherId: 'p-01', publisherName: 'Nha Nam',
    coverImageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAPScrewoz7rrV4fa42kde0E',
    priceForSell: 165000, priceForRent: 25000, pageCount: 384, totalQuantity: 48, stockForSell: 41,
    stockForRent: 7, status: BookStatus.Published, isActive: true, isFeatured: true,
    createdAt: '2025-01-12T08:00:00Z', updatedAt: '2025-09-30T03:20:00Z'
  },
  {
    id: 'b-02', code: 'BK-0002', title: 'Sapiens: Lược Sử Loài Người', subtitle: 'Một Lịch Sử Ngắn Gọn Toàn Cầu',
    slug: 'sapiens', isbn: '978-8499-1234-2', categoryId: 'c-02', categoryName: 'Khoa học & Xã hội',
    authorId: 'a-02', authorName: 'Yuval Noah Harari', publisherId: 'p-02', publisherName: 'NXB Tre',
    coverImageUrl: null, priceForSell: 218000, priceForRent: 30000, pageCount: 464, totalQuantity: 132,
    stockForSell: 128, stockForRent: 4, status: BookStatus.Published, isActive: true, isFeatured: true,
    createdAt: '2025-01-15T08:00:00Z', updatedAt: '2025-10-02T02:10:00Z'
  },
  {
    id: 'b-03', code: 'BK-0003', title: 'Clean Code: A Handbook of Agile Software Craftsmanship',
    subtitle: null, slug: 'clean-code', isbn: '978-0132350884', categoryId: 'c-03', categoryName: 'Công nghệ & Kinh doanh',
    authorId: 'a-03', authorName: 'Robert C. Martin', publisherId: 'p-03', publisherName: "O'Reilly Media",
    coverImageUrl: null, priceForSell: 420000, priceForRent: 60000, pageCount: 464, totalQuantity: 76,
    stockForSell: 76, stockForRent: 0, status: BookStatus.Published, isActive: true, isFeatured: false,
    createdAt: '2025-02-01T08:00:00Z', updatedAt: null
  },
  {
    id: 'b-04', code: 'BK-0004', title: '1Q84 Bộ Xế T Tròn Bộ 3 Tập', subtitle: 'Hộp Sơn Mai',
    slug: '1q84-boxset', isbn: '978-8499-1234-4', categoryId: 'c-01', categoryName: 'Văn học & Văn học đẳng',
    authorId: 'a-01', authorName: 'Haruki Murakami', publisherId: 'p-01', publisherName: 'Nha Nam',
    coverImageUrl: null, priceForSell: 495000, priceForRent: 70000, pageCount: 1440, totalQuantity: 32,
    stockForSell: 32, stockForRent: 0, status: BookStatus.Published, isActive: true, isFeatured: true,
    createdAt: '2025-02-11T08:00:00Z', updatedAt: null
  },
  {
    id: 'b-05', code: 'BK-0005', title: 'Những Ngày Ở Hiệu Sách Morisaki', subtitle: 'Nhật Bản Gốc',
    slug: 'days-at-the-morisaki-bookshop', isbn: '978-8499-1234-5', categoryId: 'c-01', categoryName: 'Văn học & Văn học đẳng',
    authorId: 'a-04', authorName: 'Akiyuki Nishida', publisherId: 'p-01', publisherName: 'Nha Nam',
    coverImageUrl: null, priceForSell: 98000, priceForRent: 15000, pageCount: 208, totalQuantity: 58,
    stockForSell: 52, stockForRent: 6, status: BookStatus.Published, isActive: true, isFeatured: false,
    createdAt: '2025-03-02T08:00:00Z', updatedAt: '2025-08-11T07:00:00Z'
  },
  {
    id: 'b-06', code: 'BK-0006', title: 'Hoá Thân (Die Verwandlung)', subtitle: 'Tiến Bản Dùng',
    slug: 'the-metamorphosis', isbn: '978-8499-1234-6', categoryId: 'c-01', categoryName: 'Văn học & Văn học đẳng',
    authorId: 'a-05', authorName: 'Franz Kafka', publisherId: 'p-02', publisherName: 'NXB Tre',
    coverImageUrl: null, priceForSell: 89000, priceForRent: 12000, pageCount: 176, totalQuantity: 41,
    stockForSell: 40, stockForRent: 1, status: BookStatus.Published, isActive: true, isFeatured: false,
    createdAt: '2025-03-20T08:00:00Z', updatedAt: null
  },
  {
    id: 'b-07', code: 'BK-0007', title: 'Dune: The Deluxe Chronicle', subtitle: null,
    slug: 'dune', isbn: '978-0441013593', categoryId: 'c-01', categoryName: 'Văn học & Văn học đẳng',
    authorId: 'a-06', authorName: 'Frank Herbert', publisherId: 'p-04', publisherName: 'Kim Đồng',
    coverImageUrl: null, priceForSell: 349000, priceForRent: 45000, pageCount: 896, totalQuantity: 64,
    stockForSell: 60, stockForRent: 4, status: BookStatus.Published, isActive: true, isFeatured: true,
    createdAt: '2025-04-05T08:00:00Z', updatedAt: null
  },
  {
    id: 'b-08', code: 'BK-0008', title: 'Đời Mâu Chiến Muôn Cai', subtitle: 'Hướng Dẫn Pháp Luật Mới',
    slug: 'doi-mau-chien-muon-cai', isbn: '978-8499-1234-8', categoryId: 'c-04', categoryName: 'Kinh doanh & Tài chính',
    authorId: 'a-07', authorName: 'Nguyễn Văn A', publisherId: 'p-02', publisherName: 'NXB Tre',
    coverImageUrl: null, priceForSell: 250000, priceForRent: 35000, pageCount: 320, totalQuantity: 9,
    stockForSell: 6, stockForRent: 3, status: BookStatus.Published, isActive: true, isFeatured: false,
    createdAt: '2025-04-22T08:00:00Z', updatedAt: null
  },
  {
    id: 'b-09', code: 'BK-0009', title: 'Atomic Habits', subtitle: 'An Easy & Proven Way to Build Good Habits',
    slug: 'atomic-habits', isbn: '978-0735211292', categoryId: 'c-05', categoryName: 'Tự nhân & Phát triển bản thân',
    authorId: 'a-08', authorName: 'James Clear', publisherId: 'p-03', publisherName: "O'Reilly Media",
    coverImageUrl: null, priceForSell: 275000, priceForRent: 40000, pageCount: 320, totalQuantity: 5,
    stockForSell: 5, stockForRent: 0, status: BookStatus.Published, isActive: true, isFeatured: false,
    createdAt: '2025-05-02T08:00:00Z', updatedAt: null
  },
  {
    id: 'b-10', code: 'BK-0010', title: 'No Longer Human', subtitle: 'Tân Viên Xe Khách',
    slug: 'no-longer-human', isbn: '978-8499-1234-0', categoryId: 'c-06', categoryName: 'Manga & Comic',
    authorId: 'a-09', authorName: 'Tatsuhiro Takato', publisherId: 'p-04', publisherName: 'Kim Đồng',
    coverImageUrl: null, priceForSell: 145000, priceForRent: 20000, pageCount: 256, totalQuantity: 0,
    stockForSell: 0, stockForRent: 0, status: BookStatus.Draft, isActive: false, isFeatured: false,
    createdAt: '2025-06-01T08:00:00Z', updatedAt: '2025-10-10T09:00:00Z'
  }
];

export const BOOK_MOCK_ROWS: BookListItemResponse[] = BOOK_MOCK_RAW.map((b) => ({
  ...b,
  reviewCount: 0,
  soldCount: 0,
  isDeleted: false
}));

const BOOK_COPY_MOCK_RAW: BookCopyMock[] = [
  { id: 'c-01-001', bookId: 'b-01', bookTitle: 'Rừng Na Uy (Norwegian Wood)', barcode: '8938512345671',
    conditionStatus: 'Moi', shelfCode: 'A-01-03', warehouseId: 'w-01', warehouseName: 'Kho Hanoi Chinh',
    acquiredDate: '2025-01-12', dueDate: null, returnedDate: null, status: BookCopyStatus.Available, notes: null },
  { id: 'c-01-002', bookId: 'b-01', bookTitle: 'Rừng Na Uy (Norwegian Wood)', barcode: '8938512345672',
    conditionStatus: 'Moi', shelfCode: 'A-01-03', warehouseId: 'w-01', warehouseName: 'Kho Hanoi Chinh',
    acquiredDate: '2025-01-12', dueDate: '2025-11-20', returnedDate: null, status: BookCopyStatus.Lent, notes: 'Doc trong ky muon' },
  { id: 'c-02-001', bookId: 'b-02', bookTitle: 'Sapiens: Lược Sử Loài Người', barcode: '8938512345689',
    conditionStatus: 'Moi', shelfCode: 'B-02-11', warehouseId: 'w-02', warehouseName: 'Kho Da Nang',
    acquiredDate: '2025-01-15', dueDate: null, returnedDate: null, status: BookCopyStatus.Available, notes: null },
  { id: 'c-08-001', bookId: 'b-08', bookTitle: 'Đời Mâu Chiến Muôn Cai', barcode: '8938512345696',
    conditionStatus: 'Da qua su dung', shelfCode: 'C-04-01', warehouseId: 'w-03', warehouseName: 'Kho HCM Nam',
    acquiredDate: '2025-04-22', dueDate: null, returnedDate: null, status: BookCopyStatus.Reserved, notes: 'Cho don BK-1042' },
  { id: 'c-10-001', bookId: 'b-10', bookTitle: 'No Longer Human', barcode: '8938512345702',
    conditionStatus: 'Hu hong', shelfCode: 'D-06-02', warehouseId: 'w-03', warehouseName: 'Kho HCM Nam',
    acquiredDate: '2025-06-01', dueDate: null, returnedDate: '2025-08-01', status: BookCopyStatus.Damaged, notes: 'Bia bong' }
];

export const BOOK_COPY_MOCK_ROWS: BookCopyResponse[] = BOOK_COPY_MOCK_RAW.map((c) => ({
  ...c,
  currentOrderId: null,
  isDeleted: false
}));
