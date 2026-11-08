import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: number;
  severity: 'success' | 'info' | 'warn' | 'error';
  summary: string;
  detail?: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private counter = 0;
  readonly messages = signal<ToastMessage[]>([]);

  success(summary: string, detail?: string): void {
    this.push('success', summary, detail);
  }

  info(summary: string, detail?: string): void {
    this.push('info', summary, detail);
  }

  warn(summary: string, detail?: string): void {
    this.push('warn', summary, detail);
  }

  error(summary: string, detail?: string): void {
    this.push('error', summary, detail);
  }

  dismiss(id: number): void {
    this.messages.update((list) => list.filter((m) => m.id !== id));
  }

  private push(severity: ToastMessage['severity'], summary: string, detail?: string): void {
    const id = ++this.counter;
    this.messages.update((list) => [...list, { id, severity, summary, detail }]);
    setTimeout(() => this.dismiss(id), 4000);
  }
}
