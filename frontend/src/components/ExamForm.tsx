'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { examsApi, appointmentsApi } from '@/lib/api';
import type { CreateExamRequest, UpdateExamRequest, ExamType } from '@/types/exam';
import type { Appointment } from '@/types/appointment';
import { examTypes, commonExamNames } from '@/types/exam';

interface ExamFormProps {
  examId?: string;
  appointmentId?: string;
}

export function ExamForm({ examId, appointmentId: initialAppointmentId }: ExamFormProps) {
  const router = useRouter();
  const isEditing = !!examId;

  const [isLoading, setIsLoading] = useState(isEditing);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showCustomNome, setShowCustomNome] = useState(false);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loadingAppointments, setLoadingAppointments] = useState(true);

  const [formData, setFormData] = useState<CreateExamRequest>({
    tipoExame: 'Blood',
    nome: '',
    dataRealizacao: new Date().toISOString().split('T')[0],
    dataResultado: null,
    laboratorio: null,
    resultados: null,
    fileUrl: null,
    appointmentId: initialAppointmentId || '',
  });

  useEffect(() => {
    loadAppointments();
  }, []);

  useEffect(() => {
    if (isEditing && examId) {
      loadExam(examId);
    }
  }, [isEditing, examId]);

  const loadAppointments = async () => {
    try {
      setLoadingAppointments(true);
      const data = await appointmentsApi.getAll();
      setAppointments(data);
    } catch (err) {
      console.error('Error loading appointments:', err);
    } finally {
      setLoadingAppointments(false);
    }
  };

  const loadExam = async (id: string) => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await examsApi.getById(id);
      setFormData({
        tipoExame: data.tipoExame,
        nome: data.nome,
        dataRealizacao: data.dataRealizacao.split('T')[0],
        dataResultado: data.dataResultado ? data.dataResultado.split('T')[0] : null,
        laboratorio: data.laboratorio,
        resultados: data.resultados,
        fileUrl: data.fileUrl,
        appointmentId: data.appointmentId,
      });
      // Check if nome is not in the common list
      if (!commonExamNames.includes(data.nome as typeof commonExamNames[number])) {
        setShowCustomNome(true);
      }
    } catch (err) {
      setError('Erro ao carregar dados do exame. Tente novamente.');
      console.error('Error loading exam:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSaving(true);
      setError(null);
      setSuccess(null);

      if (isEditing && examId) {
        const updateData: UpdateExamRequest = {
          tipoExame: formData.tipoExame,
          nome: formData.nome,
          dataRealizacao: formData.dataRealizacao,
          dataResultado: formData.dataResultado,
          laboratorio: formData.laboratorio,
          resultados: formData.resultados,
          fileUrl: formData.fileUrl,
        };
        await examsApi.update(examId, updateData);
        setSuccess('Exame atualizado com sucesso!');
      } else {
        await examsApi.create(formData);
        setSuccess('Exame cadastrado com sucesso!');
      }

      setTimeout(() => {
        if (initialAppointmentId) {
          router.push(`/appointments/${initialAppointmentId}`);
        } else {
          router.push('/exams');
        }
      }, 1500);
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      if (error.response?.data?.message) {
        setError(error.response.data.message);
      } else {
        setError('Erro ao salvar exame. Verifique os dados e tente novamente.');
      }
      console.error('Error saving exam:', err);
    } finally {
      setIsSaving(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value === '' ? null : value,
    }));
  };

  const handleNomeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    if (value === '__custom__') {
      setShowCustomNome(true);
      setFormData((prev) => ({ ...prev, nome: '' }));
    } else {
      setShowCustomNome(false);
      setFormData((prev) => ({ ...prev, nome: value }));
    }
  };

  const formatAppointmentOption = (appointment: Appointment) => {
    const date = new Date(appointment.data).toLocaleDateString('pt-BR');
    return `${date} - ${appointment.especialidade} (${appointment.doctorNome})`;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2 text-gray-600">Carregando dados...</span>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEditing ? 'Editar Exame' : 'Novo Exame'}
      </h2>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">
          {error}
        </div>
      )}

      {success && (
        <div className="mb-4 p-4 bg-green-50 border border-green-200 text-green-700 rounded-lg">
          {success}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        {!isEditing && (
          <div>
            <label htmlFor="appointmentId" className="block text-sm font-medium text-gray-700 mb-1">
              Consulta Relacionada *
            </label>
            {loadingAppointments ? (
              <div className="animate-pulse bg-gray-200 h-10 rounded-lg"></div>
            ) : (
              <select
                id="appointmentId"
                name="appointmentId"
                value={formData.appointmentId}
                onChange={handleChange}
                required
                disabled={!!initialAppointmentId}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 disabled:bg-gray-100"
              >
                <option value="">Selecione uma consulta...</option>
                {appointments.map((appointment) => (
                  <option key={appointment.id} value={appointment.id}>
                    {formatAppointmentOption(appointment)}
                  </option>
                ))}
              </select>
            )}
          </div>
        )}

        <div>
          <label htmlFor="tipoExame" className="block text-sm font-medium text-gray-700 mb-1">
            Tipo de Exame *
          </label>
          <select
            id="tipoExame"
            name="tipoExame"
            value={formData.tipoExame}
            onChange={handleChange}
            required
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          >
            {examTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="nome" className="block text-sm font-medium text-gray-700 mb-1">
            Nome do Exame *
          </label>
          {!showCustomNome ? (
            <select
              id="nome-select"
              value={formData.nome || ''}
              onChange={handleNomeChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            >
              <option value="">Selecione um exame...</option>
              {commonExamNames.map((nome) => (
                <option key={nome} value={nome}>
                  {nome}
                </option>
              ))}
              <option value="__custom__">Outro exame...</option>
            </select>
          ) : (
            <div className="flex gap-2">
              <input
                type="text"
                id="nome"
                name="nome"
                value={formData.nome}
                onChange={handleChange}
                required
                minLength={2}
                maxLength={200}
                placeholder="Digite o nome do exame"
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
              />
              <button
                type="button"
                onClick={() => {
                  setShowCustomNome(false);
                  setFormData((prev) => ({ ...prev, nome: '' }));
                }}
                className="px-3 py-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors"
                title="Voltar para lista"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h7" />
                </svg>
              </button>
            </div>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label htmlFor="dataRealizacao" className="block text-sm font-medium text-gray-700 mb-1">
              Data de Realização *
            </label>
            <input
              type="date"
              id="dataRealizacao"
              name="dataRealizacao"
              value={formData.dataRealizacao}
              onChange={handleChange}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>

          <div>
            <label htmlFor="dataResultado" className="block text-sm font-medium text-gray-700 mb-1">
              Data do Resultado
            </label>
            <input
              type="date"
              id="dataResultado"
              name="dataResultado"
              value={formData.dataResultado || ''}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
        </div>

        <div>
          <label htmlFor="laboratorio" className="block text-sm font-medium text-gray-700 mb-1">
            Laboratório
          </label>
          <input
            type="text"
            id="laboratorio"
            name="laboratorio"
            value={formData.laboratorio || ''}
            onChange={handleChange}
            maxLength={200}
            placeholder="Nome do laboratório ou clínica"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          />
        </div>

        <div>
          <label htmlFor="resultados" className="block text-sm font-medium text-gray-700 mb-1">
            Resultados
          </label>
          <textarea
            id="resultados"
            name="resultados"
            value={formData.resultados || ''}
            onChange={handleChange}
            maxLength={4000}
            rows={6}
            placeholder="Descreva os resultados do exame..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          />
          <p className="mt-1 text-xs text-gray-500">Máximo de 4000 caracteres.</p>
        </div>

        <div>
          <label htmlFor="fileUrl" className="block text-sm font-medium text-gray-700 mb-1">
            URL do Arquivo
          </label>
          <input
            type="url"
            id="fileUrl"
            name="fileUrl"
            value={formData.fileUrl || ''}
            onChange={handleChange}
            maxLength={500}
            placeholder="https://..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          />
          <p className="mt-1 text-xs text-gray-500">Link para o arquivo digitalizado do exame (PDF, imagem, etc.).</p>
        </div>

        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => {
              if (initialAppointmentId) {
                router.push(`/appointments/${initialAppointmentId}`);
              } else {
                router.push('/exams');
              }
            }}
            className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSaving || !formData.nome || !formData.appointmentId}
            className="px-6 py-2 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSaving ? 'Salvando...' : isEditing ? 'Salvar Alterações' : 'Cadastrar'}
          </button>
        </div>
      </form>
    </div>
  );
}
