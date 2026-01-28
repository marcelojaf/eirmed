'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { doctorsApi } from '@/lib/api';
import type { CreateDoctorRequest, UpdateDoctorRequest } from '@/types/doctor';
import { especialidadesComuns } from '@/types/doctor';

interface DoctorFormProps {
  doctorId?: string;
}

export function DoctorForm({ doctorId }: DoctorFormProps) {
  const router = useRouter();
  const isEditing = !!doctorId;

  const [isLoading, setIsLoading] = useState(isEditing);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showCustomEspecialidade, setShowCustomEspecialidade] = useState(false);

  const [formData, setFormData] = useState<CreateDoctorRequest>({
    nome: '',
    especialidade: '',
    registroProfissional: null,
    clinicaHospital: null,
    contato: null,
  });

  useEffect(() => {
    if (isEditing && doctorId) {
      loadDoctor(doctorId);
    }
  }, [isEditing, doctorId]);

  const loadDoctor = async (id: string) => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await doctorsApi.getById(id);
      setFormData({
        nome: data.nome,
        especialidade: data.especialidade,
        registroProfissional: data.registroProfissional,
        clinicaHospital: data.clinicaHospital,
        contato: data.contato,
      });
      // Check if especialidade is not in the common list
      if (!especialidadesComuns.includes(data.especialidade as typeof especialidadesComuns[number])) {
        setShowCustomEspecialidade(true);
      }
    } catch (err) {
      setError('Erro ao carregar dados do profissional. Tente novamente.');
      console.error('Error loading doctor:', err);
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

      if (isEditing && doctorId) {
        await doctorsApi.update(doctorId, formData as UpdateDoctorRequest);
        setSuccess('Profissional atualizado com sucesso!');
      } else {
        await doctorsApi.create(formData);
        setSuccess('Profissional cadastrado com sucesso!');
      }

      setTimeout(() => {
        router.push('/doctors');
      }, 1500);
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      if (error.response?.data?.message) {
        setError(error.response.data.message);
      } else {
        setError('Erro ao salvar profissional. Verifique os dados e tente novamente.');
      }
      console.error('Error saving doctor:', err);
    } finally {
      setIsSaving(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
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
        {isEditing ? 'Editar Profissional de Saúde' : 'Novo Profissional de Saúde'}
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
        <div>
          <label htmlFor="nome" className="block text-sm font-medium text-gray-700 mb-1">
            Nome *
          </label>
          <input
            type="text"
            id="nome"
            name="nome"
            value={formData.nome}
            onChange={handleChange}
            required
            minLength={2}
            maxLength={200}
            placeholder="Nome completo do profissional"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          />
        </div>

        <div>
          <label htmlFor="especialidade" className="block text-sm font-medium text-gray-700 mb-1">
            Especialidade *
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
        </div>

        <div>
          <label htmlFor="registroProfissional" className="block text-sm font-medium text-gray-700 mb-1">
            Registro Profissional
          </label>
          <input
            type="text"
            id="registroProfissional"
            name="registroProfissional"
            value={formData.registroProfissional || ''}
            onChange={handleChange}
            maxLength={50}
            placeholder="Ex: CRM 12345/SP, CRN 12345, CREFITO 12345"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          />
          <p className="mt-1 text-xs text-gray-500">CRM, CRN, CREFITO, CRP ou outro registro profissional.</p>
        </div>

        <div>
          <label htmlFor="clinicaHospital" className="block text-sm font-medium text-gray-700 mb-1">
            Clínica / Hospital
          </label>
          <input
            type="text"
            id="clinicaHospital"
            name="clinicaHospital"
            value={formData.clinicaHospital || ''}
            onChange={handleChange}
            maxLength={200}
            placeholder="Nome da clínica ou hospital"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          />
        </div>

        <div>
          <label htmlFor="contato" className="block text-sm font-medium text-gray-700 mb-1">
            Contato
          </label>
          <input
            type="text"
            id="contato"
            name="contato"
            value={formData.contato || ''}
            onChange={handleChange}
            maxLength={200}
            placeholder="Telefone, e-mail ou outro contato"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
          />
        </div>

        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => router.push('/doctors')}
            className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSaving || !formData.nome || !formData.especialidade}
            className="px-6 py-2 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSaving ? 'Salvando...' : isEditing ? 'Salvar Alterações' : 'Cadastrar'}
          </button>
        </div>
      </form>
    </div>
  );
}
