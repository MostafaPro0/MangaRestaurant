export interface SaasPlan {
  id: number;
  name: string;
  nameAr: string;
  monthlyPrice: number;
  maxProducts: number;
  maxStaff: number;
  hasLuckyRewards: boolean;
  hasAdvancedReports: boolean;
  hasCustomDomain: boolean;
  hasDeliveryTracking: boolean;
  hasEmailNotifications: boolean;
  sortOrder: number;
}

export interface SaasTenant {
  name: string;
  nameAr: string;
  slug: string;
  logoUrl: string;
  customDomain?: string;
}
