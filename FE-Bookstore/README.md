# BookVerse - Frontend (FE) — Bóc tách từ Stitch & CSDL Quản lý sách

> Thư mục `FE-Bookstore/` chứa toàn bộ mã nguồn Frontend cho hệ thống **Quản lý sách (BookVerse)**,
> được dựng lại từ 18 màn giao diện Stitch trong `stitch_bookstore_admin_dashboard/`,
> bám theo pattern FE của dự án HIS (`FE/src/app`) và bám theo CSDL đã đánh giá
> (`Docs/Danh-gia-thiet-ke-CSDL-Quan-ly-Nha-sach.docx`).

---

## 1. Tổng quan 2 khu vực giao diện

Stitch chia làm 2 nhóm rõ rệt, và FE cũng tách theo đúng 2 nhóm:

| Nhóm | Stitch screens | Route FE | Yêu cầu đăng nhập |
|---|---|---|---|
| **1. Trang quản trị (Admin)** | 12 màn `bookverse_*` (không có tiền tố `storefront`) | `/admin/**` | Có — kèm `permissionGuard` |
| **2. Trang mua hàng (Storefront)** | 7 màn `bookverse_storefront_*` | `/shop/**` | Không bắt buộc (chỉ cần khi checkout) |

### 1.1. Admin — 12 màn Stitch → FE

| # | Stitch screen | FE route | FE module (feature) | Bảng CSDL chính |
|---|---|---|---|---|
| 1 | `bookverse_dashboard_overview` | `/admin/dashboard` | `features/admin/dashboard` | `Orders`, `Books`, `Categories` (aggregate) |
| 2 | `bookverse_books_management` | `/admin/books` | `features/admin/books` | `Books`, `BookCopies`, `Categories`, `Authors`, `Publishers` |
| 3 | `bookverse_category_manager` | `/admin/categories` | `features/admin/categories` | `Categories` (cây động) |
| 4 | `bookverse_authors_publishers` | `/admin/partners` | `features/admin/partners` | `Authors`, `Publishers` |
| 5 | `bookverse_orders_management` | `/admin/orders` | `features/admin/orders` | `Orders`, `OrderDetails`, `Coupons` |
| 6 | `bookverse_customer_directory` | `/admin/customers` | `features/admin/customers` | `Users` (Customer), `Orders`, `Provinces`, `Communes` |
| 7 | `bookverse_inventory_and_stock` | `/admin/inventory` | `features/admin/inventory` | `BookCopies`, `Books`, `Warehouses` |
| 8 | `bookverse_reviews_approval` | `/admin/reviews` | `features/admin/reviews` | `Reviews` |
| 9 | `bookverse_promotions_vouchers` | `/admin/promotions` | `features/admin/promotions` | `Coupons`, `Campaigns` |
| 10 | `bookverse_blog_articles` | `/admin/articles` | `features/admin/articles` | `Articles` |
| 11 | `bookverse_permissions_access` | `/admin/permissions` | `features/admin/permissions` | `Permissions`, `RolePermissions`, `Users`, `Roles` |
| 12 | `bookverse_settings` | `/admin/settings` | `features/admin/settings` | `Settings`, `PaymentGateways`, `Carriers` |

### 1.2. Storefront — 7 màn Stitch → FE

| # | Stitch screen | FE route | FE module | Bảng CSDL chính |
|---|---|---|---|---|
| 1 | `bookverse_storefront_home` | `/shop` | `features/storefront/pages/storefront-home` | `Books`, `Categories` (featured), `Coupons` (flash deal) |
| 2 | `bookverse_storefront_catalog_search` | `/shop/catalog` | `features/storefront/pages/storefront-catalog` | `Books`, `Categories`, `Authors`, `Publishers` (facets) |
| 3 | `bookverse_storefront_book_details` | `/shop/books/:slug` | `features/storefront/pages/storefront-book-details` | `Books`, `BookCopies`, `Reviews` |
| 4 | `bookverse_storefront_cart` | `/shop/cart` | `features/storefront/pages/storefront-cart` | `Books`, `Coupons` (session cart) |
| 5 | `bookverse_storefront_checkout` | `/shop/checkout` | `features/storefront/pages/storefront-checkout` | `Orders`, `OrderDetails`, `Customers`, `Provinces`, `Communes`, `Carriers` |
| 6 | `bookverse_storefront_order_success_tracking` | `/shop/order-success/:code` | `features/storefront/pages/storefront-order-success` | `Orders`, `OrderDetails` |
| 7 | `bookverse_storefront_account_order_history` | `/shop/account` | `features/storefront/pages/storefront-account` | `Customers`, `Orders`, `Reviews` |
| — | `bookverse_storefront_curated_salons_journal` | `/shop/curations` | (chưa gen — cần bảng `Articles` mở rộng + `Salons`) | `Articles`, `Salons` |

---

## 2. Cấu trúc thư mục

```
FE-Bookstore/
├── angular.json
├── package.json
├── tsconfig.json / tsconfig.app.json
├── src/
│   ├── index.html
│   ├── main.ts
│   ├── styles.css                       ← Design tokens bóc từ Stitch (Material 3 palette)
│   └── app/
│       ├── app.component.ts             ← Root + toast host
│       ├── app.config.ts                ← provideRouter / provideHttpClient / API_BASE_URL
│       ├── app.routes.ts                ← Rẽ nhánh /shop vs /admin
│       │
│       ├── core/                        ← Hạ tầng dùng chung (KHÔNG phụ thuộc domain)
│       │   ├── core.tokens.ts           ← API_BASE_URL injection token
│       │   ├── auth/
│       │   │   ├── auth.models.ts       ← LoginRequest, UserProfile (roles + permissions)
│       │   │   ├── auth.store.ts        ← Signal store: currentUser / accessToken / permissions
│       │   │   ├── auth-api.service.ts  ← /auth/login, /auth/refresh, /auth/logout
│       │   │   └── storage.service.ts   ← localStorage wrapper
│       │   ├── guards/
│       │   │   ├── auth.guard.ts        ← Chặn /admin khi chưa đăng nhập
│       │   │   └── permission.guard.ts  ← Đọc route.data.permission
│       │   ├── http/
│       │   │   ├── auth.interceptor.ts   ← Gắn Bearer token
│       │   │   └── error.interceptor.ts  ← 401 → login, 403 → toast
│       │   ├── permissions/
│       │   │   ├── permission.constants.ts ← 62 mã quyền (khớp BE PermissionConstants)
│       │   │   └── has-permission.directive.ts ← *hasPermission="'book.create'"
│       │   └── services/
│       │       ├── menu.service.ts      ← Menu admin động theo quyền (bám sidebar Stitch)
│       │       └── toast.service.ts     ← Toast signal-based
│       │
│       ├── shared/                      ← UI dùng chung + layout primitives
│       │   ├── index.ts
│       │   ├── models/
│       │   │   ├── api.models.ts         ← ApiResponse, PagedResult, PagingRequest
│       │   │   └── common.models.ts      ← Nullable, TreeNode, OptionItem
│       │   ├── layout/
│       │   │   ├── sidebar/sidebar.component.ts
│       │   │   ├── topbar/topbar.component.ts
│       │   │   └── page-wrapper/page-wrapper.component.ts
│       │   └── ui/
│       │       ├── stat-card/            ← 4 ô số liệu (Live Stock Value, Low Stock Alert...)
│       │       ├── page-header/          ← Eyebrow + tiêu đề + breadcrumb + actions
│       │       ├── search-filter-bar/    ← Ô tìm kiếm + dropdown lọc
│       │       ├── data-table/           ← Bảng dữ liệu dùng chung
│       │       ├── status-badge/
│       │       ├── empty-state/
│       │       └── confirm-delete-dialog/
│       │
│       ├── layout/app-shell/            ← Khung admin (sidebar + topbar + outlet)
│       │
│       └── features/
│           ├── auth/login/               ← LoginPageComponent
│           │
│           ├── admin/                   ← ══ KHU VỰC 1: QUẢN TRỊ ══
│           │   ├── admin.routes.ts
│           │   ├── dashboard/            ← dashboard.models|api|mock + page
│           │   ├── books/                ← Books + BookCopies
│           │   ├── categories/           ← Cây danh mục động
│           │   ├── partners/             ← Authors + Publishers (gộp 1 bảng UI)
│           │   ├── orders/
│           │   ├── customers/
│           │   ├── inventory/            ← Kho + Ban sao
│           │   ├── promotions/           ← Coupons + Campaigns
│           │   ├── reviews/
│           │   ├── articles/
│           │   ├── permissions/          ← Ma trận quyền
│           │   ├── settings/
│           │   └── errors/forbidden.page.ts
│           │
│           └── storefront/               ← ══ KHU VỰC 2: NGƯỜI MUA ══
│               ├── storefront.routes.ts
│               ├── storefront.layout.ts  ← Header/Footer/giỏ hàng badge
│               ├── storefront.models.ts  ← CatalogQuery, CartState, CheckoutPayload
│               ├── storefront.mock.ts    ← Facets, địa chỉ, lịch sử đơn
│               ├── storefront-api.service.ts
│               ├── storefront.store.ts
│               └── pages/
│                   ├── storefront-home.page.ts
│                   ├── storefront-catalog.page.ts
│                   ├── storefront-book-details.page.ts
│                   ├── storefront-cart.page.ts
│                   ├── storefront-checkout.page.ts
│                   ├── storefront-order-success.page.ts
│                   └── storefront-account.page.ts
```

---

## 3. Quy ước 4 file cho mỗi feature (giống HIS)

Mỗi module domain có đúng 4 loại file:

| File | Vai trò | Quy ước |
|---|---|---|
| `*.models.ts` | Interface request/response, enum | Dùng `Nullable<T>` thay `\| null`; kết thúc bằng `*Response` / `*Request` / `*PagedResult` |
| `*-api.service.ts` | Gọi HTTP, trả `Observable<ApiResponse<T>>` | Base URL lấy từ `inject(API_BASE_URL)`; query string build qua `HttpParams` |
| `*.store.ts` | Signal store, quản lý state + toast | `signal()` cho state, `computed()` cho derived, `loading` cho từng lần tải |
| `*.mock.ts` | Dữ liệu giả lập theo đúng shape của models | Dùng khi BE chưa sẵn sàng — cùng kiểu dữ liệu với API thật |

> `*.page.ts` chỉ chứa component; không chứa model riêng.

### 3.1. Quy ước state trong store

```ts
readonly items   = signal<T[]>([]);          // danh sách
readonly detail  = signal<TDetail | null>(null);
readonly totalItems = signal(0);
readonly loading = signal(false);
readonly filters = signal<TSearchRequest>({ page: 1, pageSize: 20 });

async load(params?: Partial<TSearchRequest>): Promise<void> {
  this.loading.set(true);
  try {
    this.filters.update((current) => ({ ...current, ...params }));
    const response = await firstValueFrom(this.api.search(this.filters()));
    this.items.set(response.data?.items ?? []);
    this.totalItems.set(response.data?.totalItems ?? 0);
  } finally {
    this.loading.set(false);
  }
}
```

### 3.2. Pattern "fallback sang mock"

Các page dùng mock làm nguồn dữ liệu khi API chưa có dữ liệu:

```ts
readonly rows = computed(() => this.store.items().length > 0 ? this.store.items() : this.mockRows());
```

Khi BE nối xong, chỉ cần xoá `.mock.ts` và đổi thành `this.store.items()`.

---

## 4. Ánh xạ CSDL → FE (chi tiết theo module)

### 4.1. Books / BookCopies → `features/admin/books`

| Cột CSDL | Field FE |
|---|---|
| `Books.Title`, `Subtitle`, `Slug`, `Isbn` | `title`, `subtitle`, `slug`, `isbn` |
| `Books.AuthorId` / `PublisherId` | `authorId` / `publisherId` (+ `authorName` / `publisherName` để hiển thị) |
| `Books.PriceForSell`, `PriceForRent` | `priceForSell`, `priceForRent` |
| `Books.PageCount`, `TotalQuantity` | `pageCount`, `totalQuantity` |
| `Books.StockForSell`, `StockForRent` | `stockForSell`, `stockForRent` |
| `Books.Status` (Draft/Published/Archived) | `status` (enum `BookStatus`) |
| `BookCopies.Barcode`, `ConditionStatus`, `ShelfCode`, `WarehouseId` | `barcode`, `conditionStatus`, `shelfCode`, `warehouseId` |
| `BookCopies.AcquiredDate / DueDate / ReturnedDate` | `acquiredDate`, `dueDate`, `returnedDate` |
| `BookCopies.Status` (Available/Lent/Reserved/Lost/Damaged/Returned) | `status` (enum `BookCopyStatus`) |

Enum `UnitType { Sell, Rent }` được dùng chung giữa `books.models.ts` và `orders.models.ts` — khớp cột `OrderDetails.UnitType` đã chốt trong CSDL.

### 4.2. Categories → `features/admin/categories`

Cây động đúng pattern HIS: node gốc `RootId = Id`, node con `RootId = Parent.RootId`, unique `(RootId, Code)`.

```ts
export const CATEGORY_ROOTS = {
  Book: 'BOOK', Publisher: 'PUBLISHER', Format: 'FORMAT',
  Language: 'LANGUAGE', Audience: 'AUDIENCE', Edition: 'EDITION'
} as const;
```

Các "facet" của storefront (danh mục, NXB, ngôn ngữ, định dạng, khoảng giá) là **cây danh mục dùng chung** — không tạo bảng riêng.

### 4.3. Authors / Publishers → `features/admin/partners`

Hai bảng CSDL tách biệt nhưng UI Stitch gộp chung 1 màn (`bookverse_authors_publishers`) → dùng `enum PartnerType { Author, Publisher }` để lọc trên cùng 1 bảng, tab "Tất cả / Tác giả / NXB".

### 4.4. Orders / OrderDetails → `features/admin/orders` + `features/storefront/pages/storefront-checkout`

| Cột CSDL | Field FE |
|---|---|
| `Orders.Code` | `code` (hiển thị dạng `#BK-1041`) |
| `Orders.CustomerId` | `customerId` (+ `customerName`, `customerPhone` denormalize) |
| `Orders.OrderDate`, `OrderStatus` | `orderDate`, `orderStatus` (enum `OrderStatus`) |
| `Orders.TotalAmount`, `ShippingAmount`, `DiscountAmount`, `FinalAmount` | tương ứng |
| `Orders.CouponId` / `CouponCode` | `couponId` / `couponCode` |
| `Orders.ShippingAddress`, `ProvinceId`, `CommuneId` | `shippingAddress`, `provinceName`, `communeName` |
| `OrderDetails.UnitType`, `Quantity`, `UnitPrice`, `DiscountAmount`, `LineTotal` | tương ứng |
| `OrderDetails.IsbnSnapshot`, `AuthorName` | `isbnSnapshot`, `authorName` |

### 4.5. Coupons → `features/admin/promotions` + giỏ hàng storefront

| Cột CSDL | Field FE |
|---|---|
| `Coupons.Code`, `Name` | `code`, `name` |
| `Coupons.DiscountType` | `discountType` (enum `DiscountType`: Percent/FixedAmount/FreeShipping/BuyXGetY) |
| `Coupons.DiscountValue` | `discountValue` |
| `Coupons.MinOrderValue`, `MaxDiscountAmount` | `minOrderValue`, `maxDiscountAmount` |
| `Coupons.UsageLimit`, `UsedCount`, `PerUserUsageLimit` | `usageLimit`, `usedCount`, `perUserUsageLimit` |
| `Coupons.StartDate`, `EndDate`, `Status`, `Audience` | tương ứng |

Cart của storefront dùng đúng 4 trường này khi áp mã (`FREESHIPHANOI`, `VIPPLATINUM15` trong Stitch).

### 4.6. Users (Customer) → `features/admin/customers` + `features/storefront`

`Users` (Identity) + profile mở rộng: `fullName`, `phoneNumber`, `provinceId`, `communeId`, `address`, `vipTier`, `lifetimeSpend`, `preferredCategory`, `status`.

`VipTier` dùng chung giữa admin (segmentation) và storefront (badge Platinum).

### 4.7. Provinces / Communes → checkout storefront

Storefront checkout (`bookverse_storefront_checkout`) có mục "Recipient & Archival Address" → dùng `Provinces` + `Communes` đã chốt trong CSDL (import CSV/XLSX, không seed tay).

---

## 5. Phân quyền

### 5.1. Nguồn quyền

- BE: bảng `Permissions` (mã dạng `book.create`) + `RolePermissions` + `Roles` (Identity).
- FE: `core/permissions/permission.constants.ts` — 62 mã, đặt tên giống hệt BE.
- Khi đăng nhập, JWT chứa claim từng quyền → `AuthStore.permissions()`.

### 5.2. Ba cơ chế kiểm tra quyền trong FE

| Cơ chế | Dùng ở đâu | Ví dụ |
|---|---|---|
| `permissionGuard` | Route admin | `data: { permission: 'book.list' }` → 403 nếu thiếu |
| `*hasPermission` | Nút hành động | `<button *hasPermission="PermissionConstants.BookCreate">` |
| `MenuService.visibleItems` | Sidebar admin | Menu item biến mất nếu không có quyền |

### 5.3. Ma trận quyền mẫu (theo vai trò trong CSDL)

| Nhóm quyền | Admin | Librarian | Staff | Customer |
|---|---|---|---|---|
| `book.*` | Toàn bộ | Toàn bộ | Xem | Xem (chỉ `status=Published`) |
| `category.*` | Toàn bộ | Xem/Tạo/Sửa/Sắp xếp | — | — |
| `order.*` | Toàn bộ | Xem | Xem/Tạo/Xử lý | Tạo/Xem của chính mình |
| `customer.*` | Toàn bộ | Xem | Xem | Xem của chính mình |
| `coupon.*` | Toàn bộ | Xem | Xem | Áp dụng |
| `shop.*` | — | — | — | Toàn bộ |

---

## 6. Ánh xạ UI Stitch → component FE (để khi code giao diện không phải đoán)

### 6.1. Admin — thành phần lặp lại

| Stitch element | FE component |
|---|---|
| Sidebar đen 256px + logo + user card + menu nhóm | `shared/layout/sidebar/sidebar.component.ts` (nhóm: General / Catalog / Sales / Operations / Marketing / System) |
| Topbar 64px + ô tìm kiếm ⌘K + badge thông báo + CTA | `shared/layout/topbar/topbar.component.ts` |
| Breadcrumb "Inventory Operations • Master Ledger" + `<h1>` + pill số lượng | `shared/ui/page-header` (`breadcrumbs` + `title` + `subtitle`) |
| 4 ô metric (Live Stock Value, Active In-Store, Low Stock Alert, Avg Gross Margin) | `shared/ui/stat-card` × 4 |
| Card "Filter & Search Toolbar" | `shared/ui/search-filter-bar` |
| Bảng dữ liệu + icon Edit/Delete + phân trang | `shared/ui/data-table` + `showActions` |
| Tab tròn "All Orders 2,845 / Pending 42 / ..." | inline `tabs` trong page |
| Badge trạng thái (Pending/Paid/Refunded) | `shared/ui/status-badge` + `statusMap` trong `TableColumn` |

### 6.2. Storefront — thành phần lặp lại

| Stitch element | FE component |
|---|---|
| Header logo + nav (home/catalog/curations/account) + search + wishlist + cart badge | `features/storefront/storefront.layout.ts` |
| Hero "Haruki Murakami: The Melancholy of Urban Solitude" | `storefront-home.page.ts` |
| Tabs "🔥 Best Sellers / ✨ New Releases / 🏆 Award Winners" | `storefront-home.page.ts` (`tabs`) |
| Card sách + nút favorite + Add to Cart | `storefront-home.page.ts` / `storefront-catalog.page.ts` |
| Sidebar facet (Category + Publisher + Price + Rating) | `storefront-catalog.page.ts` (`facets`) |
| Sort dropdown "Curator's Recommendation / Price..." | `storefront-catalog.page.ts` (`sortBy`) |
| Grid/List toggle | `storefront-catalog.page.ts` (biến `viewMode`) |
| Cart "Your Book Basket (3 volumes selected)" + Order Summary + mã giảm giá | `storefront-cart.page.ts` |
| Checkout 3 khối (Address / Carrier / Payment) + sticky tổng tiền | `storefront-checkout.page.ts` |
| Order success + mã AWB + tải hoá đơn VAT | `storefront-order-success.page.ts` |
| Account: profile card + 5 menu + lịch sử đơn + tabs trạng thái | `storefront-account.page.ts` |

### 6.3. Design tokens

`src/styles.css` chứa palette Material 3 **bóc đúng từ `tailwind-config` trong file Stitch**:
`--primary-container: #131b2e` (sidebar), `--secondary: #006c49` (CTA), `--secondary-container: #6cf8bb` (active menu),
`--tertiary-container: #001a42`, `--surface: #f7f9fb`, `--outline-variant: #c6c6cd`, `--error: #ba1a1a`,
radius `0.25 / 0.5 / 0.75 / 9999px`, font `Plus Jakarta Sans` (headline) + `Manrope` (body).

---

## 7. Hợp đồng API (endpoint FE mong đợi)

| Method | Endpoint | Mô tả |
|---|---|---|
| POST | `/api/v1/auth/login` | → `UserProfile` (roles + permissions + tokens) |
| POST | `/api/v1/auth/refresh` | → `UserProfile` mới |
| POST | `/api/v1/auth/logout` | Thu hồi refresh token |
| GET | `/api/v1/dashboard/overview` | Aggregate cho trang tổng quan |
| GET/POST/PUT/DELETE | `/api/v1/books`, `/books/{id}/status`, `/books/{id}/stock`, `/books/{id}/restore` | CRUD sách + điều chỉnh tồn kho |
| GET/POST/PUT/DELETE | `/api/v1/book-copies` | Bản sao vật lý (barcode) |
| GET | `/api/v1/categories/tree?rootCode=BOOK` | Cây danh mục |
| POST/PUT | `/api/v1/categories/{id}/move` | Đổi cha / sắp thứ tự |
| GET/POST/PUT/DELETE | `/api/v1/partners` | Tác giả + NXB (`type=Author|Publisher`) |
| GET/POST/PUT | `/api/v1/orders`, `/orders/{id}/status`, `/orders/{id}/cancel`, `/orders/{id}/refund` | Đơn hàng |
| GET/POST/PUT | `/api/v1/customers`, `/customers/{id}/voucher`, `/customers/export-pii` | Khách hàng |
| GET/POST/PUT | `/api/v1/inventory`, `/inventory/restock-all`, `/inventory/purchase-orders/draft` | Tồn kho |
| GET/POST/PUT | `/api/v1/coupons`, `/coupons/validate/{code}`, `/coupons/{id}/pause` | Voucher |
| GET/POST/PUT | `/api/v1/reviews`, `/reviews/{id}/approve`, `/reviews/{id}/reject` | Duyệt đánh giá |
| GET/POST/PUT | `/api/v1/articles`, `/articles/{id}/publish` | Blog |
| GET | `/api/v1/permissions` | Danh sách quyền |
| GET/POST/PUT | `/api/v1/roles`, `/roles/{id}/clone`, `/roles/assign` | Vai trò & ma trận quyền |
| GET/PUT | `/api/v1/settings/store`, `/settings/payment-gateways`, `/settings/carriers` | Cấu hình |
| GET | `/api/v1/shop/catalog` | Catalog + facets (storefront) |
| GET | `/api/v1/shop/books/{slug}` | Chi tiết sách |
| GET/POST/PUT/DELETE | `/api/v1/shop/cart`, `/shop/cart/items`, `/shop/cart/coupon` | Giỏ hàng |
| GET/POST | `/api/v1/shop/addresses` | Sổ địa chỉ |
| POST | `/api/v1/shop/checkout` | Đặt hàng |
| GET | `/api/v1/shop/orders`, `/shop/profile`, `/shop/reviews` | Lịch sử + hồ sơ + đánh giá |

---

## 8. Cách chạy

```bash
cd FE-Bookstore
npm install
npm start          # http://localhost:4200
```

Chưa có BE? Các page tự fallback sang dữ liệu trong `*.mock.ts`.
Muốn đăng nhập giả trong lúc phát triển, sửa `AuthStore.login()` để set session mẫu:

```ts
this.setSession({
  id: 'u-01', userName: 'admin', email: 'admin@bookverse.vn', fullName: 'Alex Nguyen',
  roles: ['Admin'], permissions: [], accessToken: 'mock', refreshToken: 'mock',
  expiresAt: new Date(Date.now() + 3600_000).toISOString()
});
```

> Lưu ý: `AuthStore.hasPermission()` trả `true` cho vai trò `Admin` — nên role Admin không cần khai báo đủ 62 quyền.

---

## 9. Việc còn lại (TODO)

| # | Hạng mục | Ghi chú |
|---|---|---|
| 1 | `bookverse_storefront_curated_salons_journal` | Cần thêm bảng `Salons` + `ArticleSubmissions` vào CSDL, sau đó gen `features/storefront/pages/storefront-curations.page.ts` |
| 2 | Form nhập liệu (modal Add New Book / Category / Coupon…) | Stitch dùng modal inline; nên tách thành `dialog/` component riêng cho từng feature |
| 3 | Biểu đồ doanh thu | Cần thêm thư viện (Chart.js hoặc `p-chart` của PrimeNG) cho `dashboard.revenueSeries` |
| 4 | Import/Export CSV | Cần endpoint `POST /books/import` (multipart) + `GET /books/export` |
| 5 | Phân trang server-side | `DataTableComponent` hiện render toàn bộ rows; cần thêm `paginator` |
| 6 | i18n | Stitch dùng song ngữ (VI storefront / EN admin); hiện hard-code, nên tách sang file `i18n/` |
| 7 | Test | Viết unit test cho `*Store` (load/filter/derived signals) |

---

## 10. Liên kết tài liệu gốc

| Tài liệu | Vai trò |
|---|---|
| `Docs/Danh-gia-thiet-ke-CSDL-Quan-ly-Nha-sach.docx` | Đánh giá & chốt CSDL (22 bảng, 107 quyền, thứ tự seed) |
| `Docs/Bo-Prompt-Template-Sinh-code-FE-BE.docx` | Prompt template sinh code FE/BE |
| `Docs/prompts/bootstrap_fe.md`, `bootstrap_be.md`, `feature.md` | Prompt GĐ0 / GĐ1 |
| `stitch_bookstore_admin_dashboard/` | 18 màn giao diện Stitch nguồn |
| `FE/src/app/` | Dự án HIS — tham chiếu pattern (signal store, guard, directive) |
