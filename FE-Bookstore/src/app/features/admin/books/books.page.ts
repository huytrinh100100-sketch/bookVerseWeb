import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { SearchFilterBarComponent, SearchFilterValue } from '../../../shared/ui/search-filter-bar/search-filter-bar.component';
import { StatCardComponent } from '../../../shared/ui/stat-card/stat-card.component';
import { LookupOptionsService } from '../../../shared/services/lookup-options.service';
import { BooksStore } from './books.store';
import { BookListItemResponse, BookSearchRequest, BookStatus } from './books.models';

type DialogMode = 'create' | 'edit' | 'view' | null;

@Component({
  selector: 'app-admin-books',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    DataTableComponent,
    PageHeaderComponent,
    SearchFilterBarComponent,
    StatCardComponent,
    FormDialogComponent,
    ConfirmDialogComponent,
    HasPermissionDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Quản lý sách"
      subtitle="Danh mục phủ theo trạng thái, tồn kho và hiển thị trên storefront."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Danh mục' }, { label: 'Quản lý sách' }]"
    >
      <button type="button" class="btn btn--ghost" *hasPermission="PermissionConstants.BookImport"><i class="pi pi-upload"></i> Import CSV</button>
      <button type="button" class="btn btn--ghost" *hasPermission="PermissionConstants.BookExport"><i class="pi pi-download"></i> Xuất dữ liệu</button>
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.BookCreate" (click)="openCreate()"><i class="pi pi-plus"></i> Thêm sách mới</button>
    </app-page-header>

    <div class="metrics">
      <app-stat-card [config]="{ label: 'Tổng số sách', value: (store.totalItems() | number) + '', icon: 'pi pi-book', tone: 'primary' }" />
      <app-stat-card [config]="{ label: 'Đang hiển thị', value: publishedCount() + '', icon: 'pi pi-eye', tone: 'success' }" />
      <app-stat-card [config]="{ label: 'Cần nhập mục thấp', value: lowStockCount() + '', icon: 'pi pi-exclamation-triangle', tone: 'danger' }" />
      <app-stat-card [config]="{ label: 'Hết hàng', value: outOfStockCount() + '', icon: 'pi pi-ban', tone: 'warning' }" />
    </div>

    <app-search-filter-bar
      [fields]="filterFields"
      [value]="filterValue()"
      searchPlaceholder="Tìm theo tên sách, tác giả hoặc ISBN-13..."
      (changed)="onFilterChanged($event)"
      (reset)="onReset()"
    />

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(row)"><i class="pi pi-eye"></i></button>
      <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.BookEdit" (click)="openEdit(row)"><i class="pi pi-pencil"></i></button>
      <button type="button" class="icon-btn" title="Bật/tắt hiển thị" *hasPermission="PermissionConstants.BookEdit" (click)="toggleActive(row)"><i class="pi pi-power-off"></i></button>
      @if (row.isDeleted) {
        <button type="button" class="icon-btn" title="Khôi phục" *hasPermission="PermissionConstants.BookRestore" (click)="askRestore(row)"><i class="pi pi-refresh"></i></button>
      } @else {
        <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.BookDeleteTemp" (click)="askDelete(row)"><i class="pi pi-trash"></i></button>
      }
    </ng-template>

    <app-data-table [columns]="columns" [rows]="rows()" [actions]="actionsTpl" />

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo sách' : 'Lưu thay đổi'"
        [errorMessage]="formError()"
        [submitting]="submitting()"
        (valuesChange)="onFormChange($event)"
        (submitted)="onSubmit()"
        (cancelled)="closeDialog()"
      />
    }

    @if (confirmAction()) {
      <app-confirm-dialog
        [title]="confirmTitle()"
        [message]="confirmMessage()"
        [detail]="confirmDetail()"
        [confirmLabel]="confirmLabel()"
        [tone]="confirmAction() === 'delete' ? 'danger' : 'info'"
        [submitting]="submitting()"
        (confirmed)="runConfirm()"
        (cancelled)="confirmAction.set(null)"
      />
    }
  `,
  styles: [
    `
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
      .icon-btn {
        border: none;
        background: transparent;
        cursor: pointer;
        color: var(--on-surface-variant, #45464d);
        padding: 0.375rem;
        border-radius: 0.375rem;
        transition: background 0.15s ease;
      }
      .icon-btn:hover { background: var(--surface-container-high, #e6e8ea); color: var(--on-surface, #191c1e); }
      .icon-btn--danger { color: var(--error, #ba1a1a); }
      .btn {
        border: none;
        border-radius: 0.5rem;
        padding: 0.5rem 1rem;
        cursor: pointer;
        font-size: 0.875rem;
        font-weight: 600;
        display: inline-flex;
        align-items: center;
        gap: 0.375rem;
      }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class AdminBooksPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(BooksStore);
  private readonly lookups = inject(LookupOptionsService);

  readonly rows = computed(() => this.store.items());
  readonly publishedCount = computed(() => this.store.publishedCount());
  readonly lowStockCount = computed(() => this.store.lowStockCount());
  readonly outOfStockCount = computed(() => this.store.outOfStockCount());
  readonly categoryOptions = this.lookups.categoryOptions;
  readonly authorOptions = this.lookups.authorOptions;
  readonly publisherOptions = this.lookups.publisherOptions;
  readonly filterValue = signal<SearchFilterValue>({ keyword: '', filters: {} });

  readonly filterFields = [
    {
      key: 'status',
      label: 'Trạng thái',
      type: 'select' as const,
      placeholder: 'Tất cả trạng thái',
      options: [
        { label: 'Tất cả trạng thái', value: '' },
        { label: 'Đang hiển thị', value: 'Published' },
        { label: 'Bản nháp', value: 'Draft' },
        { label: 'Đã lưu trữ', value: 'Archived' }
      ]
    },
    {
      key: 'stockLevel',
      label: 'Tồn kho',
      type: 'select' as const,
      placeholder: 'Tất cả mức tồn kho',
      options: [
        { label: 'Tất cả mức tồn kho', value: '' },
        { label: 'Còn hàng (>10)', value: 'in-stock' },
        { label: 'Thấp (1-9)', value: 'low-stock' },
        { label: 'Hết hàng (0)', value: 'out-of-stock' }
      ]
    }
  ];

  readonly columns: TableColumn[] = [
    { field: 'title', header: 'Tên sách' },
    { field: 'authorName', header: 'Tác giả' },
    { field: 'categoryName', header: 'Danh mục' },
    { field: 'isbn', header: 'ISBN-13' },
    { field: 'priceForSell', header: 'Giá bán', type: 'money' },
    { field: 'stockForSell', header: 'Tồn kho', type: 'number' },
    {
      field: 'status',
      header: 'Trạng thái',
      type: 'status',
      statusMap: {
        Published: { label: 'Đang hiển thị', severity: 'success' },
        Draft: { label: 'Bản nháp', severity: 'warning' },
        Archived: { label: 'Đã lưu trữ', severity: 'info' }
      }
    }
  ];

  // ---- Dialog state
  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<BookListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  // ---- Confirm state
  readonly confirmAction = signal<'delete' | 'restore' | null>(null);
  readonly confirmTarget = signal<BookListItemResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await Promise.all([this.store.load(), this.lookups.ensureLoaded()]);
  }

  // ================= Lọc =================

  onFilterChanged(value: SearchFilterValue): void {
    this.filterValue.set(value);
    const params: Partial<BookSearchRequest> = {
      keyword: value.keyword || undefined,
      stockLevel: (value.filters['stockLevel'] as BookSearchRequest['stockLevel']) || undefined,
      status: (value.filters['status'] as BookStatus) || undefined
    };
    void this.store.load(params);
  }

  onReset(): void {
    this.filterValue.set({ keyword: '', filters: {} });
    void this.store.load({ keyword: undefined, status: undefined, stockLevel: undefined });
  }

  // ================= Dialog =================

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      title: '',
      subtitle: '',
      isbn: '',
      categoryId: '',
      authorId: '',
      publisherId: '',
      coverImageUrl: '',
      description: '',
      priceForSell: 0,
      priceForRent: 0,
      pageCount: 0,
      publicationYear: new Date().getFullYear(),
      language: 'Tiếng Việt',
      format: 'Bìa cứng',
      status: BookStatus.Draft,
      isActive: true,
      isFeatured: false,
      quantity: 0
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(row: BookListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      title: row.title,
      subtitle: row.subtitle ?? '',
      isbn: row.isbn ?? '',
      categoryId: row.categoryId ?? '',
      authorId: row.authorId ?? '',
      publisherId: row.publisherId ?? '',
      coverImageUrl: row.coverImageUrl ?? '',
      priceForSell: row.priceForSell,
      priceForRent: row.priceForRent,
      pageCount: row.pageCount,
      status: row.status,
      isActive: row.isActive,
      isFeatured: row.isFeatured
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  openView(row: BookListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      title: row.title,
      subtitle: row.subtitle ?? '',
      code: row.code,
      slug: row.slug,
      isbn: row.isbn ?? '',
      categoryName: row.categoryName ?? '—',
      authorName: row.authorName ?? '—',
      publisherName: row.publisherName ?? '—',
      priceForSell: row.priceForSell,
      priceForRent: row.priceForRent,
      pageCount: row.pageCount,
      totalQuantity: row.totalQuantity,
      stockForSell: row.stockForSell,
      stockForRent: row.stockForRent,
      status: row.status,
      isActive: row.isActive,
      isFeatured: row.isFeatured,
      averageRating: row.averageRating ?? 0,
      reviewCount: row.reviewCount,
      soldCount: row.soldCount
    });
    this.formError.set('');
    this.dialogMode.set('view');
  }

  closeDialog(): void {
    this.dialogMode.set(null);
    this.dialogTarget.set(null);
    this.formError.set('');
  }

  onFormChange(values: Record<string, unknown>): void {
    this.formValues.set(values);
  }

  dialogTitle(): string {
    switch (this.dialogMode()) {
      case 'create': return 'Thêm sách mới';
      case 'edit': return 'Cập nhật sách';
      case 'view': return 'Chi tiết sách';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    if (this.dialogMode() === 'edit' && target) {
      return `Cập nhật thông tin của "${target.title}".`;
    }
    if (this.dialogMode() === 'view' && target) {
      return `Mã sách: ${target.code}.`;
    }
    return 'Nhập đầy đủ thông tin để thêm sách vào danh mục.';
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'create':
        return [
          { name: 'title', label: 'Tên sách', type: 'text', required: true, placeholder: 'vd: Rừng Na Uy' },
          { name: 'subtitle', label: 'Phụ đề', type: 'text', placeholder: 'vd: Bản bìa cứng - Phiên bản giới hạn' },
          { name: 'isbn', label: 'ISBN-13', type: 'text', placeholder: 'vd: 9788499123411' },
          { name: 'categoryId', label: 'Danh mục', type: 'select', multiple: false, options: this.categoryOptions() },
          { name: 'authorId', label: 'Tác giả', type: 'select', multiple: false, options: this.authorOptions() },
          { name: 'publisherId', label: 'Nhà xuất bản', type: 'select', multiple: false, options: this.publisherOptions() },
          { name: 'coverImageUrl', label: 'Ảnh bìa (URL)', type: 'text', placeholder: 'https://...' },
          { name: 'priceForSell', label: 'Giá bán (VNĐ)', type: 'number', required: true, min: 0, step: 1000 },
          { name: 'priceForRent', label: 'Giá thuê (VNĐ)', type: 'number', min: 0, step: 1000 },
          { name: 'pageCount', label: 'Số trang', type: 'number', min: 0 },
          { name: 'publicationYear', label: 'Năm xuất bản', type: 'number', min: 1900, max: 2100 },
          { name: 'language', label: 'Ngôn ngữ', type: 'text', placeholder: 'vd: Tiếng Việt' },
          { name: 'format', label: 'Định dạng', type: 'text', placeholder: 'vd: Bìa cứng' },
          { name: 'quantity', label: 'Số lượng nhập kho', type: 'number', min: 0 },
          { name: 'description', label: 'Mô tả', type: 'textarea', placeholder: 'Giới thiệu nội dung sách...' },
          { name: 'status', label: 'Trạng thái', type: 'select', multiple: false, options: this.statusOptions() },
          { name: 'isActive', label: 'Hiển thị', type: 'checkbox', placeholder: 'Hiển thị trên storefront' },
          { name: 'isFeatured', label: 'Nổi bật', type: 'checkbox', placeholder: 'Đánh dấu sách nổi bật' }
        ];
      case 'edit':
        return [
          { name: 'title', label: 'Tên sách', type: 'text', required: true },
          { name: 'subtitle', label: 'Phụ đề', type: 'text' },
          { name: 'isbn', label: 'ISBN-13', type: 'text' },
          { name: 'categoryId', label: 'Danh mục', type: 'select', multiple: false, options: this.categoryOptions() },
          { name: 'authorId', label: 'Tác giả', type: 'select', multiple: false, options: this.authorOptions() },
          { name: 'publisherId', label: 'Nhà xuất bản', type: 'select', multiple: false, options: this.publisherOptions() },
          { name: 'coverImageUrl', label: 'Ảnh bìa (URL)', type: 'text' },
          { name: 'priceForSell', label: 'Giá bán (VNĐ)', type: 'number', required: true, min: 0, step: 1000 },
          { name: 'priceForRent', label: 'Giá thuê (VNĐ)', type: 'number', min: 0, step: 1000 },
          { name: 'pageCount', label: 'Số trang', type: 'number', min: 0 },
          { name: 'status', label: 'Trạng thái', type: 'select', multiple: false, options: this.statusOptions() },
          { name: 'isActive', label: 'Hiển thị', type: 'checkbox', placeholder: 'Hiển thị trên storefront' },
          { name: 'isFeatured', label: 'Nổi bật', type: 'checkbox', placeholder: 'Đánh dấu sách nổi bật' }
        ];
      case 'view':
        return [
          { name: 'code', label: 'Mã sách', type: 'text' },
          { name: 'title', label: 'Tên sách', type: 'text' },
          { name: 'subtitle', label: 'Phụ đề', type: 'text' },
          { name: 'slug', label: 'Slug (URL)', type: 'text' },
          { name: 'isbn', label: 'ISBN-13', type: 'text' },
          { name: 'categoryName', label: 'Danh mục', type: 'text' },
          { name: 'authorName', label: 'Tác giả', type: 'text' },
          { name: 'publisherName', label: 'Nhà xuất bản', type: 'text' },
          { name: 'priceForSell', label: 'Giá bán (VNĐ)', type: 'number' },
          { name: 'priceForRent', label: 'Giá thuê (VNĐ)', type: 'number' },
          { name: 'pageCount', label: 'Số trang', type: 'number' },
          { name: 'totalQuantity', label: 'Tổng số lượng', type: 'number' },
          { name: 'stockForSell', label: 'Tồn kho bán', type: 'number' },
          { name: 'stockForRent', label: 'Tồn kho thuê', type: 'number' },
          { name: 'averageRating', label: 'Điểm đánh giá', type: 'number' },
          { name: 'reviewCount', label: 'Số đánh giá', type: 'number' },
          { name: 'soldCount', label: 'Đã bán', type: 'number' },
          { name: 'status', label: 'Trạng thái', type: 'text' },
          { name: 'isActive', label: 'Hiển thị', type: 'checkbox', placeholder: 'Đang hiển thị' },
          { name: 'isFeatured', label: 'Nổi bật', type: 'checkbox', placeholder: 'Sách nổi bật' }
        ];
      default:
        return [];
    }
  }

  statusOptions(): { label: string; value: string }[] {
    return [
      { label: 'Bản nháp', value: BookStatus.Draft },
      { label: 'Đang hiển thị', value: BookStatus.Published },
      { label: 'Đã lưu trữ', value: BookStatus.Archived }
    ];
  }

  async onSubmit(): Promise<void> {
    const values = this.formValues();
    const target = this.dialogTarget();
    this.formError.set('');
    this.submitting.set(true);

    try {
      let ok = false;

      if (this.dialogMode() === 'create') {
        const result = await this.store.create({
          title: String(values['title'] ?? '').trim(),
          subtitle: String(values['subtitle'] ?? '').trim() || undefined,
          isbn: String(values['isbn'] ?? '').trim() || undefined,
          categoryId: String(values['categoryId'] ?? '') || undefined,
          authorId: String(values['authorId'] ?? '') || undefined,
          publisherId: String(values['publisherId'] ?? '') || undefined,
          coverImageUrl: String(values['coverImageUrl'] ?? '').trim() || undefined,
          description: String(values['description'] ?? '').trim() || undefined,
          priceForSell: Number(values['priceForSell'] ?? 0),
          priceForRent: Number(values['priceForRent'] ?? 0),
          pageCount: Number(values['pageCount'] ?? 0),
          publicationYear: Number(values['publicationYear'] ?? 0) || undefined,
          language: String(values['language'] ?? '').trim() || undefined,
          format: String(values['format'] ?? '').trim() || undefined,
          quantity: Number(values['quantity'] ?? 0),
          status: (values['status'] as BookStatus) ?? BookStatus.Draft,
          isActive: Boolean(values['isActive']),
          isFeatured: Boolean(values['isFeatured'])
        });
        ok = result !== null;
      } else if (this.dialogMode() === 'edit' && target) {
        const result = await this.store.update(target.id, {
          title: String(values['title'] ?? '').trim(),
          subtitle: String(values['subtitle'] ?? '').trim(),
          isbn: String(values['isbn'] ?? '').trim(),
          categoryId: String(values['categoryId'] ?? '') || undefined,
          authorId: String(values['authorId'] ?? '') || undefined,
          publisherId: String(values['publisherId'] ?? '') || undefined,
          coverImageUrl: String(values['coverImageUrl'] ?? '').trim(),
          priceForSell: Number(values['priceForSell'] ?? 0),
          priceForRent: Number(values['priceForRent'] ?? 0),
          pageCount: Number(values['pageCount'] ?? 0),
          status: (values['status'] as BookStatus) ?? undefined,
          isActive: Boolean(values['isActive']),
          isFeatured: Boolean(values['isFeatured'])
        });
        ok = result !== null;
      }

      if (ok) {
        this.closeDialog();
      } else if (!this.formError()) {
        this.formError.set('Không thể lưu dữ liệu. Vui lòng kiểm tra lại thông tin.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  // ================= Hành động =================

  async toggleActive(row: BookListItemResponse): Promise<void> {
    await this.store.toggleStatus(row.id, !row.isActive);
  }

  askDelete(row: BookListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('delete');
  }

  askRestore(row: BookListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('restore');
  }

  confirmTitle(): string {
    return this.confirmAction() === 'delete' ? 'Xóa mềm sách' : 'Khôi phục sách';
  }

  confirmMessage(): string {
    return this.confirmAction() === 'delete'
      ? 'Sách sẽ bị ẩn khỏi danh sách và storefront nhưng vẫn có thể khôi phục.'
      : 'Sách sẽ hiển thị lại trong danh sách.';
  }

  confirmDetail(): string {
    const target = this.confirmTarget();
    return target ? `Sách: ${target.title} (${target.code})` : '';
  }

  confirmLabel(): string {
    return this.confirmAction() === 'delete' ? 'Xóa mềm' : 'Khôi phục';
  }

  async runConfirm(): Promise<void> {
    const target = this.confirmTarget();
    const action = this.confirmAction();
    if (!target || !action) {
      return;
    }

    this.submitting.set(true);
    try {
      if (action === 'delete') {
        await this.store.softDelete(target.id);
      } else {
        await this.store.restore(target.id);
      }
    } finally {
      this.submitting.set(false);
      this.confirmAction.set(null);
      this.confirmTarget.set(null);
    }
  }
}
