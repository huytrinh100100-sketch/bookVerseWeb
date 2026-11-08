import { Nullable, PagedResult } from '../../../shared/models/api.models';

/** Root code cho cay danh muc trong he thong (moi cay la 1 root rieng). */
export const CATEGORY_ROOTS = {
  Book: 'BOOK',
  Publisher: 'PUBLISHER',
  Format: 'FORMAT',
  Language: 'LANGUAGE',
  Audience: 'AUDIENCE',
  Edition: 'EDITION'
} as const;

export type CategoryRootCode = (typeof CATEGORY_ROOTS)[keyof typeof CATEGORY_ROOTS];

export interface CategoryListItemResponse {
  id: string;
  code: string;
  name: string;
  slug?: Nullable<string>;
  rootId: string;
  rootCode: string;
  rootName: string;
  type?: Nullable<string>;
  typeLabel?: Nullable<string>;
  parentId?: Nullable<string>;
  parentName?: Nullable<string>;
  sortOrder: number;
  description?: Nullable<string>;
  iconUrl?: Nullable<string>;
  isMegaMenu: boolean;
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;
}

export interface CategoryDetailResponse extends CategoryListItemResponse {
  updatedAt?: Nullable<string>;
  deletedAt?: Nullable<string>;
  deletedBy?: Nullable<string>;
}

export interface CategoryTreeNodeResponse {
  id: string;
  code: string;
  name: string;
  slug?: Nullable<string>;
  rootId: string;
  rootCode: string;
  rootName: string;
  type?: Nullable<string>;
  typeLabel?: Nullable<string>;
  parentId?: Nullable<string>;
  sortOrder: number;
  isMegaMenu: boolean;
  isActive: boolean;
  isDeleted: boolean;
  bookCount: number;
  children: CategoryTreeNodeResponse[];
}

export interface CreateCategoryRequest {
  code: string;
  name: string;
  slug?: string;
  parentId?: string | null;
  rootCode?: string;
  sortOrder: number;
  description?: string;
  iconUrl?: string;
  isMegaMenu: boolean;
  isActive: boolean;
}

/** BE: UpdateCategoryRequest. */
export interface UpdateCategoryRequest {
  name?: string;
  slug?: string;
  sortOrder?: number;
  description?: string;
  iconUrl?: string;
  isMegaMenu?: boolean;
  isActive?: boolean;
}

/** BE: MoveCategoryRequest. */
export interface MoveCategoryRequest {
  parentId?: string | null;
  sortOrder?: number;
}

/** BE: UpdateCategoryStatusRequest. */
export interface UpdateCategoryStatusRequest {
  isActive: boolean;
}

/** BE: ReorderCategoriesRequest. */
export interface ReorderCategoriesRequest {
  items: { id: string; parentId?: string | null; sortOrder: number }[];
}

export type CategoryPagedResult = PagedResult<CategoryListItemResponse>;
