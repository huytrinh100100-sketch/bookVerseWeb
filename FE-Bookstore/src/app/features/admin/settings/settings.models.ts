export interface StoreSettingResponse {
  storeBrandName: string;
  supportEmail: string;
  hotline: string;
  headquartersAddress: string;
  defaultCurrency: string;
  operatingTimezone: string;
  freeShippingThreshold: number;
  maintenanceNotice: string;
  staffInactivityTimeoutMinutes: number;
  updatedAt?: string;
}

export interface PaymentGatewayResponse {
  id: string;
  code: string;
  name: string;
  provider: string;
  isEnabled: boolean;
  isTestMode: boolean;
  supportedMethods: string[];
}

export interface CarrierResponse {
  id: string;
  name: string;
  code: string;
  price: number;
  estimatedDays: number;
  isEnabled: boolean;
}

export interface UpdateStoreSettingRequest {
  storeBrandName: string;
  supportEmail: string;
  hotline: string;
  headquartersAddress: string;
  defaultCurrency: string;
  operatingTimezone: string;
  freeShippingThreshold: number;
  maintenanceNotice: string;
  staffInactivityTimeoutMinutes: number;
}
