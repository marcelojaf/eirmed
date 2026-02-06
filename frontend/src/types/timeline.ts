export enum TimelineEventType {
  Consulta = 'Consulta',
  Exame = 'Exame',
  Prescricao = 'Prescricao',
}

export interface TimelineEventDetails {
  // Para Consultas
  nomeMedico: string | null;
  especialidade: string | null;
  diagnosticos: string | null;

  // Para Exames
  tipoExame: string | null;
  laboratorio: string | null;
  resultados: string | null;

  // Para Prescrições
  nomeMedicamento: string | null;
  dosagem: string | null;
  dataFim: string | null;
}

export interface TimelineEvent {
  id: string;
  data: string;
  tipo: TimelineEventType;
  titulo: string;
  subtitulo: string | null;
  descricao: string | null;
  referenciaId: string | null;
  detalhes: TimelineEventDetails | null;
}

export interface TimelineEstatisticas {
  totalConsultas: number;
  totalExames: number;
  totalPrescricoes: number;
  totalEventos: number;
  ultimaConsulta: string | null;
  ultimoExame: string | null;
}

export interface TimelineFilters {
  dataInicio?: string;
  dataFim?: string;
  tipo?: TimelineEventType;
}
