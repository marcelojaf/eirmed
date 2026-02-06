export type ExamType = 'Blood' | 'Image' | 'Functional' | 'Laboratory' | 'Other';

export interface Exam {
  id: string;
  tipoExame: ExamType;
  tipoExameDescricao: string;
  nome: string;
  dataRealizacao: string;
  dataResultado: string | null;
  laboratorio: string | null;
  resultados: string | null;
  fileUrl: string | null;
  appointmentId: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateExamRequest {
  tipoExame: ExamType;
  nome: string;
  dataRealizacao: string;
  dataResultado: string | null;
  laboratorio: string | null;
  resultados: string | null;
  fileUrl: string | null;
  appointmentId: string;
}

export interface UpdateExamRequest {
  tipoExame: ExamType;
  nome: string;
  dataRealizacao: string;
  dataResultado: string | null;
  laboratorio: string | null;
  resultados: string | null;
  fileUrl: string | null;
}

export const examTypes = [
  { value: 'Blood' as ExamType, label: 'Sangue' },
  { value: 'Image' as ExamType, label: 'Imagem' },
  { value: 'Functional' as ExamType, label: 'Funcional' },
  { value: 'Laboratory' as ExamType, label: 'Laboratorial' },
  { value: 'Other' as ExamType, label: 'Outro' },
] as const;

export const commonExamNames = [
  'Hemograma Completo',
  'Glicemia em Jejum',
  'Colesterol Total e Frações',
  'Triglicerídeos',
  'TSH',
  'T3 e T4 Livre',
  'Creatinina',
  'Ureia',
  'TGO e TGP',
  'Ácido Úrico',
  'Vitamina D',
  'Vitamina B12',
  'Ferritina',
  'Raio-X',
  'Ultrassonografia',
  'Tomografia',
  'Ressonância Magnética',
  'Eletrocardiograma',
  'Ecocardiograma',
  'Endoscopia',
  'Colonoscopia',
] as const;
