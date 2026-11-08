export interface LoginRequest {
  userNameOrEmail: string;
  password: string;
}

/**
 * User sau khi dang nhap. BE tra ve trong `AuthResponse.User` (AuthUserDto).
 * Ten field giong hoa camelCase de khop voi BE (options.JsonSerializerOptions.PropertyNamingPolicy = CamelCase).
 */
export interface AuthUser {
  id: string;
  userName: string;
  email?: string | null;
  fullName: string;
  phoneNumber?: string | null;
  avatarUrl?: string | null;
  vipTier: string;
  rewardPoints: number;
}

/**
 * Payload BE tra ve o `POST /auth/login` va `POST /auth/refresh`.
 * Khop voi `BookVerse.Core.Models.Auth.AuthResponse`.
 */
export interface UserProfile {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AuthUser;
  roles: string[];
  permissions: string[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
}
