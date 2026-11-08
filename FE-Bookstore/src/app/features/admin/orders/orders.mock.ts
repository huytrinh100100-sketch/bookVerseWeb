import { OrderListItemResponse, OrderStatus, PaymentMethod } from './orders.models';

type OrderMock = Omit<OrderListItemResponse, 'couponCode' | 'totalItems'>;

const ORDER_MOCK_RAW: OrderMock[] = [
  { id: 'o-1041', code: 'BK-1041', customerId: 'cu-01', customerName: 'Trần Bảo Linh', customerPhone: '0901234567',
    orderDate: '2025-11-03T09:12:00Z', itemCount: 3, totalAmount: 795000, shippingAmount: 0, discountAmount: 0,
    finalAmount: 795000, paymentMethod: PaymentMethod.MoMo, orderStatus: OrderStatus.Processing, isPaid: true,
    paidAt: '2025-11-03T09:14:00Z', shippingAddress: 'So 12 Ly Nam De, So 5, Quan Ba Dinh', provinceName: 'Hà Nội', communeName: 'Phường Ly Nam De' },
  { id: 'o-1040', code: 'BK-1040', customerId: 'cu-02', customerName: 'Nguyen Thi Hoa', customerPhone: '0912233445',
    orderDate: '2025-11-03T07:40:00Z', itemCount: 1, totalAmount: 495000, shippingAmount: 30000, discountAmount: 74250,
    finalAmount: 450750, paymentMethod: PaymentMethod.VNPay, orderStatus: OrderStatus.Paid, isPaid: true,
    paidAt: '2025-11-03T07:44:00Z', shippingAddress: '45 Nguyen Van Linh, P12, Quan Binh Thanh', provinceName: 'Hồ Chí Minh', communeName: 'Phường 12' },
  { id: 'o-1039', code: 'BK-1039', customerId: 'cu-03', customerName: 'Le Van Minh', customerPhone: '0987654321',
    orderDate: '2025-11-02T16:05:00Z', itemCount: 2, totalAmount: 340000, shippingAmount: 20000, discountAmount: 0,
    finalAmount: 360000, paymentMethod: PaymentMethod.COD, orderStatus: OrderStatus.Shipped, isPaid: false,
    paidAt: null, shippingAddress: '88 Hai Ba Trung, Quan Hai Chau', provinceName: 'Da Nang', communeName: 'Quan Hai Chau' },
  { id: 'o-1038', code: 'BK-1038', customerId: 'cu-04', customerName: 'Phạm Quỳnh Anh', customerPhone: '0968877665',
    orderDate: '2025-11-02T11:22:00Z', itemCount: 4, totalAmount: 1120000, shippingAmount: 0, discountAmount: 168000,
    finalAmount: 952000, paymentMethod: PaymentMethod.OnePay, orderStatus: OrderStatus.Completed, isPaid: true,
    paidAt: '2025-11-02T11:25:00Z', shippingAddress: '170 Điện Biên Phủ, Quan 3', provinceName: 'Hồ Chí Minh', communeName: 'Phường 6' },
  { id: 'o-1037', code: 'BK-1037', customerId: 'cu-05', customerName: 'Đỗ Anh Tuấn', customerPhone: '0933445566',
    orderDate: '2025-11-01T18:30:00Z', itemCount: 1, totalAmount: 165000, shippingAmount: 30000, discountAmount: 0,
    finalAmount: 195000, paymentMethod: PaymentMethod.COD, orderStatus: OrderStatus.Cancelled, isPaid: false,
    paidAt: null, shippingAddress: '9 Tran Phu, Hà Nội', provinceName: 'Hà Nội', communeName: 'Phường Tran Phu' },
  { id: 'o-1036', code: 'BK-1036', customerId: 'cu-06', customerName: 'Hoang Thi Mai', customerPhone: '0922334455',
    orderDate: '2025-11-01T10:02:00Z', itemCount: 5, totalAmount: 1385000, shippingAmount: 0, discountAmount: 0,
    finalAmount: 1385000, paymentMethod: PaymentMethod.BankTransfer, orderStatus: OrderStatus.Pending, isPaid: false,
    paidAt: null, shippingAddress: '300 Điện Biên Phủ, Quan 5', provinceName: 'Hồ Chí Minh', communeName: 'Phường 9' }
];

export const ORDER_MOCK_ROWS: OrderListItemResponse[] = ORDER_MOCK_RAW.map((o) => ({
  ...o,
  couponCode: null,
  totalItems: o.itemCount
}));
