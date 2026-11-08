import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

export interface DialogFieldOption {
  label: string;
  value: string;
}

export interface DialogField {
  /** Ten field - khop voi key cua object model trong form. */
  name: string;
  label: string;
  type: 'text' | 'number' | 'select' | 'textarea' | 'checkbox' | 'password' | 'date' | 'money' | 'chips';
  placeholder?: string;
  required?: boolean;
  /**Chi ap dung cho type = 'number'. */
  min?: number;
  max?: number;
  step?: number;
  /** Chi ap dung cho type = 'select'. */
  options?: DialogFieldOption[];
  /** Hien thi nhieu dong (select) - mac dinh true khi co options. */
  multiple?: boolean;
  /** Gom 2 field vao cung mot hang (chi ap dung cho text/number/select). */
  half?: boolean;
  /** Loi bao rieng cho field (dung khi validate). */
  error?: string;
  hint?: string;
  disabled?: boolean;
}

/**
 * Modal dung chung cho cac thao tac them / sua / xoa.
 *
 * - `fields` khai bao cac truong nhap -> component tu sinh form.
 * - `values` gia tri hien tai (khi sua).
 * - `mode` = 'view' se khoa input va an nut Luu.
 */
@Component({
  selector: 'app-form-dialog',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="overlay" (click)="onOverlayClick($event)">
      <div class="dialog" role="dialog" aria-modal="true">
        <header class="dialog__head">
          <div>
            <h2 class="dialog__title">{{ title() }}</h2>
            @if (subtitle()) {
              <p class="dialog__subtitle">{{ subtitle() }}</p>
            }
          </div>
          <button type="button" class="dialog__close" (click)="cancelled.emit()" aria-label="Đóng">
            <i class="pi pi-times"></i>
          </button>
        </header>

        <div class="dialog__body">
          @if (fields().length) {
            <div class="dialog__grid">
              @for (field of fields(); track field.name) {
                <label class="field" [class.field--half]="field.half">
                  <span class="field__label">
                    {{ field.label }}
                    @if (field.required) {
                      <span class="field__required">*</span>
                    }
                  </span>

                  @switch (field.type) {
                    @case ('textarea') {
                      <textarea
                        [rows]="3"
                        [placeholder]="field.placeholder ?? ''"
                        [disabled]="isReadonly() || field.disabled"
                        [value]="stringValue(field.name)"
                        (input)="onInput(field, $event)"
                      ></textarea>
                    }
                    @case ('select') {
                      <select
                        [multiple]="field.multiple ?? true"
                        [disabled]="isReadonly() || field.disabled"
                        (change)="onSelect(field, $event)"
                      >
                        @if (!(field.multiple ?? true)) {
                          <option value="">-- Chọn --</option>
                        }
                        @for (option of field.options ?? []; track option.value) {
                          <option [value]="option.value" [selected]="isSelected(field.name, option.value)">
                            {{ option.label }}
                          </option>
                        }
                      </select>
                    }
                    @case ('checkbox') {
                      <span class="field__checkbox">
                        <input
                          type="checkbox"
                          [checked]="boolValue(field.name)"
                          [disabled]="isReadonly() || field.disabled"
                          (change)="onCheckbox(field, $event)"
                        />
                        <span>{{ field.placeholder ?? 'Bật' }}</span>
                      </span>
                    }
                    @case ('money') {
                      <input
                        type="number"
                        [placeholder]="field.placeholder ?? ''"
                        [disabled]="isReadonly() || field.disabled"
                        [value]="stringValue(field.name)"
                        (input)="onInput(field, $event)"
                      />
                    }
                    @case ('chips') {
                      <span class="field__hint">Danh sách nhiều mục, dùng ô chọn ở trên.</span>
                    }
                    @default {
                      <input
                        [type]="field.type"
                        [placeholder]="field.placeholder ?? ''"
                        [disabled]="isReadonly() || field.disabled"
                        [value]="stringValue(field.name)"
                        (input)="onInput(field, $event)"
                      />
                    }
                  }

                  @if (field.error) {
                    <span class="field__error">{{ field.error }}</span>
                  } @else if (field.hint) {
                    <span class="field__hint">{{ field.hint }}</span>
                  }
                </label>
              }
            </div>
          } @else {
            <ng-content />
          }
        </div>

        <footer class="dialog__foot">
          @if (errorMessage()) {
            <span class="dialog__error">{{ errorMessage() }}</span>
          }
          <div class="dialog__actions">
            <button type="button" class="btn btn--ghost" (click)="cancelled.emit()">
              {{ cancelLabel() }}
            </button>
            @if (!isReadonly()) {
              <button type="button" class="btn btn--primary" [disabled]="submitting()" (click)="submitted.emit()">
                @if (submitting()) {
                  <i class="pi pi-spin pi-spinner"></i> Đang lưu...
                } @else {
                  <i class="pi pi-check"></i> {{ submitLabel() }}
                }
              </button>
            }
          </div>
        </footer>
      </div>
    </div>
  `,
  styles: [
    `
      .overlay {
        position: fixed;
        inset: 0;
        background: rgb(0 0 0 / 0.45);
        display: flex;
        align-items: flex-start;
        justify-content: center;
        padding: 3rem 1rem;
        z-index: 1000;
        overflow-y: auto;
        backdrop-filter: blur(2px);
      }
      .dialog {
        width: 100%;
        max-width: 42rem;
        background: var(--surface-container-lowest, #fff);
        border-radius: 1rem;
        box-shadow: 0 12px 40px rgb(0 0 0 / 0.25);
        display: flex;
        flex-direction: column;
        overflow: hidden;
      }
      .dialog__head {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 1rem;
        padding: 1.25rem 1.5rem;
        border-bottom: 1px solid var(--outline-variant, #c6c6cd);
      }
      .dialog__title { margin: 0; font-size: 1.125rem; }
      .dialog__subtitle { margin: 0.25rem 0 0; font-size: 0.8125rem; color: var(--on-surface-variant, #45464d); }
      .dialog__close {
        border: none;
        background: transparent;
        cursor: pointer;
        color: var(--on-surface-variant, #45464d);
        font-size: 1.125rem;
        padding: 0.25rem;
        border-radius: 0.375rem;
        flex-shrink: 0;
      }
      .dialog__close:hover { background: var(--surface-container-low, #f2f4f6); }
      .dialog__body { padding: 1.25rem 1.5rem; max-height: 65vh; overflow-y: auto; }
      .dialog__grid { display: grid; grid-template-columns: 1fr; gap: 1rem; }
      .field { display: flex; flex-direction: column; gap: 0.375rem; }
      .field__label { font-size: 0.75rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.03em; color: var(--on-surface-variant, #45464d); }
      .field__required { color: var(--error, #ba1a1a); }
      .field input, .field select, .field textarea {
        padding: 0.5rem 0.75rem;
        border-radius: 0.5rem;
        border: 1px solid var(--outline-variant, #c6c6cd);
        background: var(--surface-container-lowest, #fff);
        font-family: inherit;
        font-size: 0.875rem;
        width: 100%;
      }
      .field input:disabled, .field select:disabled, .field textarea:disabled {
        background: var(--surface-container-low, #f2f4f6);
        color: var(--on-surface-variant, #45464d);
      }
      .field select[multiple] { min-height: 8rem; }
      .field__checkbox { display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; }
      .field__error { font-size: 0.75rem; color: var(--error, #ba1a1a); }
      .field__hint { font-size: 0.75rem; color: var(--outline, #76777d); }
      .dialog__foot {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        padding: 1rem 1.5rem;
        border-top: 1px solid var(--outline-variant, #c6c6cd);
        background: var(--surface-container-low, #f2f4f6);
      }
      .dialog__error { font-size: 0.8125rem; color: var(--error, #ba1a1a); }
      .dialog__actions { display: flex; gap: 0.5rem; margin-left: auto; }
      .btn { border: none; border-radius: 0.5rem; padding: 0.5rem 1rem; cursor: pointer; font-size: 0.875rem; font-weight: 600; display: inline-flex; align-items: center; gap: 0.375rem; }
      .btn--ghost { background: var(--surface-container-high, #e6e8ea); }
      .btn--primary { background: var(--secondary, #006c49); color: #fff; }
      .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    `
  ]
})
export class FormDialogComponent {
  readonly title = input('Thông tin');
  readonly subtitle = input('');
  readonly fields = input<DialogField[]>([]);
  readonly values = input<Record<string, unknown>>({});
  readonly mode = input<'create' | 'edit' | 'view'>('create');
  readonly submitLabel = input('Lưu');
  readonly cancelLabel = input('Hủy');
  readonly submitting = input(false);
  readonly errorMessage = input('');

  readonly submitted = output<void>();
  readonly cancelled = output<void>();

  readonly isReadonly = () => this.mode() === 'view';

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.cancelled.emit();
    }
  }

  stringValue(name: string): string {
    const value = this.values()[name];
    return value === undefined || value === null ? '' : String(value);
  }

  boolValue(name: string): boolean {
    return Boolean(this.values()[name]);
  }

  isSelected(name: string, optionValue: string): boolean {
    const current = this.values()[name];
    if (Array.isArray(current)) {
      return current.some((item) => String(item) === optionValue);
    }
    return String(current ?? '') === optionValue;
  }

  onInput(field: DialogField, event: Event): void {
    const target = event.target as HTMLInputElement;
    let value: unknown = target.value;

    if (field.type === 'number') {
      value = target.value === '' ? null : Number(target.value);
    }

    // Component cha nhan gia tri moi qua output valuesChange.
    this.valuesChange.emit({ ...this.values(), [field.name]: value });
  }

  onSelect(field: DialogField, event: Event): void {
    const target = event.target as HTMLSelectElement;

    if (field.multiple ?? true) {
      const selected = Array.from(target.selectedOptions).map((option) => option.value);
      this.valuesChange.emit({ ...this.values(), [field.name]: selected });
      return;
    }

    this.valuesChange.emit({ ...this.values(), [field.name]: target.value });
  }

  onCheckbox(field: DialogField, event: Event): void {
    const target = event.target as HTMLInputElement;
    this.valuesChange.emit({ ...this.values(), [field.name]: target.checked });
  }

  /** Signal output dung de thong bao gia tri moi cho component cha. */
  readonly valuesChange = output<Record<string, unknown>>();
}
