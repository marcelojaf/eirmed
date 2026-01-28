export type BloodType =
  | 'APositive'
  | 'ANegative'
  | 'BPositive'
  | 'BNegative'
  | 'ABPositive'
  | 'ABNegative'
  | 'OPositive'
  | 'ONegative';

export interface UserProfile {
  id: string;
  nome: string;
  email: string;
  profilePictureUrl: string | null;
  dataNascimento: string | null;
  tipoSanguineo: BloodType | null;
  alergias: string | null;
  observacoesGerais: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateUserProfileRequest {
  nome: string;
  dataNascimento: string | null;
  tipoSanguineo: BloodType | null;
  alergias: string | null;
  observacoesGerais: string | null;
}

export const bloodTypeLabels: Record<BloodType, string> = {
  APositive: 'A+',
  ANegative: 'A-',
  BPositive: 'B+',
  BNegative: 'B-',
  ABPositive: 'AB+',
  ABNegative: 'AB-',
  OPositive: 'O+',
  ONegative: 'O-',
};
