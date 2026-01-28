'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { appointmentsApi, doctorsApi } from '@/lib/api';
import type { CreateAppointmentRequest, UpdateAppointmentRequest } from '@/types/appointment';
import type { Doctor } from '@/types/doctor';
import type { Attachment } from '@/types/attachment';
import { especialidadesComuns } from '@/types/doctor';
import { FileUpload } from './FileUpload';

interface AppointmentFormProps {
  appointmentId?: string;
}

export function AppointmentForm({ appointmentId }: AppointmentFormProps) {
  const router = useRouter();
  const isEditing = !!appointmentId;

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showCustomEspecialidade, setShowCustomEspecialidade] = useState(false);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [attachments, setAttachments] = useState<Attachment[]>([]);

  const [formData, setFormData] = useState<CreateAppointmentRequest>({
    data: '',
    especialidade: '',
    queixaPrincipal: null,
    observacoesGerais: null,
    diagnosticos: null,
    doctorId: '',
  });

  useEffect(() => {
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Load doctors first
      const doctorsData = await doctorsApi.getAll();
      setDoctors(doctorsData);

      // If editing, load appointment data
      if (isEditing && appointmentId) {
        const appointmentData = await appointmentsApi.getById(appointmentId);
        const dateForInput = new Date(appointmentData.data).toISOString().slice(0, 16);
        setFormData({
          data: dateForInput,
          especialidade: appointmentData.especialidade,
          queixaPrincipal: appointmentData.queixaPrincipal,
          observacoesGerais: appointmentData.observacoesGerais,
          diagnosticos: appointmentData.diagnosticos,
          doctorId: appointmentData.doctorId,
        });
        // Load existing attachments
        if (appointmentData.attachments) {
          setAttachments(appointmentData.attachments);
        }
        // Check if especialidade is not in the common list
        if (!especialidadesComuns.includes(appointmentData.especialidade as typeof especialidadesComuns[number])) {
          setShowCustomEspecialidade(true);
        }
      }
    } catch (err) {
      setError('Erro ao carregar dados. Tente novamente.');
      console.error('Error loading data:', err);
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

      // Convert local datetime to ISO string
      const dataToSend = {
        ...formData,
        data: new Date(formData.data).toISOString(),
      };

      if (isEditing && appointmentId) {
        await appointmentsApi.update(appointmentId, dataToSend as UpdateAppointmentRequest);
        setSuccess('Consulta atualizada com sucesso!');
      } else {
        await appointmentsApi.create(dataToSend);
        setSuccess('Consulta cadastrada com sucesso!');
      }

      setTimeout(() => {
        router.push('/appointments');
      }, 1500);
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      if (error.response?.data?.message) {
        setError(error.response.data.message);
      } else {
        setError('Erro ao salvar consulta. Verifique os dados e tente novamente.');
      }
      console.error('Error saving appointment:', err);
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

  const handleEspecialidadeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    if (value === '__custom__') {
      setShowCustomEspecialidade(true);
      setFormData((prev) => ({ ...prev, especialidade: '' }));
    } else {
      setShowCustomEspecialidade(false);
      setFormData((prev) => ({ ...prev, especialidade: value }));
    }
  };

  const handleDoctorChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const doctorId = e.target.value;
    const selectedDoctor = doctors.find(d => d.id === doctorId);

    setFormData((prev) => ({
      ...prev,
      doctorId,
      // Auto-fill especialidade from doctor if not already set
      especialidade: prev.especialidade || (selectedDoctor?.especialidade || ''),
    }));

    // Check if we need to show custom especialidade field
    if (selectedDoctor && !especialidadesComuns.includes(selectedDoctor.especialidade as typeof especialidadesComuns[number])) {
      setShowCustomEspecialidade(true);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2 text-gray-600">Carregando dados...</span>
      </div>
    );
  }

  if (doctors.length === 0) {
    return (
      <div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
        <div className="text-center py-8">
          <svg
            className="mx-auto h-12 w-12 text-gray-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
            />
          </svg>
          <h3 className="mt-2 text-lg font-medium text-gray-900">Nenhum profissional cadastrado</h3>
          <p className="mt-1 text-sm text-gray-500">
            Para registrar uma consulta, primeiro cadastre ao menos um profissional de saude.
          </p>
          <div className="mt-6">
            <button
              onClick={() => router.push('/doctors/new')}
              className="inline-flex items-center px-4 py-2 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors"
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Cadastrar Profissional
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEditing ? 'Editar Consulta' : 'Nova Consulta'}
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
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label htmlFor="data" className="block text-sm font-medium text-gray-700 mb-1">
              Data e Hora *
            </label>
            <input
              type="datetime-local"
              id="data"
              name="data"
              value={formData.data}
              onChange={handleChange}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>

          <div>
            <label htmlFor="doctorId" className="block text-sm font-medium text-gray-700 mb-1">
              Profissional de Saude *
            </label>
            <select
              id="doctorId"
              name="doctorId"
              value={formData.doctorId}
              onChange={handleDoctorChange}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            >
              <option value="">Selecione um profissional...</option>
              {doctors.map((doctor) => (
                <option key={doctor.id} value={doctor.id}>
                  {doctor.nome} - {doctor.especialidade}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label htmlFor="especialidade" className="block text-sm font-medium text-gray-700 mb-1">
            Especialidade da Consulta *
          </label>
          {!showCustomEspecialidade ? (
            <select
              id="especialidade-select"
              value={formData.especialidade || ''}
              onChange={handleEspecialidadeChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            >
              <option value="">Selecione uma especialidade...</option>
              {especialidadesComuns.map((esp) => (
                <option key={esp} value={esp}>
                  {esp}
                </option>
              ))}
              <option value="__custom__">Outra especialidade...</option>
            </select>
          ) : (
            <div className="flex gap-2">
              <input
                type="text"
                id="especialidade"
                name="especialidade"
                value={formData.especialidade}
                onChange={handleChange}
                required
                minLength={2}
                maxLength={100}
                placeholder="Digite a especialidade"
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
              />
              <button
                type="button"
                onClick={() => {
                  setShowCustomEspecialidade(false);
                  setFormData((prev) => ({ ...prev, especialidade: '' }));
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
          <p className="mt-1 text-xs text-gray-500">
            A especialidade pode ser diferente da especialidade do profissional.
          </p>
        </div>

        <div>
          <label htmlFor="queixaPrincipal" className="block text-sm font-medium text-gray-700 mb-1">
            Queixa Principal
          </label>
          <textarea
            id="queixaPrincipal"
            name="queixaPrincipal"
            value={formData.queixaPrincipal || ''}
            onChange={handleChange}
            maxLength={500}
            rows={3}
            placeholder="Descreva o motivo principal da consulta..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 resize-none"
          />
          <p className="mt-1 text-xs text-gray-500">Maximo de 500 caracteres.</p>
        </div>

        <div>
          <label htmlFor="diagnosticos" className="block text-sm font-medium text-gray-700 mb-1">
            Diagnosticos
          </label>
          <textarea
            id="diagnosticos"
            name="diagnosticos"
            value={formData.diagnosticos || ''}
            onChange={handleChange}
            maxLength={2000}
            rows={4}
            placeholder="Diagnosticos informados pelo profissional..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 resize-none"
          />
          <p className="mt-1 text-xs text-gray-500">Maximo de 2000 caracteres.</p>
        </div>

        <div>
          <label htmlFor="observacoesGerais" className="block text-sm font-medium text-gray-700 mb-1">
            Observacoes Gerais
          </label>
          <textarea
            id="observacoesGerais"
            name="observacoesGerais"
            value={formData.observacoesGerais || ''}
            onChange={handleChange}
            maxLength={2000}
            rows={4}
            placeholder="Outras observacoes relevantes sobre a consulta..."
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 resize-none"
          />
          <p className="mt-1 text-xs text-gray-500">Maximo de 2000 caracteres.</p>
        </div>

        {/* File Upload Section - Only show when editing */}
        {isEditing && appointmentId && (
          <div className="border-t border-gray-200 pt-6">
            <FileUpload
              appointmentId={appointmentId}
              existingAttachments={attachments}
              onUploadSuccess={(newAttachment) => {
                setAttachments((prev) => [...prev, newAttachment]);
              }}
              onDeleteSuccess={(deletedId) => {
                setAttachments((prev) => prev.filter((a) => a.id !== deletedId));
              }}
            />
          </div>
        )}

        {/* Info for new appointments */}
        {!isEditing && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div className="flex items-start">
              <svg className="w-5 h-5 text-blue-600 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div>
                <p className="text-sm text-blue-800">
                  <strong>Arquivos:</strong> Voce podera anexar arquivos (receitas, exames, relatorios) depois de salvar a consulta.
                </p>
              </div>
            </div>
          </div>
        )}

        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => router.push('/appointments')}
            className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSaving || !formData.data || !formData.especialidade || !formData.doctorId}
            className="px-6 py-2 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSaving ? 'Salvando...' : isEditing ? 'Salvar Alteracoes' : 'Cadastrar'}
          </button>
        </div>
      </form>
    </div>
  );
}
