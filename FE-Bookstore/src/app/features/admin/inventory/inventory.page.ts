import { ChangeDetectionStrategy, Component, computed, inject, OnInit } from '@angular/core';
import { DataTableComponent, TableColumn } from '../../../shared/ui/data-table/data-table.component';
import { PageHeaderComponent } from '../../../shared/ui/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/ui/stat-card/stat-card.component';
import { InventoryStore } from './inventory.store';

@Component({
  selector: 'app-admin-inventory',
  standalone: true,
  imports: [DataTableComponent, PageHeaderComponent, StatCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Tồn kho &amp; Bản sao"
      subtitle="Theo dõi lưu lượng bán, mức tồn thấp và tạo phiếu nhập hàng."
      [breadcrumbs]="[{ label: 'Trang chủ', route: '/admin/dashboard', icon: 'pi pi-home' }, { label: 'Vận hành' }, { label: 'Tồn kho' }]"
    >
      <button type="button" class="btn btn--ghost"><i class="pi pi-download"></i> Phiếu kiểm kê</button>
      <button type="button" class="btn btn--primary" (click)="restock()"><i class="pi pi-bolt"></i> Nhập gấp mức thấp</button>
    </app-page-header>

    <div class="metrics">
      <app-stat-card [config]="{ label: 'Tổng lưu lượng', value: totalStockValue() + ' bản', icon: 'pi pi-box', tone: 'primary' }" />
      <app-stat-card [config]="{ label: 'Mức nguy cấp (<5)', value: criticalCount() + '', icon: 'pi pi-exclamation-circle', tone: 'danger' }" />
      <app-stat-card [config]="{ label: 'Mức thấp (<10)', value: lowCount() + '', icon: 'pi pi-exclamation-triangle', tone: 'warning' }" />
      <app-stat-card [config]="{ label: 'Hết hàng', value: outOfStockCount() + '', icon: 'pi pi-ban', tone: 'danger' }" />
    </div>

    <div class="warehouses">
      @for (warehouse of warehouses(); track warehouse.id) {
        <span class="warehouses__chip"><i class="pi pi-warehouse"></i> {{ warehouse.name }}</span>
      } @empty {
        <span class="warehouses__chip warehouses__chip--empty">Chưa có kho hàng</span>
      }
    </div>

    <ng-template #actionsTpl let-row>
      <button type="button" class="icon-btn" title="Nhập thêm"><i class="pi pi-bolt"></i></button>
      <button type="button" class="icon-btn" title="Đặt hàng"><i class="pi pi-shopping-cart"></i></button>
    </ng-template>

    <app-data-table [columns]="columns" [rows]="rows()" [actions]="actionsTpl" />
  `,
  styles: [
    `
      .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(13rem, 1fr)); gap: 1rem; margin-bottom: 1rem; }
      .warehouses { display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1rem; }
      .warehouses__chip {
        font-size: 0.8125rem;
        padding: 0.25rem 0.75rem;
        border-radius: 9999px;
        background: var(--surface-container-high, #e6e8ea);
        display: inline-flex;
        align-items: center;
        gap: 0.375rem;
      }
      .warehouses__chip--empty { color: var(--outline, #76777d); }
      .icon-btn {
        border: none;
        background: transparent;
        cursor: pointer;
        color: var(--on-surface-variant, #45464d);
        padding: 0.375rem;
        border-radius: 0.375rem;
      }
      .icon-btn:hover { background: var(--surface-container-high, #e6e8ea); }
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
export class AdminInventoryPageComponent implements OnInit {
  readonly store = inject(InventoryStore);

  readonly rows = computed(() => this.store.items());
  readonly warehouses = computed(() => this.store.warehouses());
  readonly criticalCount = computed(() => this.rows().filter((r) => r.stockForSell > 0 && r.stockForSell < 5).length);
  readonly lowCount = computed(() => this.rows().filter((r) => r.stockForSell >= 5 && r.stockForSell < 10).length);
  readonly outOfStockCount = computed(() => this.rows().filter((r) => r.stockForSell === 0).length);
  readonly totalStockValue = computed(() => this.rows().reduce((sum, r) => sum + r.totalQuantity, 0));

  readonly columns: TableColumn[] = [
    { field: 'title', header: 'Sách & Mã vạch' },
    { field: 'publisherName', header: 'NXB' },
    { field: 'warehouseName', header: 'Kho' },
    { field: 'totalQuantity', header: 'Lượng tồn', type: 'number' },
    { field: 'velocity', header: 'Tốc độ bán' },
    { field: 'reorderPoint', header: 'Điểm nhắc', type: 'number' }
  ];

  async ngOnInit(): Promise<void> {
    await this.store.load();
    await this.store.loadWarehouses();
  }

  async restock(): Promise<void> {
    await this.store.restockAll();
  }
}
