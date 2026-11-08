import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { StorageService } from './storage.service';
import { AuthApiService } from './auth-api.service';
import { UserProfile } from './auth.models';

const STORAGE_KEY = 'bookverse.session';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly api = inject(AuthApiService);
  private readonly storage = inject(StorageService);

  private readonly user = signal<UserProfile | null>(this.storage.get<UserProfile>(STORAGE_KEY));

  readonly currentUser = this.user.asReadonly();
  readonly accessToken = computed(() => this.user()?.accessToken ?? null);
  readonly roles = computed(() => this.user()?.roles ?? []);
  readonly permissions = computed(() => this.user()?.permissions ?? []);
  readonly isAuthenticated = computed(() => !!this.user()?.accessToken);
  readonly isSuperAdmin = computed(() => this.roles().includes('Admin'));

  /** Thong tin user hien tai (BE tra ve trong AuthResponse.User). */
  readonly profile = computed(() => this.user()?.user ?? null);
  readonly displayName = computed(() => this.user()?.user?.fullName ?? this.user()?.user?.userName ?? '');

  hasPermission(code: string): boolean {
    if (this.isSuperAdmin()) {
      return true;
    }

    return this.permissions().includes(code);
  }

  hasAnyPermission(codes: string[]): boolean {
    return codes.some((code) => this.hasPermission(code));
  }

  async login(userNameOrEmail: string, password: string): Promise<UserProfile | null> {
    const response = await firstValueFrom(this.api.login({ userNameOrEmail, password }));

    if (!response.isSuccess || !response.data) {
      return null;
    }

    const profile = response.data;
    this.setSession(profile);
    return profile;
  }

  setSession(profile: UserProfile): void {
    this.user.set(profile);
    this.storage.set(STORAGE_KEY, profile);
  }

  clearSession(): void {
    this.user.set(null);
    this.storage.remove(STORAGE_KEY);
  }
}
