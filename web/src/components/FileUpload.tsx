'use client';

import { useState, useRef } from 'react';
import { filesApi } from '@/lib/api';
import type { Attachment } from '@/types/attachment';

interface FileUploadProps {
  appointmentId: string;
  existingAttachments?: Attachment[];
  onUploadSuccess?: (attachment: Attachment) => void;
  onDeleteSuccess?: (attachmentId: string) => void;
}

const ALLOWED_EXTENSIONS = ['.pdf', '.jpg', '.jpeg', '.png', '.gif', '.doc', '.docx', '.xls', '.xlsx'];
const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

export function FileUpload({
  appointmentId,
  existingAttachments = [],
  onUploadSuccess,
  onDeleteSuccess
}: FileUploadProps) {
  const [attachments, setAttachments] = useState<Attachment[]>(existingAttachments);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getFileIcon = (contentType: string | null, fileName: string): string => {
    const ext = fileName.toLowerCase().split('.').pop();

    if (contentType?.startsWith('image/') || ['jpg', 'jpeg', 'png', 'gif'].includes(ext || '')) {
      return '🖼️';
    }
    if (contentType === 'application/pdf' || ext === 'pdf') {
      return '📄';
    }
    if (contentType?.includes('word') || ['doc', 'docx'].includes(ext || '')) {
      return '📝';
    }
    if (contentType?.includes('excel') || contentType?.includes('spreadsheet') || ['xls', 'xlsx'].includes(ext || '')) {
      return '📊';
    }
    return '📎';
  };

  const validateFile = (file: File): string | null => {
    const extension = '.' + file.name.split('.').pop()?.toLowerCase();

    if (!ALLOWED_EXTENSIONS.includes(extension)) {
      return `Tipo de arquivo não permitido. Extensões permitidas: ${ALLOWED_EXTENSIONS.join(', ')}`;
    }

    if (file.size > MAX_FILE_SIZE) {
      return `O arquivo excede o tamanho máximo permitido de 10MB.`;
    }

    return null;
  };

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const validationError = validateFile(file);
    if (validationError) {
      setUploadError(validationError);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
      return;
    }

    try {
      setIsUploading(true);
      setUploadError(null);

      const response = await filesApi.uploadForAppointment(appointmentId, file);
      const newAttachment: Attachment = {
        id: response.id,
        fileName: response.fileName,
        fileUrl: response.fileUrl,
        contentType: response.contentType,
        fileSizeBytes: response.fileSizeBytes,
        createdAt: response.createdAt,
      };

      setAttachments((prev) => [...prev, newAttachment]);
      onUploadSuccess?.(newAttachment);
    } catch (err: unknown) {
      const error = err as { response?: { data?: { message?: string } } };
      if (error.response?.data?.message) {
        setUploadError(error.response.data.message);
      } else {
        setUploadError('Erro ao fazer upload do arquivo. Tente novamente.');
      }
      console.error('Error uploading file:', err);
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const handleDelete = async (attachmentId: string) => {
    if (!confirm('Tem certeza que deseja excluir este arquivo?')) {
      return;
    }

    try {
      setDeletingId(attachmentId);
      await filesApi.deleteAttachment(attachmentId);
      setAttachments((prev) => prev.filter((a) => a.id !== attachmentId));
      onDeleteSuccess?.(attachmentId);
    } catch (err) {
      console.error('Error deleting file:', err);
      setUploadError('Erro ao excluir o arquivo. Tente novamente.');
    } finally {
      setDeletingId(null);
    }
  };

  const handleDownload = async (attachment: Attachment) => {
    try {
      const blob = await filesApi.downloadAttachment(attachment.id);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = attachment.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Error downloading file:', err);
      setUploadError('Erro ao baixar o arquivo. Tente novamente.');
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <label className="block text-sm font-medium text-gray-700">
          Arquivos Anexados
        </label>
        <span className="text-xs text-gray-500">
          Max: 10MB | {ALLOWED_EXTENSIONS.join(', ')}
        </span>
      </div>

      {uploadError && (
        <div className="p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg flex items-center justify-between">
          <span>{uploadError}</span>
          <button
            onClick={() => setUploadError(null)}
            className="text-red-500 hover:text-red-700"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      )}

      {/* Upload area */}
      <div className="relative">
        <input
          ref={fileInputRef}
          type="file"
          onChange={handleFileSelect}
          accept={ALLOWED_EXTENSIONS.join(',')}
          disabled={isUploading}
          className="hidden"
          id="file-upload"
        />
        <label
          htmlFor="file-upload"
          className={`
            flex flex-col items-center justify-center w-full h-32
            border-2 border-dashed rounded-lg cursor-pointer
            transition-colors
            ${isUploading
              ? 'border-gray-300 bg-gray-50 cursor-not-allowed'
              : 'border-gray-300 hover:border-indigo-400 hover:bg-indigo-50'
            }
          `}
        >
          {isUploading ? (
            <div className="flex items-center">
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-indigo-600"></div>
              <span className="ml-2 text-gray-600">Enviando arquivo...</span>
            </div>
          ) : (
            <>
              <svg
                className="w-10 h-10 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
                />
              </svg>
              <p className="mt-2 text-sm text-gray-600">
                <span className="font-medium text-indigo-600">Clique para enviar</span>
              </p>
              <p className="text-xs text-gray-500">ou arraste e solte</p>
            </>
          )}
        </label>
      </div>

      {/* Attachments list */}
      {attachments.length > 0 && (
        <div className="space-y-2">
          <p className="text-sm font-medium text-gray-700">
            {attachments.length} arquivo{attachments.length > 1 ? 's' : ''} anexado{attachments.length > 1 ? 's' : ''}
          </p>
          <ul className="divide-y divide-gray-200 border border-gray-200 rounded-lg">
            {attachments.map((attachment) => (
              <li
                key={attachment.id}
                className="flex items-center justify-between p-3 hover:bg-gray-50"
              >
                <div className="flex items-center min-w-0 flex-1">
                  <span className="text-2xl mr-3">
                    {getFileIcon(attachment.contentType, attachment.fileName)}
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium text-gray-900 truncate">
                      {attachment.fileName}
                    </p>
                    <p className="text-xs text-gray-500">
                      {formatFileSize(attachment.fileSizeBytes)} • {new Date(attachment.createdAt).toLocaleDateString('pt-BR')}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2 ml-4">
                  <button
                    onClick={() => handleDownload(attachment)}
                    className="p-2 text-indigo-600 hover:text-indigo-800 hover:bg-indigo-50 rounded-lg transition-colors"
                    title="Baixar arquivo"
                  >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                    </svg>
                  </button>
                  <button
                    onClick={() => handleDelete(attachment.id)}
                    disabled={deletingId === attachment.id}
                    className="p-2 text-red-600 hover:text-red-800 hover:bg-red-50 rounded-lg transition-colors disabled:opacity-50"
                    title="Excluir arquivo"
                  >
                    {deletingId === attachment.id ? (
                      <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-red-600"></div>
                    ) : (
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    )}
                  </button>
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
