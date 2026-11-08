import { Nullable } from '../../../shared/models/api.models';

export interface DashboardMetric {
  label: string;
  value: string;
  trendValue: string;
  trendDirection: 'up' | 'down' | 'flat';
  icon: string;
  tone: 'primary' | 'success' | 'warning' | 'danger' | 'info';
}

export interface CategoryPerformance {
  categoryName: string;
  revenue: number;
  orderCount: number;
  sharePercent: number;
}

export interface RecentOrder {
  id: string;
  code: string;
  customerName: string;
  previewItem: string;
  amount: number;
  paymentMethod: string;
  orderStatus: string;
  orderDate: string;
}

export interface LowStockAlert {
  bookId: string;
  title: string;
  stockForSell: number;
  stockForRent: number;
  warehouseName?: Nullable<string>;
}

export interface DashboardOverviewResponse {
  metrics: DashboardMetric[];
  categoryPerformance: CategoryPerformance[];
  recentOrders: RecentOrder[];
  lowStockAlerts: LowStockAlert[];
  revenueSeries: { label: string; value: number }[];
}
