export interface User {
  displayName: string;
  email: string;
  token: string;
  phoneNumber?: string;
  phoneNumber2?: string;
  profilePictureUrl?: string;
  hasPassword?: boolean;
}
