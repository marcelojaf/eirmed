'use client';

import { useState, useEffect } from 'react';
import { timelineApi } from '@/lib/api';
import { TimelineEvent, TimelineEventType, TimelineEstatisticas, TimelineFilters } from '@/types/timeline';
import Link from 'next/link';

export function MedicalTimeline() {
  const [events, setEvents] = useState<TimelineEvent[]>([]);
  const [estatisticas, setEstatisticas] = useState<TimelineEstatisticas | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<TimelineFilters>({});

  useEffect(() => {
    loadTimeline();
    loadEstatisticas();
  }, []);

  useEffect(() => {
    loadTimeline();
  }, [filters]);

  const loadTimeline = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await timelineApi.getTimeline(filters);
      setEvents(data);
    } catch (err) {
      setError('Erro ao carregar linha do tempo. Tente novamente.');
      console.error('Error loading timeline:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const loadEstatisticas = async () => {
    try {
      const data = await timelineApi.getEstatisticas();
      setEstatisticas(data);
    } catch (err) {
      console.error('Error loading statistics:', err);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getEventIcon = (tipo: TimelineEventType) => {
    switch (tipo) {
      case TimelineEventType.Consulta:
        return (
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        );
      case TimelineEventType.Exame:
        return (
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        );
      case TimelineEventType.Prescricao:
        return (
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
          </svg>
        );
    }
  };

  const getEventColor = (tipo: TimelineEventType) => {
    switch (tipo) {
      case TimelineEventType.Consulta:
        return {
          bg: 'bg-blue-100',
          text: 'text-blue-600',
          border: 'border-blue-300',
          light: 'bg-blue-50',
        };
      case TimelineEventType.Exame:
        return {
          bg: 'bg-purple-100',
          text: 'text-purple-600',
          border: 'border-purple-300',
          light: 'bg-purple-50',
        };
      case TimelineEventType.Prescricao:
        return {
          bg: 'bg-green-100',
          text: 'text-green-600',
          border: 'border-green-300',
          light: 'bg-green-50',
        };
    }
  };

  const getEventLink = (event: TimelineEvent) => {
    if (!event.referenciaId) return null;
    switch (event.tipo) {
      case TimelineEventType.Consulta:
        return `/appointments/${event.referenciaId}`;
      case TimelineEventType.Exame:
        return `/exams/${event.referenciaId}`;
      case TimelineEventType.Prescricao:
        return null; // Prescriptions don't have a dedicated page yet
    }
  };

  const handleFilterChange = (key: keyof TimelineFilters, value: string | undefined) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value || undefined,
    }));
  };

  const clearFilters = () => {
    setFilters({});
  };

  const groupEventsByMonth = (events: TimelineEvent[]) => {
    const grouped: { [key: string]: TimelineEvent[] } = {};
    events.forEach((event) => {
      const date = new Date(event.data);
      const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
      if (!grouped[key]) grouped[key] = [];
      grouped[key].push(event);
    });
    return grouped;
  };

  const formatMonthYear = (key: string) => {
    const [year, month] = key.split('-');
    const date = new Date(parseInt(year), parseInt(month) - 1);
    return date.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
  };

  if (isLoading && events.length === 0) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
        <span className="ml-2 text-gray-600">Carregando linha do tempo...</span>
      </div>
    );
  }

  const groupedEvents = groupEventsByMonth(events);
  const sortedMonths = Object.keys(groupedEvents).sort((a, b) => b.localeCompare(a));

  return (
    <div className="max-w-4xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Linha do Tempo Médica</h2>
        <p className="text-gray-600 mt-1">
          Visualização cronológica unificada do seu histórico médico
        </p>
      </div>

      {/* Statistics Cards */}
      {estatisticas && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
          <div className="bg-white rounded-lg shadow-md p-4 border-l-4 border-blue-500">
            <div className="text-2xl font-bold text-blue-600">{estatisticas.totalConsultas}</div>
            <div className="text-sm text-gray-600">Consultas</div>
          </div>
          <div className="bg-white rounded-lg shadow-md p-4 border-l-4 border-purple-500">
            <div className="text-2xl font-bold text-purple-600">{estatisticas.totalExames}</div>
            <div className="text-sm text-gray-600">Exames</div>
          </div>
          <div className="bg-white rounded-lg shadow-md p-4 border-l-4 border-green-500">
            <div className="text-2xl font-bold text-green-600">{estatisticas.totalPrescricoes}</div>
            <div className="text-sm text-gray-600">Prescricoes</div>
          </div>
          <div className="bg-white rounded-lg shadow-md p-4 border-l-4 border-indigo-500">
            <div className="text-2xl font-bold text-indigo-600">{estatisticas.totalEventos}</div>
            <div className="text-sm text-gray-600">Total</div>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="bg-white rounded-lg shadow-md p-4 mb-6">
        <div className="flex flex-wrap gap-4 items-end">
          <div className="flex-1 min-w-[200px]">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Início
            </label>
            <input
              type="date"
              value={filters.dataInicio || ''}
              onChange={(e) => handleFilterChange('dataInicio', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
          <div className="flex-1 min-w-[200px]">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Fim
            </label>
            <input
              type="date"
              value={filters.dataFim || ''}
              onChange={(e) => handleFilterChange('dataFim', e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
          <div className="flex-1 min-w-[200px]">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tipo de Evento
            </label>
            <select
              value={filters.tipo || ''}
              onChange={(e) => handleFilterChange('tipo', e.target.value as TimelineEventType || undefined)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            >
              <option value="">Todos</option>
              <option value={TimelineEventType.Consulta}>Consultas</option>
              <option value={TimelineEventType.Exame}>Exames</option>
              <option value={TimelineEventType.Prescricao}>Prescricoes</option>
            </select>
          </div>
          <button
            onClick={clearFilters}
            className="px-4 py-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors"
          >
            Limpar
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">
          {error}
          <button onClick={() => setError(null)} className="ml-2 text-red-500 hover:text-red-700">
            x
          </button>
        </div>
      )}

      {/* Timeline */}
      {events.length === 0 ? (
        <div className="text-center py-12 bg-white rounded-lg shadow-md">
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
              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <h3 className="mt-2 text-sm font-medium text-gray-900">
            Nenhum evento encontrado
          </h3>
          <p className="mt-1 text-sm text-gray-500">
            Registre consultas, exames e prescricoes para visualizar sua linha do tempo.
          </p>
        </div>
      ) : (
        <div className="space-y-8">
          {sortedMonths.map((monthKey) => (
            <div key={monthKey}>
              {/* Month Header */}
              <div className="sticky top-0 bg-gradient-to-br from-blue-50 to-indigo-100 py-2 z-10">
                <h3 className="text-lg font-semibold text-gray-700 capitalize">
                  {formatMonthYear(monthKey)}
                </h3>
              </div>

              {/* Events for this month */}
              <div className="relative">
                {/* Timeline line */}
                <div className="absolute left-5 top-0 bottom-0 w-0.5 bg-gray-200"></div>

                <div className="space-y-4 ml-2">
                  {groupedEvents[monthKey].map((event, index) => {
                    const colors = getEventColor(event.tipo);
                    const link = getEventLink(event);

                    return (
                      <div key={event.id} className="relative flex gap-4">
                        {/* Timeline dot */}
                        <div className={`flex-shrink-0 w-10 h-10 rounded-full ${colors.bg} ${colors.text} flex items-center justify-center z-10 ring-4 ring-white`}>
                          {getEventIcon(event.tipo)}
                        </div>

                        {/* Event card */}
                        <div className={`flex-1 bg-white rounded-lg shadow-md p-4 border-l-4 ${colors.border} hover:shadow-lg transition-shadow`}>
                          <div className="flex items-start justify-between">
                            <div className="flex-1">
                              <div className="flex items-center gap-2 mb-1">
                                <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${colors.bg} ${colors.text}`}>
                                  {event.tipo}
                                </span>
                                <span className="text-sm text-gray-500">
                                  {formatDateTime(event.data)}
                                </span>
                              </div>
                              <h4 className="text-lg font-medium text-gray-900">
                                {event.titulo}
                              </h4>
                              {event.subtitulo && (
                                <p className="text-sm text-gray-600 mt-0.5">
                                  {event.subtitulo}
                                </p>
                              )}
                              {event.descricao && (
                                <p className="text-sm text-gray-500 mt-2">
                                  {event.descricao}
                                </p>
                              )}

                              {/* Additional details based on type */}
                              {event.detalhes && (
                                <div className={`mt-3 p-2 rounded ${colors.light} text-sm`}>
                                  {event.tipo === TimelineEventType.Consulta && event.detalhes.diagnosticos && (
                                    <div>
                                      <span className="font-medium">Diagnóstico:</span>{' '}
                                      {event.detalhes.diagnosticos}
                                    </div>
                                  )}
                                  {event.tipo === TimelineEventType.Exame && (
                                    <>
                                      {event.detalhes.laboratorio && (
                                        <div>
                                          <span className="font-medium">Laboratório:</span>{' '}
                                          {event.detalhes.laboratorio}
                                        </div>
                                      )}
                                      {event.detalhes.resultados && (
                                        <div className="mt-1">
                                          <span className="font-medium">Resultados:</span>{' '}
                                          {event.detalhes.resultados.length > 100
                                            ? `${event.detalhes.resultados.substring(0, 100)}...`
                                            : event.detalhes.resultados}
                                        </div>
                                      )}
                                    </>
                                  )}
                                  {event.tipo === TimelineEventType.Prescricao && (
                                    <>
                                      {event.detalhes.dosagem && (
                                        <div>
                                          <span className="font-medium">Dosagem:</span>{' '}
                                          {event.detalhes.dosagem}
                                        </div>
                                      )}
                                      {event.detalhes.dataFim && (
                                        <div>
                                          <span className="font-medium">Uso até:</span>{' '}
                                          {formatDate(event.detalhes.dataFim)}
                                        </div>
                                      )}
                                    </>
                                  )}
                                </div>
                              )}
                            </div>

                            {/* View details button */}
                            {link && (
                              <Link
                                href={link}
                                className="flex-shrink-0 p-2 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                                title="Ver detalhes"
                              >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                </svg>
                              </Link>
                            )}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
