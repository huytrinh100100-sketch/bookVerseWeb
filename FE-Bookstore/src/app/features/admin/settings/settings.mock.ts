import { CarrierResponse, PaymentGatewayResponse, StoreSettingResponse } from './settings.models';

export const STORE_SETTING_MOCK: StoreSettingResponse = {
  storeBrandName: 'BookVerse',
  supportEmail: 'support@bookverse.vn',
  hotline: '1900 1234',
  headquartersAddress: 'Số 88 Hàng gai, Hoàn Kiếm, Hà Nội',
  defaultCurrency: 'VND',
  operatingTimezone: 'GMT+07:00',
  freeShippingThreshold: 500000,
  maintenanceNotice: 'Dang ton tai lich phuc vu lau dai thuong nhat trong ngay 25/12.',
  staffInactivityTimeoutMinutes: 30,
  updatedAt: '2025-10-28T08:00:00Z'
};

export const PAYMENT_GATEWAY_MOCK: PaymentGatewayResponse[] = [
  { id: 'pg-01', code: 'MOMO', name: 'MoMo E-Wallet', provider: 'MoMo', isEnabled: true, isTestMode: false, supportedMethods: ['QR', 'Deeplink'] },
  { id: 'pg-02', code: 'VNPAY', name: 'VNPay QR Gateway', provider: 'VNPay', isEnabled: true, isTestMode: false, supportedMethods: ['QR', 'NAPAS'] },
  { id: 'pg-03', code: 'ONEPAY', name: 'Cards (OnePay)', provider: 'OnePay', isEnabled: true, isTestMode: false, supportedMethods: ['Visa', 'Mastercard', 'JCB'] },
  { id: 'pg-04', code: 'COD', name: 'Cash on Delivery', provider: 'Internal', isEnabled: true, isTestMode: false, supportedMethods: ['COD'] }
];

export const CARRIER_MOCK: CarrierResponse[] = [
  { id: 'cr-01', name: 'Giao Hàng Nhanh (GHN)', code: 'GHN', price: 30000, estimatedDays: 1, isEnabled: true },
  { id: 'cr-02', name: 'GHTK Priority Fulfillment', code: 'GHTK', price: 25000, estimatedDays: 2, isEnabled: true },
  { id: 'cr-03', name: 'ViettelPost Standard Parcel', code: 'VTP', price: 20000, estimatedDays: 3, isEnabled: true }
];
