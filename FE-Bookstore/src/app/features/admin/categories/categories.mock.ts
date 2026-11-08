import { CategoryTreeNodeResponse } from './categories.models';

type CategoryTreeMock = Omit<CategoryTreeNodeResponse, 'rootName' | 'type' | 'typeLabel'>;
type CategoryTreeMockNode = Omit<CategoryTreeMock, 'children'> & { children?: CategoryTreeMockNode[] };

const CATEGORY_TREE_MOCK_RAW: CategoryTreeMockNode[] = [
  {
    id: 'c-01', code: 'FICTION', name: 'Văn học & Văn học đẳng', slug: 'van-hoc', rootId: 'c-01', rootCode: 'BOOK',
    parentId: null, sortOrder: 1, isMegaMenu: true, isActive: true, isDeleted: false, bookCount: 214,
    children: [
      { id: 'c-01-01', code: 'FICTION-CLASSIC', name: 'Tiền bản kinh điển', slug: 'tien-ban-kinh-dien', rootId: 'c-01', rootCode: 'BOOK', parentId: 'c-01', sortOrder: 1, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 64, children: [] },
      { id: 'c-01-02', code: 'FICTION-MODERN', name: 'Đời mới dương dai', slug: 'doi-moi', rootId: 'c-01', rootCode: 'BOOK', parentId: 'c-01', sortOrder: 2, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 82, children: [] },
      { id: 'c-01-03', code: 'FICTION-JAPAN', name: 'Văn học Nhật Bản', slug: 'van-hoc-nhat-ban', rootId: 'c-01', rootCode: 'BOOK', parentId: 'c-01', sortOrder: 3, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 45, children: [] }
    ]
  },
  {
    id: 'c-02', code: 'SCIENCE', name: 'Khoa học & Xã hội', slug: 'khoa-hoc-xa-hoi', rootId: 'c-02', rootCode: 'BOOK',
    parentId: null, sortOrder: 2, isMegaMenu: true, isActive: true, isDeleted: false, bookCount: 96,
    children: [
      { id: 'c-02-01', code: 'SCIENCE-PHYSICS', name: 'Vật lý', slug: 'vat-ly', rootId: 'c-02', rootCode: 'BOOK', parentId: 'c-02', sortOrder: 1, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 28, children: [] }
    ]
  },
  {
    id: 'c-03', code: 'TECH', name: 'Công nghệ & Kinh doanh', slug: 'cong-nghe-kinh-doanh', rootId: 'c-03', rootCode: 'BOOK',
    parentId: null, sortOrder: 3, isMegaMenu: true, isActive: true, isDeleted: false, bookCount: 68,
    children: [
      { id: 'c-03-01', code: 'TECH-PROGRAMMING', name: 'Lập trình', slug: 'lap-trinh', rootId: 'c-03', rootCode: 'BOOK', parentId: 'c-03', sortOrder: 1, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 41, children: [] },
      { id: 'c-03-02', code: 'TECH-ARCHITECTURE', name: 'Kiến trúc & Hệ thống', slug: 'kien-truc', rootId: 'c-03', rootCode: 'BOOK', parentId: 'c-03', sortOrder: 2, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 27, children: [] }
    ]
  },
  {
    id: 'c-04', code: 'BUSINESS', name: 'Kinh doanh & Tài chính', slug: 'kinh-doanh-tai-chinh', rootId: 'c-04', rootCode: 'BOOK',
    parentId: null, sortOrder: 4, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 52, children: []
  },
  {
    id: 'c-05', code: 'SELF', name: 'Tự nhân & Phát triển bản thân', slug: 'tu-nhan', rootId: 'c-05', rootCode: 'BOOK',
    parentId: null, sortOrder: 5, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 38, children: []
  },
  {
    id: 'c-06', code: 'MANGA', name: 'Manga & Comic', slug: 'manga-comic', rootId: 'c-06', rootCode: 'BOOK',
    parentId: null, sortOrder: 6, isMegaMenu: false, isActive: true, isDeleted: false, bookCount: 50, children: []
  }
];

function withRootName(node: CategoryTreeMockNode): CategoryTreeNodeResponse {
  return {
    ...node,
    rootName: node.rootCode,
    type: null,
    typeLabel: null,
    children: (node.children ?? []).map(withRootName)
  };
}

export const CATEGORY_TREE_MOCK: CategoryTreeNodeResponse[] = CATEGORY_TREE_MOCK_RAW.map(withRootName);
