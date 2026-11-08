import { Nullable, PagedResult } from '../../../shared/models/api.models';

export enum ReviewStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Flagged = 'Flagged',
  Spam = 'Spam'
}

export enum Sentiment {
  Positive = 'Positive',
  Neutral = 'Neutral',
  Negative = 'Negative'
}

export interface ReviewListItemResponse {
  id: string;
  bookId: string;
  bookTitle: string;
  coverImageUrl?: Nullable<string>;
  customerId: string;
  customerName: string;
  rating: number;
  title?: Nullable<string>;
  content: string;
  status: ReviewStatus;
  sentiment: Sentiment;
  hasMedia: boolean;
  mediaUrls: string[];
  isVerifiedPurchase: boolean;
  helpfulCount: number;
  createdAt: string;
  replyContent?: Nullable<string>;
}

export interface ReviewSearchRequest {
  keyword?: string;
  bookId?: string;
  rating?: number;
  status?: ReviewStatus;
  sentiment?: Sentiment;
  isDeleted?: boolean;
  page?: number;
  pageSize?: number;
}

/** BE: ApproveReviewRequest. */
export interface ApproveReviewRequest {
  replyContent?: string;
}

/** BE: RejectReviewRequest. */
export interface RejectReviewRequest {
  reason: string;
}

/** BE: FlagReviewRequest. */
export interface FlagReviewRequest {
  reason: string;
}

export type ReviewPagedResult = PagedResult<ReviewListItemResponse>;
