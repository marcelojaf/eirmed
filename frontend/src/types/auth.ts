export interface UserInfo {
  id: string;
  nome: string;
  email: string;
  profilePictureUrl: string | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}
