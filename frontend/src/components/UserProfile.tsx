'use client';

import { useAuth } from '@/contexts/AuthContext';
import Image from 'next/image';

export function UserProfile() {
  const { user, logout, isLoading } = useAuth();

  if (!user) {
    return null;
  }

  return (
    <div className="flex items-center gap-4 rounded-lg bg-white p-4 shadow-md">
      {user.profilePictureUrl && (
        <Image
          src={user.profilePictureUrl}
          alt={user.nome}
          width={48}
          height={48}
          className="rounded-full"
        />
      )}
      <div className="flex flex-col">
        <span className="font-medium text-gray-900">{user.nome}</span>
        <span className="text-sm text-gray-500">{user.email}</span>
      </div>
      <button
        onClick={logout}
        disabled={isLoading}
        className="ml-auto rounded-lg bg-red-500 px-4 py-2 text-white transition-colors hover:bg-red-600 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {isLoading ? 'Saindo...' : 'Sair'}
      </button>
    </div>
  );
}
