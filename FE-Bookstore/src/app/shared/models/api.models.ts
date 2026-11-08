export type Nullable<T> = T | null;

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: Nullable<T>;
  errors?: string[] | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface PagingRequest {
  page?: number;
  pageSize?: number;
  keyword?: string;
}

export interface SortRequest {
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}
