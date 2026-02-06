import type { Attachment } from './attachment';

export interface Appointment {
  id: string;
  data: string;
  especialidade: string;
  queixaPrincipal: string | null;
  observacoesGerais: string | null;
  diagnosticos: string | null;
  doctorId: string;
  doctorNome: string;
  createdAt: string;
  updatedAt: string | null;
  attachments: Attachment[] | null;
}

export interface CreateAppointmentRequest {
  data: string;
  especialidade: string;
  queixaPrincipal: string | null;
  observacoesGerais: string | null;
  diagnosticos: string | null;
  doctorId: string;
}

export interface UpdateAppointmentRequest {
  data: string;
  especialidade: string;
  queixaPrincipal: string | null;
  observacoesGerais: string | null;
  diagnosticos: string | null;
  doctorId: string;
}
