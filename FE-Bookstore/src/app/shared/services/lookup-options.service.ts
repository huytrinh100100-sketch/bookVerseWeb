import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../../core/core.tokens';
import { ApiResponse } from '../models/api.models';
import { OptionItem } from '../models/common.models';

interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

interface CategoryItem {
  id: string;
  name: string;
  parentId?: string | null;
  rootCode?: string | null;
}

/** Muc danh muc da duoc chuan hoa cho dropdown. */
interface LookupCategory {
  label: string;
  value: string;
  rootCode?: string | null;
}

interface PartnerItem {
  id: string;
  name: string;
  type: string;
}

/**
 * Tai cac danh sach chon dung chung cho form (danh muc, tac gia, nha xuat ban).
 *
 * Cac trang admin deu can dropdown nay, nen giu mot instance singleton de tranh
 * goi lai API nhieu lan. Du lieu duoc cache trong signal va chi load khi thieu.
 */
@Injectable({ providedIn: 'root' })
export class LookupOptionsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  private readonly categories = signal<OptionItem[]>([]);
  private readonly bookCategories = signal<OptionItem[]>([]);
  private readonly articleCategories = signal<OptionItem[]>([]);
  private readonly authors = signal<OptionItem[]>([]);
  private readonly publishers = signal<OptionItem[]>([]);

  private loaded = false;
  private loading?: Promise<void>;

  /** Danh muc san pham (cay "Danh muc sach"). */
  readonly categoryOptions = computed(() => this.bookCategories());
  /** Danh muc bai viet (cay "Chu de bai viet"). */
  readonly articleCategoryOptions = computed(() => this.articleCategories());
  readonly authorOptions = computed(() => this.authors());
  readonly publisherOptions = computed(() => this.publishers());

  /** Load mot lan duy nhat, cac trang sau chi doc signal da cache. */
  async ensureLoaded(): Promise<void> {
    if (this.loaded) {
      return;
    }
    this.loading ??= this.fetchAll();
    return this.loading;
  }

  private async fetchAll(): Promise<void> {
    const [allCategories, authors, publishers] = await Promise.all([
      this.fetchCategories(),
      this.fetchPartners('Author'),
      this.fetchPartners('Publisher')
    ]);

    this.categories.set(allCategories);
    this.bookCategories.set(allCategories.filter((x) => x.rootCode === 'book-category'));
    this.articleCategories.set(allCategories.filter((x) => x.rootCode === 'article-category'));
    this.authors.set(authors);
    this.publishers.set(publishers);
    this.loaded = true;
  }

  private async fetchCategories(): Promise<LookupCategory[]> {
    const params = new HttpParams().set('pageSize', '200').set('isDeleted', 'false');
    const response = await firstValueFrom(
      this.http.get<ApiResponse<PagedResult<CategoryItem>>>(`${this.baseUrl}/categories`, { params })
    );
    return (response.data?.items ?? [])
      // Bo cac node goc (khong co parent) - chi hien danh muc con cho dropdown.
      .filter((item) => !!item.parentId)
      .map((item) => ({ label: item.name, value: item.id, rootCode: item.rootCode }));
  }

  private async fetchPartners(type: string): Promise<OptionItem[]> {
    const params = new HttpParams().set('pageSize', '200').set('type', type).set('isDeleted', 'false');
    const response = await firstValueFrom(
      this.http.get<ApiResponse<PagedResult<PartnerItem>>>(`${this.baseUrl}/partners`, { params })
    );
    return (response.data?.items ?? []).map((item) => ({ label: item.name, value: item.id }));
  }
}
