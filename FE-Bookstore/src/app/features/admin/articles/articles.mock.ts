import { ArticleListItemResponse, ArticleStatus } from './articles.models';

type ArticleMock = Omit<ArticleListItemResponse, 'isDeleted' | 'updatedAt'>;

const ARTICLE_MOCK_RAW: ArticleMock[] = [
  { id: 'ar-01', code: 'ART-0001', title: 'Haruki Murakami: The Melancholy of Urban Solitude', slug: 'haruki-murakami-melancholy',
    summary: 'Mot bai phan tich ve tri duyet va thanh pho trong trieu tac pham cua Murakami.',
    coverImageUrl: null, categoryId: 'c-01', categoryName: 'Phong van tac gia', linkedBookIds: ['b-01', 'b-04'],
    linkedBookTitles: ['Rừng Na Uy (Norwegian Wood)', '1Q84 Bộ Xế T Tròn Bộ 3 Tập'], authorName: 'Mai Chi',
    editorName: 'Hoang Nam', viewCount: 24800, conversionCount: 412, status: ArticleStatus.Published,
    publishedAt: '2025-10-20T02:00:00Z', createdAt: '2025-10-18T08:00:00Z' },
  { id: 'ar-02', code: 'ART-0002', title: '10 Cuốn Sách Về Kỹ Thuật Đáng Để Học', slug: '10-cuon-sach-ky-thuat',
    summary: 'Danh sach goi y cho nguoi moi nhap nghe lap trinh.', coverImageUrl: null,
    categoryId: 'c-03', categoryName: 'Danh mục tuyển chọn', linkedBookIds: ['b-03', 'b-09'],
    linkedBookTitles: ['Clean Code: A Handbook of Agile Software Craftsmanship', 'Atomic Habits'],
    authorName: 'Alex Nguyen', editorName: null, viewCount: 31200, conversionCount: 890,
    status: ArticleStatus.Published, publishedAt: '2025-10-05T02:00:00Z', createdAt: '2025-10-03T08:00:00Z' },
  { id: 'ar-03', code: 'ART-0003', title: 'Việt Nam Mười Năm - Từ Lịch Sử Đến Hiện Đại', slug: 'viet-nam-muon-nam',
    summary: 'Dang soan bai.', coverImageUrl: null, categoryId: 'c-02', categoryName: 'Lich su xuat ban',
    linkedBookIds: [], linkedBookTitles: [], authorName: 'Mai Chi', editorName: 'Hoang Nam',
    viewCount: 0, conversionCount: 0, status: ArticleStatus.InReview, publishedAt: null,
    createdAt: '2025-11-01T08:00:00Z' }
];

export const ARTICLE_MOCK_ROWS: ArticleListItemResponse[] = ARTICLE_MOCK_RAW.map((a) => ({
  ...a,
  isDeleted: false,
  updatedAt: null
}));
