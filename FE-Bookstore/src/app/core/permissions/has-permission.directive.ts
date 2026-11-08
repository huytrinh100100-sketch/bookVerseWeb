import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthStore } from '../auth/auth.store';

/**
 * <button *hasPermission="PermissionConstants.BookCreate">Them moi</button>
 *
 * Luu y: day la STRUCTURAL directive, nen `ElementRef.nativeElement` chi la
 * comment anchor (khong phai element that) - khong co `.style`. Neu gan
 * `style.display` se nem "Cannot set properties of undefined" va lam Angular
 * dung render giua chung, khien phan con lai cua trang bi trong.
 * Vi du quyen chi can render/hide embedded view, khong can doi style.
 */
@Directive({ selector: '[hasPermission]', standalone: true })
export class HasPermissionDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly authStore = inject(AuthStore);

  readonly hasPermission = input.required<string>();
  private rendered = false;

  constructor() {
    effect(() => {
      const allowed = this.authStore.hasPermission(this.hasPermission());

      if (allowed && !this.rendered) {
        this.viewContainer.createEmbeddedView(this.templateRef);
        this.rendered = true;
      } else if (!allowed && this.rendered) {
        this.viewContainer.clear();
        this.rendered = false;
      }
    });
  }
}
