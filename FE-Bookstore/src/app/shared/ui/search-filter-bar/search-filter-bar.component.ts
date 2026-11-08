import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

export interface SearchFilterField {
  key: string;
  label: string;
  type: 'text' | 'select' | 'date';
  placeholder: string;
  options?: { label: string; value: string }[];
}

export interface SearchFilterValue {
  keyword: string;
  filters: Record<string, string>;
}

@Component({
  selector: 'app-search-filter-bar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="filter-bar">
      <div class="filter-bar__search">
        <i class="pi pi-search"></i>
        <input
          type="search"
          [placeholder]="searchPlaceholder()"
          [value]="value().keyword"
          (input)="onKeyword($event)"
        />
      </div>
      <div class="filter-bar__fields">
        @for (field of fields(); track field.key) {
          <label class="filter-bar__field">
            <span class="filter-bar__label">{{ field.label }}</span>
            @if (field.type === 'select') {
              <select [value]="value().filters[field.key]" (change)="onFilter(field.key, $event)">
                @for (option of field.options ?? []; track option.value) {
                  <option [value]="option.value">{{ option.label }}</option>
                }
              </select>
            } @else {
              <input
                [type]="field.type"
                [placeholder]="field.placeholder"
                [value]="value().filters[field.key]"
                (input)="onFilter(field.key, $event)"
              />
            }
          </label>
        }
        <button type="button" class="filter-bar__reset" (click)="reset.emit()">Xóa bộ lọc</button>
      </div>
    </div>
  `,
  styles: [
    `
      .filter-bar {
        background: var(--surface-container-lowest, #fff);
        border-radius: 0.75rem;
        padding: 1rem;
        box-shadow: 0 1px 3px rgb(0 0 0 / 0.08);
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }
      .filter-bar__search { position: relative; }
      .filter-bar__search i { position: absolute; left: 0.75rem; top: 50%; transform: translateY(-50%); color: var(--outline, #76777d); }
      .filter-bar__search input { width: 100%; padding: 0.5rem 0.75rem 0.5rem 2.5rem; border-radius: 0.5rem; border: none; background: var(--surface-container-low, #f2f4f6); }
      .filter-bar__fields { display: flex; flex-wrap: wrap; gap: 0.75rem; align-items: flex-end; }
      .filter-bar__field { display: flex; flex-direction: column; gap: 0.25rem; }
      .filter-bar__label { font-size: 0.6875rem; text-transform: uppercase; letter-spacing: 0.03em; color: var(--on-surface-variant, #45464d); }
      .filter-bar__field select, .filter-bar__field input { padding: 0.5rem 0.75rem; border-radius: 0.5rem; border: none; background: var(--surface-container-low, #f2f4f6); }
      .filter-bar__reset { border: none; background: transparent; color: var(--secondary, #006c49); font-weight: 600; cursor: pointer; padding: 0.5rem; }
    `
  ]
})
export class SearchFilterBarComponent {
  readonly fields = input<SearchFilterField[]>([]);
  readonly searchPlaceholder = input('Tìm kiếm...');
  readonly value = input<SearchFilterValue>({ keyword: '', filters: {} });
  readonly changed = output<SearchFilterValue>();
  readonly reset = output<void>();

  onKeyword(event: Event): void {
    const keyword = (event.target as HTMLInputElement).value;
    this.changed.emit({ ...this.value(), keyword });
  }

  onFilter(key: string, event: Event): void {
    const raw = (event.target as HTMLInputElement | HTMLSelectElement).value;
    this.changed.emit({ ...this.value(), filters: { ...this.value().filters, [key]: raw } });
  }
}
