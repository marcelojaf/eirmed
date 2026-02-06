'use client';

import { useAuth } from "@/contexts/AuthContext";
import { UserProfile } from "@/components/UserProfile";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { appointmentsApi } from "@/lib/api";
import type { Appointment } from "@/types/appointment";

export default function AppointmentDetailsPage() {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();
  const params = useParams();
  const appointmentId = params.id as string;

  const [appointment, setAppointment] = useState<Appointment | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/');
    }
  }, [isAuthenticated, authLoading, router]);

  useEffect(() => {
    if (isAuthenticated && appointmentId) {
      loadAppointment();
    }
  }, [isAuthenticated, appointmentId]);

  const loadAppointment = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await appointmentsApi.getById(appointmentId);
      setAppointment(data);
    } catch (err) {
      setError('Erro ao carregar dados da consulta.');
      console.error('Error loading appointment:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      weekday: 'long',
      day: '2-digit',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (authLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100">
        <div className="animate-pulse text-xl text-gray-600">Carregando...</div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="flex min-h-screen flex-col bg-gradient-to-br from-blue-50 to-indigo-100">
      <header className="border-b bg-white/80 backdrop-blur-sm">
        <div className="container mx-auto flex items-center justify-between px-4 py-4">
          <Link href="/" className="text-2xl font-bold text-indigo-600 hover:text-indigo-700">
            EirMed
          </Link>
          <UserProfile />
        </div>
      </header>

      <main className="container mx-auto flex-1 px-4 py-8">
        <div className="mb-6">
          <Link
            href="/appointments"
            className="inline-flex items-center text-indigo-600 hover:text-indigo-700"
          >
            <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Voltar para Consultas
          </Link>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center p-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600">Carregando dados...</span>
          </div>
        ) : error ? (
          <div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
            <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">
              {error}
            </div>
          </div>
        ) : appointment ? (
          <div className="max-w-2xl mx-auto">
            <div className="bg-white rounded-lg shadow-md overflow-hidden">
              <div className="p-6 border-b border-gray-200">
                <div className="flex items-center justify-between">
                  <div>
                    <h2 className="text-2xl font-bold text-gray-900">Detalhes da Consulta</h2>
                    <p className="mt-1 text-sm text-gray-500">{formatDate(appointment.data)}</p>
                  </div>
                  <Link
                    href={`/appointments/${appointmentId}/edit`}
                    className="inline-flex items-center px-4 py-2 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors"
                  >
                    <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    Editar
                  </Link>
                </div>
              </div>

              <div className="p-6 space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <h3 className="text-sm font-medium text-gray-500">Profissional de Saude</h3>
                    <p className="mt-1 text-lg text-gray-900">{appointment.doctorNome}</p>
                  </div>
                  <div>
                    <h3 className="text-sm font-medium text-gray-500">Especialidade</h3>
                    <p className="mt-1 text-lg text-gray-900">{appointment.especialidade}</p>
                  </div>
                </div>

                {appointment.queixaPrincipal && (
                  <div>
                    <h3 className="text-sm font-medium text-gray-500 mb-2">Queixa Principal</h3>
                    <div className="p-4 bg-gray-50 rounded-lg">
                      <p className="text-gray-800 whitespace-pre-wrap">{appointment.queixaPrincipal}</p>
                    </div>
                  </div>
                )}

                {appointment.diagnosticos && (
                  <div>
                    <h3 className="text-sm font-medium text-gray-500 mb-2">Diagnosticos</h3>
                    <div className="p-4 bg-blue-50 rounded-lg border border-blue-100">
                      <p className="text-gray-800 whitespace-pre-wrap">{appointment.diagnosticos}</p>
                    </div>
                  </div>
                )}

                {appointment.observacoesGerais && (
                  <div>
                    <h3 className="text-sm font-medium text-gray-500 mb-2">Observacoes Gerais</h3>
                    <div className="p-4 bg-gray-50 rounded-lg">
                      <p className="text-gray-800 whitespace-pre-wrap">{appointment.observacoesGerais}</p>
                    </div>
                  </div>
                )}

                <div className="pt-4 border-t border-gray-200">
                  <div className="flex justify-between text-sm text-gray-500">
                    <span>Criado em: {new Date(appointment.createdAt).toLocaleDateString('pt-BR')}</span>
                    {appointment.updatedAt && (
                      <span>Atualizado em: {new Date(appointment.updatedAt).toLocaleDateString('pt-BR')}</span>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        ) : null}
      </main>

      <footer className="border-t bg-white/80 py-4 text-center text-sm text-gray-500">
        © {new Date().getFullYear()} EirMed - Todos os direitos reservados
      </footer>
    </div>
  );
}
