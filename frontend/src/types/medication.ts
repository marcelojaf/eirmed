export type PharmaceuticalForm = 'Tablet' | 'Capsule' | 'Liquid' | 'Injectable' | 'Topical' | 'Inhaler' | 'Drops' | 'Suppository' | 'Patch' | 'Other';

export type MedicationUsageType = 'Continuous' | 'Occasional';

export interface Medication {
  id: string;
  nome: string;
  principioAtivo: string | null;
  dosagem: string;
  formaFarmaceutica: PharmaceuticalForm;
  dataValidade: string | null;
  quantidadeAtual: number;
  quantidadeMinima: number;
  tipoUso: MedicationUsageType;
  estoqueBaixo: boolean;
  vencido: boolean;
  proximoVencimento: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateMedicationRequest {
  nome: string;
  principioAtivo: string | null;
  dosagem: string;
  formaFarmaceutica: PharmaceuticalForm;
  dataValidade: string | null;
  quantidadeAtual: number;
  quantidadeMinima: number;
  tipoUso: MedicationUsageType;
}

export interface UpdateMedicationRequest {
  nome: string;
  principioAtivo: string | null;
  dosagem: string;
  formaFarmaceutica: PharmaceuticalForm;
  dataValidade: string | null;
  quantidadeAtual: number;
  quantidadeMinima: number;
  tipoUso: MedicationUsageType;
}

export interface UpdateStockRequest {
  quantidadeAtual: number;
}

export const pharmaceuticalForms = [
  { value: 'Tablet' as PharmaceuticalForm, label: 'Comprimido' },
  { value: 'Capsule' as PharmaceuticalForm, label: 'Cápsula' },
  { value: 'Liquid' as PharmaceuticalForm, label: 'Líquido' },
  { value: 'Injectable' as PharmaceuticalForm, label: 'Injetável' },
  { value: 'Topical' as PharmaceuticalForm, label: 'Tópico/Pomada' },
  { value: 'Inhaler' as PharmaceuticalForm, label: 'Inalador' },
  { value: 'Drops' as PharmaceuticalForm, label: 'Gotas' },
  { value: 'Suppository' as PharmaceuticalForm, label: 'Supositório' },
  { value: 'Patch' as PharmaceuticalForm, label: 'Adesivo' },
  { value: 'Other' as PharmaceuticalForm, label: 'Outro' },
] as const;

export const usageTypes = [
  { value: 'Continuous' as MedicationUsageType, label: 'Uso Contínuo' },
  { value: 'Occasional' as MedicationUsageType, label: 'Uso Eventual' },
] as const;

export const commonMedications = [
  'Paracetamol',
  'Dipirona',
  'Ibuprofeno',
  'Amoxicilina',
  'Azitromicina',
  'Omeprazol',
  'Losartana',
  'Metformina',
  'Atenolol',
  'Sinvastatina',
  'Levotiroxina',
  'Clonazepam',
  'Fluoxetina',
  'Sertralina',
  'Diazepam',
  'Rivotril',
  'Aspirina',
  'Enalapril',
  'Hidroclorotiazida',
  'Loratadina',
] as const;
