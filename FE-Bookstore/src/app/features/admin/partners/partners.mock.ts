import { AgreementStatus, PartnerListItemResponse, PartnerType } from './partners.models';

type PartnerMock = Omit<
  PartnerListItemResponse,
  'description' | 'avatarUrl' | 'logoUrl' | 'agreementExpiresAt' | 'isActive' | 'isDeleted' | 'updatedAt'
>;

const PARTNER_MOCK_RAW: PartnerMock[] = [
  { id: 'a-01', code: 'AUT-0001', name: 'Haruki Murakami', type: PartnerType.Author, country: 'Japan (JP)',
    website: 'https://harukimurakami.com', email: 'press@harukimurakami.jp', phoneNumber: null,
    biography: 'Tác giả Rừng Na Uy, 1Q84, Bộ Viện Không Có Tuổi Thơ.', bookCount: 14,
    agreementStatus: AgreementStatus.Active, createdAt: '2025-01-10T08:00:00Z' },
  { id: 'a-02', code: 'AUT-0002', name: 'Yuval Noah Harari', type: PartnerType.Author, country: 'Israel (IL)',
    website: null, email: 'info@ynh.io', phoneNumber: null, biography: 'Tác giả Sapiens, Homo Deus.',
    bookCount: 7, agreementStatus: AgreementStatus.Active, createdAt: '2025-01-11T08:00:00Z' },
  { id: 'a-03', code: 'AUT-0003', name: 'Robert C. Martin', type: PartnerType.Author, country: 'United States (US)',
    website: 'https://clean-code.com', email: null, phoneNumber: null, biography: 'Tác giả Clean Code, The Clean Coder.',
    bookCount: 9, agreementStatus: AgreementStatus.Active, createdAt: '2025-01-12T08:00:00Z' },
  { id: 'a-04', code: 'AUT-0004', name: 'Akiyuki Nishida', type: PartnerType.Author, country: 'Japan (JP)',
    website: null, email: null, phoneNumber: null, biography: 'Tác giả Những Ngày Ở Hiệu Sách Morisaki.',
    bookCount: 3, agreementStatus: AgreementStatus.Draft, createdAt: '2025-02-01T08:00:00Z' },
  { id: 'a-05', code: 'AUT-0005', name: 'Franz Kafka', type: PartnerType.Author, country: 'Czech (CZ)',
    website: null, email: null, phoneNumber: null, biography: 'Tác giả Hoá Thân, The Trial.', bookCount: 11,
    agreementStatus: AgreementStatus.Active, createdAt: '2025-02-05T08:00:00Z' },
  { id: 'p-01', code: 'PUB-0001', name: 'Nha Nam', type: PartnerType.Publisher, country: 'Vietnam (VN)',
    website: 'https://nhanam.vn', email: 'info@nhanam.vn', phoneNumber: '+84 24 3822 9999',
    biography: 'Nhà xuất bản van hoc lon nhat Viet Nam.', bookCount: 142,
    agreementStatus: AgreementStatus.Active, createdAt: '2025-01-01T08:00:00Z' },
  { id: 'p-02', code: 'PUB-0002', name: 'NXB Tre', type: PartnerType.Publisher, country: 'Vietnam (VN)',
    website: 'https://nxbtre.com.vn', email: 'cs@nxbtre.com.vn', phoneNumber: '+84 28 3935 5454',
    biography: 'NXB Tre - Văn học va khoa hoc.', bookCount: 85,
    agreementStatus: AgreementStatus.Active, createdAt: '2025-01-01T08:00:00Z' },
  { id: 'p-03', code: 'PUB-0003', name: "O'Reilly Media", type: PartnerType.Publisher, country: 'United States (US)',
    website: 'https://oreilly.com', email: 'sales@oreilly.com', phoneNumber: null,
    biography: 'Nhà xuất bản ky thuat danh tieng.', bookCount: 34,
    agreementStatus: AgreementStatus.Active, createdAt: '2025-01-01T08:00:00Z' },
  { id: 'p-04', code: 'PUB-0004', name: 'Kim Đồng', type: PartnerType.Publisher, country: 'Vietnam (VN)',
    website: 'https://kimdong.com.vn', email: 'info@kimdong.com.vn', phoneNumber: null,
    biography: 'NXB Kim Đồng - Thiếu nhi & manga.', bookCount: 98,
    agreementStatus: AgreementStatus.Active, createdAt: '2025-01-01T08:00:00Z' }
];

export const PARTNER_MOCK_ROWS: PartnerListItemResponse[] = PARTNER_MOCK_RAW.map((p) => ({
  ...p,
  description: null,
  avatarUrl: null,
  logoUrl: null,
  agreementExpiresAt: null,
  isActive: true,
  isDeleted: false,
  updatedAt: null
}));
