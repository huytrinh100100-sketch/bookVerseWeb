import { Nullable, PagedResult } from '../../../shared/models/api.models';

export enum ArticleStatus {
  Draft = 'Draft',
  InReview = 'InReview',
  Scheduled = 'Scheduled',
  Published = 'Published',
  Archived = 'Archived'
}

export interface ArticleListItemResponse {
  id: string;
  code: string;
  title: string;
  slug: string;
  summary?: Nullable<string>;
  coverImageUrl?: Nullable<string>;
  categoryId?: Nullable<string>;
  categoryName?: Nullable<string>;
  linkedBookIds: string[];
  linkedBookTitles: string[];
  authorId?: Nullable<string>;
  authorName: string;
  editorId?: Nullable<string>;
  editorName?: Nullable<string>;
  viewCount: number;
  conversionCount: number;
  status: ArticleStatus;
  publishedAt?: Nullable<string>;
  isDeleted: boolean;
  createdAt: string;
  updatedAt?: Nullable<string>;
}

export interface ArticleDetailResponse extends ArticleListItemResponse {
  content?: Nullable<string>;
  seoTitle?: Nullable<string>;
  seoDescription?: Nullable<string>;
  updatedAt?: Nullable<string>;
}

export interface ArticleSearchRequest {
  keyword?: string;
  categoryId?: string;
  status?: ArticleStatus;
  isDeleted?: boolean;
  page?: number;
  pageSize?: number;
}

/** BE: CreateArticleRequest. */
export interface CreateArticleRequest {
  code?: string;
  title: string;
  slug?: string;
  summary?: string;
  content?: string;
  coverImageUrl?: string;
  categoryId?: string;
  authorId?: string;
  linkedBookIds?: string[];
  seoTitle?: string;
  seoDescription?: string;
  status: ArticleStatus;
  publishedAt?: string;
}

/** BE: UpdateArticleRequest. */
export interface UpdateArticleRequest {
  title?: string;
  slug?: string;
  summary?: string;
  content?: string;
  coverImageUrl?: string;
  categoryId?: string;
  authorId?: string;
  editorId?: string;
  linkedBookIds?: string[];
  seoTitle?: string;
  seoDescription?: string;
  status?: ArticleStatus;
  publishedAt?: string;
}

export type ArticlePagedResult = PagedResult<ArticleListItemResponse>;
