import { Nullable, PagedResult } from '../../../shared/models/api.models';
import { BookListItemResponse } from '../books/books.models';

export enum PartnerType {
  Author = 'Author',
  Publisher = 'Publisher'
}

export enum AgreementStatus {
  Draft = 'Draft',
  Active = 'Active',
  Expired = 'Expired',
  Terminated = 'Terminated'
}

/** Tác giả & NXB - 2 bang tach biet, dung chung khung hien thi. */
export interface PartnerListItemResponse {
  id: string;
  code: string;
  name: string;
  type: PartnerType;
  country?: Nullable<string>;
  website?: Nullable<string>;
  email?: Nullable<string>;
  phoneNumber?: Nullable<string>;
  biography?: Nullable<string>;
  description?: Nullable<string>;
  avatarUrl?: Nullable<string>;
  logoUrl?: Nullable<string>;
  bookCount: number;
  agreementStatus: AgreementStatus;
  agreementExpiresAt?: Nullable<string>;
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;
  updatedAt?: Nullable<string>;
}

export interface PartnerDetailResponse extends PartnerListItemResponse {
  books: BookListItemResponse[];
}

export interface PartnerSearchRequest {
  keyword?: string;
  type?: PartnerType;
  agreementStatus?: AgreementStatus;
  country?: string;
  isActive?: boolean;
  isDeleted?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CreatePartnerRequest {
  code?: string;
  name: string;
  type: PartnerType;
  country?: string;
  website?: string;
  email?: string;
  phoneNumber?: string;
  biography?: string;
  description?: string;
  avatarUrl?: string;
  logoUrl?: string;
  agreementStatus: AgreementStatus;
  agreementExpiresAt?: string;
  isActive?: boolean;
}

/** BE: UpdatePartnerRequest. */
export interface UpdatePartnerRequest {
  name?: string;
  country?: string;
  website?: string;
  email?: string;
  phoneNumber?: string;
  biography?: string;
  description?: string;
  avatarUrl?: string;
  logoUrl?: string;
  agreementStatus?: AgreementStatus;
  agreementExpiresAt?: string;
  isActive?: boolean;
}

export type PartnerPagedResult = PagedResult<PartnerListItemResponse>;
