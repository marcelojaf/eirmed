export interface Doctor {
  id: string;
  nome: string;
  especialidade: string;
  registroProfissional: string | null;
  clinicaHospital: string | null;
  contato: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateDoctorRequest {
  nome: string;
  especialidade: string;
  registroProfissional: string | null;
  clinicaHospital: string | null;
  contato: string | null;
}

export interface UpdateDoctorRequest {
  nome: string;
  especialidade: string;
  registroProfissional: string | null;
  clinicaHospital: string | null;
  contato: string | null;
}

export const especialidadesComuns = [
  'Cardiologista',
  'Clínico Geral',
  'Dermatologista',
  'Endocrinologista',
  'Fisioterapeuta',
  'Gastroenterologista',
  'Ginecologista',
  'Neurologista',
  'Nutricionista',
  'Oftalmologista',
  'Ortopedista',
  'Otorrinolaringologista',
  'Pediatra',
  'Pneumologista',
  'Psicólogo',
  'Psiquiatra',
  'Reumatologista',
  'Urologista',
] as const;
