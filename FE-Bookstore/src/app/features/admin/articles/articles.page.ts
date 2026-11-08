import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { PermissionConstants } from '../../../core/permissions/permission.constants';
import { ConfirmDialogComponent } from '../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';
import { DialogField, FormDialogComponent } from '../../../shared/ui/form-dialog/form-dialog.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { LookupOptionsService } from '../../../shared/services/lookup-options.service';
import { ArticleListItemResponse, ArticleStatus } from './articles.models';
import { ArticlesStore } from './articles.store';

type DialogMode = 'create' | 'edit' | 'view' | null;

@Component({
  selector: 'app-admin-articles',
  standalone: true,
  imports: [
    FormsModule,
    DataTableComponent,
    PageHeaderComponent,
    FormDialogComponent,
    ConfirmDialogComponent,
    HasPermissionDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Blog &amp; Bài viết"
      subtitle="Nội dung liên kết trực tiếp với sách và chỉ số lưu lượng."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Tiếp thị' }, { label: 'Bài viết' }]"
    >
      <a class="btn btn--ghost" href="/shop/blog"><i class="pi pi-external-link"></i> Xem storefront</a>
      <button type="button" class="btn btn--primary" *hasPermission="PermissionConstants.ArticleCreate" (click)="openCreate()"><i class="pi pi-plus"></i> Bài viết mới</button>
    </app-page-header>

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Xem chi tiết" (click)="openView(row)"><i class="pi pi-eye"></i></button>
      <button type="button" class="icon-btn" title="Sửa" *hasPermission="PermissionConstants.ArticleEdit" (click)="openEdit(row)"><i class="pi pi-pencil"></i></button>
      <button type="button" class="icon-btn" title="Xuất bản" *hasPermission="PermissionConstants.ArticlePublish" (click)="askPublish(row)"><i class="pi pi-send"></i></button>
      <button type="button" class="icon-btn icon-btn--danger" title="Xóa mềm" *hasPermission="PermissionConstants.ArticleDelete" (click)="askDelete(row)"><i class="pi pi-trash"></i></button>
    </ng-template>

    <app-data-table [columns]="columns" [rows]="rows()" [actions]="actionsTpl" />

    @if (dialogMode()) {
      <app-form-dialog
        [title]="dialogTitle()"
        [subtitle]="dialogSubtitle()"
        [fields]="dialogFields()"
        [values]="formValues()"
        [mode]="dialogMode() === 'view' ? 'view' : 'edit'"
        [submitLabel]="dialogMode() === 'create' ? 'Tạo bài viết' : 'Lưu thay đổi'"
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
      .icon-btn {
        border: none;
        background: transparent;
        cursor: pointer;
        color: var(--on-surface-variant, #45464d);
        padding: 0.375rem;
        border-radius: 0.375rem;
      }
      .icon-btn:hover { background: var(--surface-container-high, #e6e8ea); }
      .icon-btn--danger { color: var(--error, #ba1a1a); }
      .btn {
        border: none;
        border-radius: 0.5rem;
        padding: 0.5rem 1rem;
        cursor: pointer;
        font-size: 0.875rem;
        font-weight: 600;
        text-decoration: none;
        display: inline-flex;
        align-items: center;
        gap: 0.375rem;
      }
      .btn--ghost { background: var(--surface-container-low, #f2f4f6); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
    `
  ]
})
export class AdminArticlesPageComponent implements OnInit {
  /** Mã quyền dùng cho directive *hasPermission trong template. */
  readonly PermissionConstants = PermissionConstants;

  readonly store = inject(ArticlesStore);
  private readonly lookups = inject(LookupOptionsService);

  readonly rows = computed(() => this.store.items());
  readonly categoryOptions = this.lookups.articleCategoryOptions;

  readonly columns: TableColumn[] = [
    { field: 'title', header: 'Bài viết' },
    { field: 'authorName', header: 'Tác giả / Biên tập' },
    { field: 'categoryName', header: 'Chuyên mục' },
    { field: 'viewCount', header: 'Lượt xem', type: 'number' },
    {
      field: 'status',
      header: 'Trạng thái',
      type: 'status',
      statusMap: {
        Draft: { label: 'Bản nháp', severity: 'warning' },
        InReview: { label: 'Đang duyệt', severity: 'info' },
        Scheduled: { label: 'Đã lên lịch', severity: 'info' },
        Published: { label: 'Đã xuất bản', severity: 'success' },
        Archived: { label: 'Đã lưu trữ', severity: 'info' }
      }
    },
    { field: 'publishedAt', header: 'Ngày đăng', type: 'date' }
  ];

  readonly dialogMode = signal<DialogMode>(null);
  readonly dialogTarget = signal<ArticleListItemResponse | null>(null);
  readonly formValues = signal<Record<string, unknown>>({});
  readonly formError = signal('');
  readonly submitting = signal(false);

  readonly confirmAction = signal<'publish' | 'delete' | null>(null);
  readonly confirmTarget = signal<ArticleListItemResponse | null>(null);

  async ngOnInit(): Promise<void> {
    await Promise.all([this.store.load(), this.lookups.ensureLoaded()]);
  }

  openCreate(): void {
    this.dialogTarget.set(null);
    this.formValues.set({
      title: '',
      slug: '',
      summary: '',
      content: '',
      coverImageUrl: '',
      categoryId: '',
      seoTitle: '',
      seoDescription: '',
      status: ArticleStatus.Draft
    });
    this.formError.set('');
    this.dialogMode.set('create');
  }

  openEdit(row: ArticleListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      title: row.title,
      slug: row.slug,
      summary: row.summary ?? '',
      content: '',
      coverImageUrl: row.coverImageUrl ?? '',
      categoryId: row.categoryId ?? '',
      seoTitle: '',
      seoDescription: '',
      status: row.status
    });
    this.formError.set('');
    this.dialogMode.set('edit');
  }

  openView(row: ArticleListItemResponse): void {
    this.dialogTarget.set(row);
    this.formValues.set({
      code: row.code,
      title: row.title,
      slug: row.slug,
      summary: row.summary ?? '',
      categoryName: row.categoryName ?? '—',
      authorName: row.authorName,
      editorName: row.editorName ?? '—',
      linkedBookTitles: row.linkedBookTitles,
      viewCount: row.viewCount,
      conversionCount: row.conversionCount,
      status: row.status,
      publishedAt: row.publishedAt ?? ''
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
      case 'create': return 'Thêm bài viết';
      case 'edit': return 'Cập nhật bài viết';
      case 'view': return 'Chi tiết bài viết';
      default: return '';
    }
  }

  dialogSubtitle(): string {
    const target = this.dialogTarget();
    if (this.dialogMode() === 'edit' && target) {
      return `Cập nhật bài viết "${target.title}".`;
    }
    if (this.dialogMode() === 'view' && target) {
      return `Mã bài viết: ${target.code}.`;
    }
    return 'Bài viết có thể liên kết tới nhiều sách để tăng chuyển đổi.';
  }

  dialogFields(): DialogField[] {
    switch (this.dialogMode()) {
      case 'create':
        return [
          { name: 'title', label: 'Tiêu đề', type: 'text', required: true, placeholder: 'vd: Top 10 sách hay nhất 2026' },
          { name: 'slug', label: 'Slug (URL)', type: 'text', placeholder: 'vd: top-10-sach-hay-nhat-2026' },
          { name: 'summary', label: 'Tóm tắt', type: 'textarea', placeholder: 'Đoạn giới thiệu ngắn...' },
          { name: 'content', label: 'Nội dung', type: 'textarea', placeholder: 'Nội dung bài viết...' },
          { name: 'coverImageUrl', label: 'Ảnh bìa (URL)', type: 'text' },
          { name: 'categoryId', label: 'Chuyên mục', type: 'select', multiple: false, options: this.categoryOptions() },
          { name: 'seoTitle', label: 'SEO tiêu đề', type: 'text' },
          { name: 'seoDescription', label: 'SEO mô tả', type: 'textarea' },
          { name: 'status', label: 'Trạng thái', type: 'select', multiple: false, required: true, options: this.statusOptions() }
        ];
      case 'edit':
        return [
          { name: 'title', label: 'Tiêu đề', type: 'text', required: true },
          { name: 'slug', label: 'Slug (URL)', type: 'text' },
          { name: 'summary', label: 'Tóm tắt', type: 'textarea' },
          { name: 'content', label: 'Nội dung', type: 'textarea' },
          { name: 'coverImageUrl', label: 'Ảnh bìa (URL)', type: 'text' },
          { name: 'seoTitle', label: 'SEO tiêu đề', type: 'text' },
          { name: 'seoDescription', label: 'SEO mô tả', type: 'textarea' },
          { name: 'status', label: 'Trạng thái', type: 'select', multiple: false, options: this.statusOptions() }
        ];
      case 'view':
        return [
          { name: 'code', label: 'Mã bài viết', type: 'text' },
          { name: 'title', label: 'Tiêu đề', type: 'text' },
          { name: 'slug', label: 'Slug (URL)', type: 'text' },
          { name: 'summary', label: 'Tóm tắt', type: 'textarea' },
          { name: 'categoryName', label: 'Chuyên mục', type: 'text' },
          { name: 'authorName', label: 'Tác giả', type: 'text' },
          { name: 'editorName', label: 'Biên tập viên', type: 'text' },
          { name: 'linkedBookTitles', label: 'Sách liên kết', type: 'chips' },
          { name: 'viewCount', label: 'Lượt xem', type: 'number' },
          { name: 'conversionCount', label: 'Lượt chuyển đổi', type: 'number' },
          { name: 'status', label: 'Trạng thái', type: 'text' },
          { name: 'publishedAt', label: 'Ngày đăng', type: 'date' }
        ];
      default:
        return [];
    }
  }

  statusOptions(): { label: string; value: string }[] {
    return [
      { label: 'Bản nháp', value: ArticleStatus.Draft },
      { label: 'Đang duyệt', value: ArticleStatus.InReview },
      { label: 'Đã lên lịch', value: ArticleStatus.Scheduled },
      { label: 'Đã xuất bản', value: ArticleStatus.Published },
      { label: 'Đã lưu trữ', value: ArticleStatus.Archived }
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
        ok = await this.store.create({
          title: String(values['title'] ?? '').trim(),
          slug: String(values['slug'] ?? '').trim() || undefined,
          summary: String(values['summary'] ?? '').trim() || undefined,
          content: String(values['content'] ?? '').trim() || undefined,
          coverImageUrl: String(values['coverImageUrl'] ?? '').trim() || undefined,
          categoryId: String(values['categoryId'] ?? '') || undefined,
          seoTitle: String(values['seoTitle'] ?? '').trim() || undefined,
          seoDescription: String(values['seoDescription'] ?? '').trim() || undefined,
          status: (values['status'] as ArticleStatus) ?? ArticleStatus.Draft
        });
      } else if (this.dialogMode() === 'edit' && target) {
        ok = await this.store.update(target.id, {
          title: String(values['title'] ?? '').trim(),
          slug: String(values['slug'] ?? '').trim(),
          summary: String(values['summary'] ?? '').trim(),
          content: String(values['content'] ?? '').trim(),
          coverImageUrl: String(values['coverImageUrl'] ?? '').trim(),
          seoTitle: String(values['seoTitle'] ?? '').trim(),
          seoDescription: String(values['seoDescription'] ?? '').trim(),
          status: (values['status'] as ArticleStatus) ?? undefined
        });
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

  askPublish(row: ArticleListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('publish');
  }

  askDelete(row: ArticleListItemResponse): void {
    this.confirmTarget.set(row);
    this.confirmAction.set('delete');
  }

  confirmTitle(): string {
    return this.confirmAction() === 'publish' ? 'Xuất bản bài viết' : 'Xóa mềm bài viết';
  }

  confirmMessage(): string {
    return this.confirmAction() === 'publish'
      ? 'Bài viết sẽ được xuất bản và hiển thị công khai trên storefront.'
      : 'Bài viết sẽ bị ẩn khỏi danh sách.';
  }

  confirmDetail(): string {
    const target = this.confirmTarget();
    return target ? `${target.title} (${target.code})` : '';
  }

  confirmLabel(): string {
    return this.confirmAction() === 'publish' ? 'Xuất bản' : 'Xóa mềm';
  }

  async runConfirm(): Promise<void> {
    const target = this.confirmTarget();
    const action = this.confirmAction();
    if (!target || !action) {
      return;
    }

    this.submitting.set(true);
    try {
      if (action === 'publish') {
        await this.store.publish(target.id);
      } else {
        await this.store.softDelete(target.id);
      }
    } finally {
      this.submitting.set(false);
      this.confirmAction.set(null);
      this.confirmTarget.set(null);
    }
  }
}
