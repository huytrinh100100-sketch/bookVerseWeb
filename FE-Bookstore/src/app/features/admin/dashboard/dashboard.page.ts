import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { DASHBOARD_MOCK } from './dashboard.mock';
import { DashboardApiService } from './dashboard-api.service';
import { DashboardOverviewResponse } from './dashboard.models';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/ui/stat-card/stat-card.component';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink, PageHeaderComponent, StatCardComponent, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Tổng quan"
      subtitle="Số liệu vận hành của BookVerse trong 30 ngày qua."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Tổng quan' }]"
    >
      <button type="button" class="btn btn--ghost"><i class="pi pi-download"></i> Tải báo cáo</button>
    </app-page-header>

    <div class="metrics">
      @for (metric of overview()?.metrics ?? []; track metric.label) {
        <app-stat-card [config]="metric" />
      }
    </div>

    <div class="grid">
      <section class="panel">
        <h2 class="panel__title">Hiệu lực theo danh mục</h2>
        <div class="panel__body">
          @for (row of overview()?.categoryPerformance ?? []; track row.categoryName) {
            <div class="bar-row">
              <span class="bar-row__label">{{ row.categoryName }}</span>
              <div class="bar-row__track">
                <div class="bar-row__fill" [style.width.%]="row.sharePercent"></div>
              </div>
              <span class="bar-row__value">{{ row.sharePercent }}%</span>
            </div>
          } @empty {
            <p class="panel__empty">Chưa có dữ liệu hiệu lực.</p>
          }
        </div>
      </section>

      <section class="panel panel--danger">
        <h2 class="panel__title">Cần nhậc mức thấp</h2>
        <ul class="alert-list">
          @for (alert of overview()?.lowStockAlerts ?? []; track alert.bookId) {
            <li>
              <span>{{ alert.title }}</span>
              <span class="alert-list__value">{{ alert.stockForSell }} bản bán</span>
            </li>
          } @empty {
            <li class="panel__empty">Tất cả sách đều còn đủ tồn kho.</li>
          }
        </ul>
      </section>
    </div>

    <section class="panel">
      <h2 class="panel__title">Đơn hàng mới nhất</h2>
      <ng-template #actionsTpl>
        <a class="btn btn--link" [routerLink]="['/admin/orders']">Chi tiết</a>
      </ng-template>
      <app-data-table [columns]="orderColumns" [rows]="overview()?.recentOrders ?? []" [actions]="actionsTpl" />
    </section>
  `,
  styles: [
    `
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(15rem, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
      .grid { display: grid; grid-template-columns: 2fr 1fr; gap: 1rem; }
      @media (max-width: 60rem) { .grid { grid-template-columns: 1fr; } }
      .panel {
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.875rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        padding: 1.25rem;
        box-shadow: 0 1px 2px rgb(0 0 0 / 0.05);
        margin-bottom: 1.5rem;
      }
      .panel__title { margin: 0 0 1rem; font-size: 1.0625rem; font-weight: 600; }
      .panel__body { display: flex; flex-direction: column; gap: 0.75rem; }
      .panel__empty { color: var(--outline, #76777d); font-size: 0.875rem; text-align: center; padding: 1rem; margin: 0; }
      .panel--danger { border-left: 3px solid var(--error, #ba1a1a); }
      .bar-row { display: grid; grid-template-columns: 14rem 1fr 4rem; align-items: center; gap: 0.75rem; }
      .bar-row__label { font-size: 0.875rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
      .bar-row__track { background: var(--surface-container-high, #e6e8ea); border-radius: 9999px; height: 0.5rem; overflow: hidden; }
      .bar-row__fill { background: var(--secondary, #006c49); height: 100%; border-radius: 9999px; }
      .bar-row__value { text-align: right; font-size: 0.8125rem; font-weight: 600; }
      .alert-list { list-style: none; margin: 0; padding: 0; }
      .alert-list li { display: flex; justify-content: space-between; gap: 1rem; padding: 0.5rem 0; border-bottom: 1px solid var(--surface-container-high, #e6e8ea); font-size: 0.875rem; }
      .alert-list__value { font-weight: 600; color: var(--error, #ba1a1a); white-space: nowrap; }
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
      .btn--link { background: transparent; color: var(--secondary, #006c49); }
    `
  ]
})
export class AdminDashboardPageComponent implements OnInit {
  private readonly api = inject(DashboardApiService);
  readonly overview = signal<DashboardOverviewResponse | null>(null);
  readonly loading = signal(true);

  readonly orderColumns: TableColumn[] = [
    { field: 'code', header: 'Mã đơn' },
    { field: 'customerName', header: 'Khách hàng' },
    { field: 'previewItem', header: 'Sản phẩm' },
    { field: 'amount', header: 'Tổng tiền', type: 'money' },
    { field: 'paymentMethod', header: 'Thanh toán' },
    {
      field: 'orderStatus',
      header: 'Trạng thái',
      type: 'status',
      statusMap: {
        Pending: { label: 'Chờ xử lý', severity: 'warning' },
        Paid: { label: 'Đã thanh toán', severity: 'info' },
        Processing: { label: 'Đang xử lý', severity: 'info' },
        Shipped: { label: 'Đang giao', severity: 'info' },
        Completed: { label: 'Hoàn thành', severity: 'success' },
        Cancelled: { label: 'Đã hủy', severity: 'danger' },
        Refunded: { label: 'Đã hoàn tiền', severity: 'danger' }
      }
    }
  ];

  async ngOnInit(): Promise<void> {
    this.loading.set(true);
    try {
      const response = await firstValueFrom(this.api.getOverview());
      this.overview.set(response.data ?? null);
    } finally {
      this.loading.set(false);
    }
  }
}
