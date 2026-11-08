import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, TemplateRef } from '@angular/core';

export interface TableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  type?: 'text' | 'status' | 'number' | 'date' | 'image' | 'money' | 'chips';
  statusMap?: Record<string, { label: string; severity: 'success' | 'info' | 'warning' | 'danger' }>;
  width?: string;
  /** Định dạng tiền tệ cho type = 'money'. */
  currency?: string;
}

/**
 * Hàng dữ liệu của bảng. Dùng `any` để mọi interface response từ BE
 * (BookListItemResponse, OrderListItemResponse...) gán được trực tiếp
 * mà không cần ép index signature.
 */
export type TableRow = Record<string, any>;

/** Ngữ cảnh truyền vào template thao tác của mỗi hàng. */
export interface TableActionContext {
  $implicit: TableRow;
  row: TableRow;
  index: number;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [NgTemplateOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="table-wrap">
      <table class="data-table">
        <thead>
          <tr>
            @for (column of columns(); track column.field) {
              <th [style.width]="column.width ?? null" [attr.data-type]="column.type ?? 'text'">
                {{ column.header }}
              </th>
            }
            @if (actions()) {
              <th class="data-table__actions-col">Thao tác</th>
            }
          </tr>
        </thead>
        <tbody>
          @for (row of rows(); track row['id'] ?? row['code']; let i = $index) {
            <tr [class.data-table__row--muted]="row['isDeleted'] === true">
              @for (column of columns(); track column.field) {
                <td [attr.data-type]="column.type ?? 'text'">
                  @switch (column.type) {
                    @case ('status') {
                      <span class="badge" [attr.data-severity]="severityOf(column, row)">
                        {{ statusLabel(column, row) }}
                      </span>
                    }
                    @case ('image') {
                      @if (row[column.field]) {
                        <img class="data-table__img" [src]="row[column.field]" alt="" loading="lazy" />
                      } @else {
                        <span class="data-table__img-placeholder"><i class="pi pi-image"></i></span>
                      }
                    }
                    @case ('money') {
                      <span class="data-table__money">{{ formatMoney(row[column.field], column.currency) }}</span>
                    }
                    @case ('date') {
                      <span class="data-table__date">{{ formatDate(row[column.field]) }}</span>
                    }
                    @case ('chips') {
                      <span class="chips">
                        @for (chip of toChips(row[column.field]); track chip) {
                          <span class="chip">{{ chip }}</span>
                        } @empty {
                          <span class="data-table__muted">—</span>
                        }
                      </span>
                    }
                    @default {
                      <span [class.data-table__muted]="row[column.field] === null || row[column.field] === undefined">
                        {{ row[column.field] ?? '—' }}
                      </span>
                    }
                  }
                </td>
              }
              @if (actions()) {
                <td class="data-table__actions-col">
                  <ng-container
                    [ngTemplateOutlet]="actions()!"
                    [ngTemplateOutletContext]="{ $implicit: row, row: row, index: i }"
                  />
                </td>
              }
            </tr>
          } @empty {
            <tr>
              <td [attr.colspan]="columns().length + (actions() ? 1 : 0)" class="data-table__empty">
                <i class="pi pi-inbox"></i>
                <span>Không có dữ liệu phù hợp</span>
              </td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [
    `
      .table-wrap {
        overflow-x: auto;
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.875rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        box-shadow: 0 1px 2px rgb(0 0 0 / 0.05);
      }

      .data-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.875rem;
      }

      .data-table thead th {
        text-align: left;
        padding: 0.75rem 1rem;
        font-size: 0.6875rem;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.06em;
        color: var(--on-surface-variant, #45464d);
        background: var(--surface-container-low, #f2f4f6);
        border-bottom: 1px solid var(--outline-variant, #c6c6cd);
        white-space: nowrap;
      }
      .data-table thead th[data-type='money'],
      .data-table thead th[data-type='number'] {
        text-align: right;
      }
      .data-table thead th[data-type='image'] { width: 4rem; }

      .data-table tbody td {
        padding: 0.75rem 1rem;
        border-bottom: 1px solid var(--surface-container-high, #e6e8ea);
        vertical-align: middle;
      }
      .data-table tbody td[data-type='money'],
      .data-table tbody td[data-type='number'] {
        text-align: right;
        font-variant-numeric: tabular-nums;
      }
      .data-table tbody tr:last-child td { border-bottom: none; }
      .data-table tbody tr { transition: background 0.15s ease; }
      .data-table tbody tr:hover { background: var(--surface-container-lowest, #f7f9fb); }
      .data-table__row--muted { opacity: 0.55; }

      .data-table__img {
        width: 2.25rem;
        height: 3rem;
        object-fit: cover;
        border-radius: 0.375rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        display: block;
      }
      .data-table__img-placeholder {
        width: 2.25rem;
        height: 3rem;
        border-radius: 0.375rem;
        background: var(--surface-container-high, #e6e8ea);
        color: var(--outline, #76777d);
        display: inline-flex;
        align-items: center;
        justify-content: center;
      }

      .data-table__money { font-weight: 600; font-variant-numeric: tabular-nums; }
      .data-table__date { color: var(--on-surface-variant, #45464d); white-space: nowrap; }
      .data-table__muted { color: var(--outline, #76777d); }
      .data-table__actions-col { text-align: right; white-space: nowrap; }
      .data-table__empty {
        text-align: center;
        padding: 3rem 1rem;
        color: var(--outline, #76777d);
      }
      .data-table__empty i { display: block; font-size: 2rem; margin-bottom: 0.5rem; opacity: 0.5; }

      .badge {
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
        padding: 0.1875rem 0.5625rem;
        border-radius: 9999px;
        font-size: 0.75rem;
        font-weight: 600;
        white-space: nowrap;
        background: var(--surface-container-high, #e6e8ea);
        color: var(--on-surface-variant, #45464d);
      }
      .badge[data-severity='success'] { background: #c8f0dc; color: #005236; }
      .badge[data-severity='warning'] { background: #ffe3b3; color: #7a4b00; }
      .badge[data-severity='danger'] { background: #ffdad6; color: #93000a; }
      .badge[data-severity='info'] { background: var(--tertiary-fixed, #d8e2ff); color: #001a42; }

      .chips { display: inline-flex; flex-wrap: wrap; gap: 0.25rem; }
      .chip {
        display: inline-flex;
        padding: 0.125rem 0.5rem;
        border-radius: 0.375rem;
        font-size: 0.6875rem;
        font-weight: 600;
        background: var(--surface-container-high, #e6e8ea);
        color: var(--on-surface-variant, #45464d);
        white-space: nowrap;
      }
    `
  ]
})
export class DataTableComponent {
  readonly columns = input<TableColumn[]>([]);
  readonly rows = input<TableRow[]>([]);
  readonly showActions = computed(() => this.actions() !== null);
  /** Template chứa các nút thao tác, nhận `row` qua ngTemplateOutletContext. */
  readonly actions = input<TemplateRef<TableActionContext> | null>(null);

  statusLabel(column: TableColumn, row: TableRow): string {
    const raw = String(row[column.field] ?? '');
    return column.statusMap?.[raw]?.label ?? raw;
  }

  severityOf(column: TableColumn, row: TableRow): string {
    const raw = String(row[column.field] ?? '');
    return column.statusMap?.[raw]?.severity ?? 'info';
  }

  toChips(value: unknown): string[] {
    if (Array.isArray(value)) {
      return value.map((item) => String(item));
    }
    if (typeof value === 'string' && value.trim()) {
      return value.split(',').map((item) => item.trim()).filter(Boolean);
    }
    return [];
  }

  formatMoney(value: unknown, currency = 'VND'): string {
    const amount = Number(value ?? 0);
    if (!Number.isFinite(amount)) {
      return '—';
    }
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency,
      maximumFractionDigits: currency === 'VND' ? 0 : 2
    }).format(amount);
  }

  formatDate(value: unknown): string {
    if (!value) {
      return '—';
    }
    const date = new Date(value as string);
    if (Number.isNaN(date.getTime())) {
      return String(value);
    }
    return new Intl.DateTimeFormat('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  }
}
