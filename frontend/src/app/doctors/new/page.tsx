'use client';

import { useAuth } from "@/contexts/AuthContext";
import { UserProfile } from "@/components/UserProfile";
import { DoctorForm } from "@/components/DoctorForm";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function NewDoctorPage() {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/');
    }
  }, [isAuthenticated, isLoading, router]);

  if (isLoading) {
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
            href="/doctors"
            className="inline-flex items-center text-indigo-600 hover:text-indigo-700"
          >
            <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Voltar para Profissionais
          </Link>
        </div>
        <DoctorForm />
      </main>

      <footer className="border-t bg-white/80 py-4 text-center text-sm text-gray-500">
        © {new Date().getFullYear()} EirMed - Todos os direitos reservados
      </footer>
    </div>
  );
}
