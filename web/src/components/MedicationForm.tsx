'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { medicationsApi } from '@/lib/api';
import type { CreateMedicationRequest, UpdateMedicationRequest, PharmaceuticalForm, MedicationUsageType } from '@/types/medication';
import { pharmaceuticalForms, usageTypes, commonMedications } from '@/types/medication';

interface MedicationFormProps {
  medicationId?: string;
}

export function MedicationForm({ medicationId }: MedicationFormProps) {
  const router = useRouter();
  const isEditing = !!medicationId;

  const [isLoading, setIsLoading] = useState(isEditing);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showCustomNome, setShowCustomNome] = useState(false);

  const [formData, setFormData] = useState<CreateMedicationRequest>({
    nome: '',
    principioAtivo: null,
    dosagem: '',
    formaFarmaceutica: 'Tablet',
    dataValidade: null,
    quantidadeAtual: 0,
    quantidadeMinima: 0,
    tipoUso: 'Occasional',
  });

  useEffect(() => {
    if (isEditing && medicationId) {
      loadMedication(medicationId);
    }
  }, [isEditing, medicationId]);

  const loadMedication = async (id: string) => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await medicationsApi.getById(id);
      setFormData({
        nome: data.nome,
        principioAtivo: data.principioAtivo,
        dosagem: data.dosagem,
        formaFarmaceutica: data.formaFarmaceutica,
        dataValidade: data.dataValidade,
        quantidadeAtual: data.quantidadeAtual,
        quantidadeMinima: data.quantidadeMinima,
        tipoUso: data.tipoUso,
      });
      // Check if nome is not in the common list
      if (!commonMedications.includes(data.nome as typeof commonMedications[number])) {
        setShowCustomNome(true);
      }
    } catch (err) {
      setError('Erro ao carregar dados do medicamento. Tente novamente.');
      console.error('Error loading medication:', err);
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

      if (isEditing && medicationId) {
        await medicationsApi.update(medicationId, formData as UpdateMedicationRequest);
        setSuccess('Medicamento atualizado com sucesso!');
      } else {
        await medicationsApi.create(formData);
        setSuccess('Medicamento cadastrado com sucesso!');
      }

      setTimeout(() => {
        router.push('/medications');
      }, 1500);
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      if (error.response?.data?.message) {
        setError(error.response.data.message);
      } else {
        setError('Erro ao salvar medicamento. Verifique os dados e tente novamente.');
      }
      console.error('Error saving medication:', err);
    } finally {
      setIsSaving(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;

    if (type === 'number') {
      setFormData((prev) => ({
        ...prev,
        [name]: value === '' ? 0 : parseInt(value, 10),
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        [name]: value === '' ? null : value,
      }));
    }
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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
        <span className="ml-2 text-gray-600">Carregando dados...</span>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEditing ? 'Editar Medicamento' : 'Novo Medicamento'}
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
            Nome do Medicamento *
          </label>
          {!showCustomNome ? (
            <select
              id="nome-select"
              value={formData.nome || ''}
              onChange={handleNomeChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
            >
              <option value="">Selecione um medicamento...</option>
              {commonMedications.map((med) => (
                <option key={med} value={med}>
                  {med}
                </option>
              ))}
              <option value="__custom__">Outro medicamento...</option>
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
                placeholder="Digite o nome do medicamento"
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
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

        <div>
          <label htmlFor="principioAtivo" className="block text-sm font-medium text-gray-700 mb-1">
            Princípio Ativo
          </label>
          <input
            type="text"
            id="principioAtivo"
            name="principioAtivo"
            value={formData.principioAtivo || ''}
            onChange={handleChange}
            maxLength={200}
            placeholder="Ex: Paracetamol, Dipirona Sódica"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
          />
          <p className="mt-1 text-xs text-gray-500">Substância ativa do medicamento (opcional).</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label htmlFor="dosagem" className="block text-sm font-medium text-gray-700 mb-1">
              Dosagem *
            </label>
            <input
              type="text"
              id="dosagem"
              name="dosagem"
              value={formData.dosagem}
              onChange={handleChange}
              required
              minLength={1}
              maxLength={100}
              placeholder="Ex: 500mg, 10ml, 20mg/ml"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
            />
          </div>

          <div>
            <label htmlFor="formaFarmaceutica" className="block text-sm font-medium text-gray-700 mb-1">
              Forma Farmacêutica *
            </label>
            <select
              id="formaFarmaceutica"
              name="formaFarmaceutica"
              value={formData.formaFarmaceutica}
              onChange={(e) => setFormData((prev) => ({ ...prev, formaFarmaceutica: e.target.value as PharmaceuticalForm }))}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
            >
              {pharmaceuticalForms.map((form) => (
                <option key={form.value} value={form.value}>
                  {form.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label htmlFor="tipoUso" className="block text-sm font-medium text-gray-700 mb-1">
            Tipo de Uso *
          </label>
          <div className="flex gap-4">
            {usageTypes.map((type) => (
              <label key={type.value} className="flex items-center">
                <input
                  type="radio"
                  name="tipoUso"
                  value={type.value}
                  checked={formData.tipoUso === type.value}
                  onChange={(e) => setFormData((prev) => ({ ...prev, tipoUso: e.target.value as MedicationUsageType }))}
                  className="w-4 h-4 text-green-600 border-gray-300 focus:ring-green-500"
                />
                <span className="ml-2 text-sm text-gray-700">{type.label}</span>
              </label>
            ))}
          </div>
          <p className="mt-1 text-xs text-gray-500">
            Uso contínuo: medicamentos tomados regularmente. Uso eventual: medicamentos usados quando necessário.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label htmlFor="quantidadeAtual" className="block text-sm font-medium text-gray-700 mb-1">
              Quantidade Atual *
            </label>
            <input
              type="number"
              id="quantidadeAtual"
              name="quantidadeAtual"
              value={formData.quantidadeAtual}
              onChange={handleChange}
              required
              min={0}
              placeholder="0"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
            />
            <p className="mt-1 text-xs text-gray-500">Quantidade de unidades em estoque.</p>
          </div>

          <div>
            <label htmlFor="quantidadeMinima" className="block text-sm font-medium text-gray-700 mb-1">
              Quantidade Mínima *
            </label>
            <input
              type="number"
              id="quantidadeMinima"
              name="quantidadeMinima"
              value={formData.quantidadeMinima}
              onChange={handleChange}
              required
              min={0}
              placeholder="0"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
            />
            <p className="mt-1 text-xs text-gray-500">Alerta quando estoque ficar abaixo deste valor.</p>
          </div>
        </div>

        <div>
          <label htmlFor="dataValidade" className="block text-sm font-medium text-gray-700 mb-1">
            Data de Validade
          </label>
          <input
            type="date"
            id="dataValidade"
            name="dataValidade"
            value={formData.dataValidade || ''}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500"
          />
          <p className="mt-1 text-xs text-gray-500">Você receberá alertas 30 dias antes do vencimento.</p>
        </div>

        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => router.push('/medications')}
            className="px-6 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSaving || !formData.nome || !formData.dosagem}
            className="px-6 py-2 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {isSaving ? 'Salvando...' : isEditing ? 'Salvar Alterações' : 'Cadastrar'}
          </button>
        </div>
      </form>
    </div>
  );
}
