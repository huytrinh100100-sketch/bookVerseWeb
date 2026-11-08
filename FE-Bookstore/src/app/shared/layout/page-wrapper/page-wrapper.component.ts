import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-page-wrapper',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="page"><ng-content /></div>`,
  styles: [
    `
      .page { display: flex; flex-direction: column; gap: 1.5rem; }
    `
  ]
})
export class PageWrapperComponent {}
