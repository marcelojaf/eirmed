import axios from 'axios';
import type { AuthResponse } from '@/types/auth';
import type { UserProfile, UpdateUserProfileRequest } from '@/types/profile';
import type { Doctor, CreateDoctorRequest, UpdateDoctorRequest } from '@/types/doctor';
import type { Appointment, CreateAppointmentRequest, UpdateAppointmentRequest } from '@/types/appointment';
import type { Exam, CreateExamRequest, UpdateExamRequest, ExamType } from '@/types/exam';
import type { Medication, CreateMedicationRequest, UpdateMedicationRequest, UpdateStockRequest, MedicationUsageType } from '@/types/medication';
import type { TimelineEvent, TimelineEstatisticas, TimelineFilters, TimelineEventType } from '@/types/timeline';
import type { Attachment, UploadResponse } from '@/types/attachment';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
          const response = await axios.post<AuthResponse>(
            `${API_BASE_URL}/api/auth/refresh`,
            { refreshToken }
          );

          const { accessToken, refreshToken: newRefreshToken } = response.data;
          localStorage.setItem('accessToken', accessToken);
          localStorage.setItem('refreshToken', newRefreshToken);

          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export const authApi = {
  loginWithGoogle: async (idToken: string): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/api/auth/google', { idToken });
    return response.data;
  },

  refresh: async (refreshToken: string): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/api/auth/refresh', { refreshToken });
    return response.data;
  },

  logout: async (refreshToken: string): Promise<void> => {
    await api.post('/api/auth/logout', { refreshToken });
  },

  me: async () => {
    const response = await api.get('/api/auth/me');
    return response.data;
  },
};

export const profileApi = {
  getProfile: async (): Promise<UserProfile> => {
    const response = await api.get<UserProfile>('/api/userprofile');
    return response.data;
  },

  updateProfile: async (data: UpdateUserProfileRequest): Promise<UserProfile> => {
    const response = await api.put<UserProfile>('/api/userprofile', data);
    return response.data;
  },
};

export const doctorsApi = {
  getAll: async (): Promise<Doctor[]> => {
    const response = await api.get<Doctor[]>('/api/doctors');
    return response.data;
  },

  getById: async (id: string): Promise<Doctor> => {
    const response = await api.get<Doctor>(`/api/doctors/${id}`);
    return response.data;
  },

  create: async (data: CreateDoctorRequest): Promise<Doctor> => {
    const response = await api.post<Doctor>('/api/doctors', data);
    return response.data;
  },

  update: async (id: string, data: UpdateDoctorRequest): Promise<Doctor> => {
    const response = await api.put<Doctor>(`/api/doctors/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/doctors/${id}`);
  },
};

export const appointmentsApi = {
  getAll: async (): Promise<Appointment[]> => {
    const response = await api.get<Appointment[]>('/api/appointments');
    return response.data;
  },

  getById: async (id: string): Promise<Appointment> => {
    const response = await api.get<Appointment>(`/api/appointments/${id}`);
    return response.data;
  },

  getByDoctor: async (doctorId: string): Promise<Appointment[]> => {
    const response = await api.get<Appointment[]>(`/api/appointments/by-doctor/${doctorId}`);
    return response.data;
  },

  create: async (data: CreateAppointmentRequest): Promise<Appointment> => {
    const response = await api.post<Appointment>('/api/appointments', data);
    return response.data;
  },

  update: async (id: string, data: UpdateAppointmentRequest): Promise<Appointment> => {
    const response = await api.put<Appointment>(`/api/appointments/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/appointments/${id}`);
  },
};

export const examsApi = {
  getAll: async (): Promise<Exam[]> => {
    const response = await api.get<Exam[]>('/api/exams');
    return response.data;
  },

  getById: async (id: string): Promise<Exam> => {
    const response = await api.get<Exam>(`/api/exams/${id}`);
    return response.data;
  },

  getByAppointment: async (appointmentId: string): Promise<Exam[]> => {
    const response = await api.get<Exam[]>(`/api/exams/by-appointment/${appointmentId}`);
    return response.data;
  },

  getByType: async (examType: ExamType): Promise<Exam[]> => {
    const response = await api.get<Exam[]>(`/api/exams/by-type/${examType}`);
    return response.data;
  },

  create: async (data: CreateExamRequest): Promise<Exam> => {
    const response = await api.post<Exam>('/api/exams', data);
    return response.data;
  },

  update: async (id: string, data: UpdateExamRequest): Promise<Exam> => {
    const response = await api.put<Exam>(`/api/exams/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/exams/${id}`);
  },
};

export const medicationsApi = {
  getAll: async (): Promise<Medication[]> => {
    const response = await api.get<Medication[]>('/api/medications');
    return response.data;
  },

  getById: async (id: string): Promise<Medication> => {
    const response = await api.get<Medication>(`/api/medications/${id}`);
    return response.data;
  },

  getAlerts: async (): Promise<Medication[]> => {
    const response = await api.get<Medication[]>('/api/medications/alerts');
    return response.data;
  },

  getByUsageType: async (usageType: MedicationUsageType): Promise<Medication[]> => {
    const response = await api.get<Medication[]>(`/api/medications/by-usage/${usageType}`);
    return response.data;
  },

  create: async (data: CreateMedicationRequest): Promise<Medication> => {
    const response = await api.post<Medication>('/api/medications', data);
    return response.data;
  },

  update: async (id: string, data: UpdateMedicationRequest): Promise<Medication> => {
    const response = await api.put<Medication>(`/api/medications/${id}`, data);
    return response.data;
  },

  updateStock: async (id: string, data: UpdateStockRequest): Promise<Medication> => {
    const response = await api.patch<Medication>(`/api/medications/${id}/stock`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/medications/${id}`);
  },
};

export const timelineApi = {
  getTimeline: async (filters?: TimelineFilters): Promise<TimelineEvent[]> => {
    const params = new URLSearchParams();
    if (filters?.dataInicio) params.append('dataInicio', filters.dataInicio);
    if (filters?.dataFim) params.append('dataFim', filters.dataFim);
    if (filters?.tipo) params.append('tipo', filters.tipo);

    const queryString = params.toString();
    const url = queryString ? `/api/timeline?${queryString}` : '/api/timeline';
    const response = await api.get<TimelineEvent[]>(url);
    return response.data;
  },

  getEstatisticas: async (): Promise<TimelineEstatisticas> => {
    const response = await api.get<TimelineEstatisticas>('/api/timeline/estatisticas');
    return response.data;
  },
};

export const filesApi = {
  uploadForAppointment: async (appointmentId: string, file: File): Promise<UploadResponse> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<UploadResponse>(`/api/files/appointments/${appointmentId}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  getAttachmentsByAppointment: async (appointmentId: string): Promise<Attachment[]> => {
    const response = await api.get<Attachment[]>(`/api/files/appointments/${appointmentId}`);
    return response.data;
  },

  getAttachment: async (id: string): Promise<Attachment> => {
    const response = await api.get<Attachment>(`/api/files/${id}`);
    return response.data;
  },

  downloadAttachment: async (id: string): Promise<Blob> => {
    const response = await api.get(`/api/files/${id}/download`, {
      responseType: 'blob',
    });
    return response.data;
  },

  deleteAttachment: async (id: string): Promise<void> => {
    await api.delete(`/api/files/${id}`);
  },
};

// Re-export types for convenience
export type { TimelineEvent, TimelineEstatisticas, TimelineFilters, TimelineEventType };
export type { Attachment, UploadResponse };

export default api;
