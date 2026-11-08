import { DashboardOverviewResponse } from './dashboard.models';

export const DASHBOARD_MOCK: DashboardOverviewResponse = {
  metrics: [
    { label: 'Doanh thu tháng', value: '842.600.000 d', trendValue: '+8.2% so với tháng trước', trendDirection: 'up', icon: 'pi pi-wallet', tone: 'success' },
    { label: 'Đơn hàng moi', value: '2,845', trendValue: '+214 đơn trong 30 ngày', trendDirection: 'up', icon: 'pi pi-receipt', tone: 'primary' },
    { label: 'Giá trị tồn kho', value: '390.200.000 d', trendValue: '+2.4% curated profit', trendDirection: 'up', icon: 'pi pi-box', tone: 'info' },
    { label: 'Cần nhắc mức thấp', value: '14 cuốn', trendValue: 'Cần nhập gấp hôm nay', trendDirection: 'down', icon: 'pi pi-exclamation-triangle', tone: 'danger' }
  ],
  categoryPerformance: [
    { categoryName: 'Văn học & Văn học đẳng', revenue: 284000000, orderCount: 1120, sharePercent: 33.7 },
    { categoryName: 'Công nghệ & Kinh doanh', revenue: 218000000, orderCount: 864, sharePercent: 25.9 },
    { categoryName: 'Khoa học & Xã hội', revenue: 156000000, orderCount: 512, sharePercent: 18.5 },
    { categoryName: 'Kinh doanh & Tài chính', revenue: 104000000, orderCount: 224, sharePercent: 12.4 },
    { categoryName: 'Manga & Comic', revenue: 80600000, orderCount: 125, sharePercent: 9.5 }
  ],
  recentOrders: [
    { id: 'o-1041', code: 'BK-1041', customerName: 'Trần Bảo Linh', previewItem: 'Rừng Na Uy + 2', amount: 795000, paymentMethod: 'MoMo', orderStatus: 'Processing', orderDate: '2025-11-03T09:12:00Z' },
    { id: 'o-1040', code: 'BK-1040', customerName: 'Nguyen Thi Hoa', previewItem: '1Q84 Boxtet', amount: 450750, paymentMethod: 'VNPay', orderStatus: 'Paid', orderDate: '2025-11-03T07:40:00Z' },
    { id: 'o-1039', code: 'BK-1039', customerName: 'Le Van Minh', previewItem: 'Clean Code', amount: 360000, paymentMethod: 'COD', orderStatus: 'Shipped', orderDate: '2025-11-02T16:05:00Z' },
    { id: 'o-1038', code: 'BK-1038', customerName: 'Phạm Quỳnh Anh', previewItem: 'Sapiens + 3', amount: 952000, paymentMethod: 'OnePay', orderStatus: 'Completed', orderDate: '2025-11-02T11:22:00Z' }
  ],
  lowStockAlerts: [
    { bookId: 'b-08', title: 'Đời Mâu Chiến Muôn Cai', stockForSell: 6, stockForRent: 3, warehouseName: 'Kho HCM Nam' },
    { bookId: 'b-09', title: 'Atomic Habits', stockForSell: 5, stockForRent: 0, warehouseName: 'Kho Hanoi Chinh' },
    { bookId: 'b-06', title: 'Hoá Thân (Die Verwandlung)', stockForSell: 8, stockForRent: 1, warehouseName: 'Kho Da Nang' }
  ],
  revenueSeries: [
    { label: 'T5', value: 620000000 },
    { label: 'T6', value: 688000000 },
    { label: 'T7', value: 712000000 },
    { label: 'T8', value: 690000000 },
    { label: 'T9', value: 780000000 },
    { label: 'T10', value: 842600000 }
  ]
};
