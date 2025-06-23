import axios, { AxiosResponse } from 'axios';
import { 
  QsoAggregateDto, 
  CreateQsoRequest,
  UpdateQsoRequest,
  CreateParticipantRequest,
  LoginRequest,
  LoginByEmailRequest,
  RegisterRequest,
  TokenDto,
  UpdateProfileRequest,
  ModeratorDto
} from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5041/api';

// Instance axios avec configuration de base
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Intercepteur pour ajouter le token JWT automatiquement
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Intercepteur pour gérer les erreurs de réponse
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expiré ou invalide, rediriger vers login
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Services API

export const qsoApiService = {
  // QSO Aggregates
  async getAllQsos(): Promise<QsoAggregateDto[]> {
    const response: AxiosResponse<QsoAggregateDto[]> = await apiClient.get('/QsoAggregate');
    return response.data;
  },

  async getQso(id: string): Promise<QsoAggregateDto> {
    const response: AxiosResponse<QsoAggregateDto> = await apiClient.get(`/QsoAggregate/${id}`);
    return response.data;
  },

  async createQso(qso: CreateQsoRequest): Promise<QsoAggregateDto> {
    const response: AxiosResponse<QsoAggregateDto> = await apiClient.post('/QsoAggregate', qso);
    return response.data;
  },

  async updateQso(id: string, qso: UpdateQsoRequest): Promise<QsoAggregateDto> {
    const response: AxiosResponse<QsoAggregateDto> = await apiClient.put(`/QsoAggregate/${id}`, qso);
    return response.data;
  },

  async addParticipant(qsoId: string, participant: CreateParticipantRequest): Promise<QsoAggregateDto> {
    const response: AxiosResponse<QsoAggregateDto> = await apiClient.post(
      `/QsoAggregate/${qsoId}/participants`, 
      participant
    );
    return response.data;
  },

  async removeParticipant(qsoId: string, callSign: string): Promise<void> {
    await apiClient.delete(`/QsoAggregate/${qsoId}/participants/${callSign}`);
  },
  async searchQsoByName(name: string): Promise<QsoAggregateDto[]> {
    const response: AxiosResponse<QsoAggregateDto[]> = await apiClient.get(`/QsoAggregate/search?name=${encodeURIComponent(name)}`);
    return response.data;
  },

  async getMyModeratedQsos(): Promise<QsoAggregateDto[]> {
    const response: AxiosResponse<QsoAggregateDto[]> = await apiClient.get('/QsoAggregate/my-moderated');
    return response.data;
  },

  // Aliases pour maintenir la compatibilité
  async getAllQsoAggregates(): Promise<QsoAggregateDto[]> {
    return this.getAllQsos();
  },

  async getQsoAggregateById(id: string): Promise<QsoAggregateDto> {
    return this.getQso(id);
  },

  async createQsoAggregate(qso: CreateQsoRequest): Promise<QsoAggregateDto> {
    return this.createQso(qso);
  }
};

export const authApiService = {
  async login(credentials: LoginRequest): Promise<TokenDto> {
    const response: AxiosResponse<TokenDto> = await apiClient.post('/Auth/login', credentials);
    return response.data;
  },

  async loginByEmail(credentials: LoginByEmailRequest): Promise<TokenDto> {
    const response: AxiosResponse<TokenDto> = await apiClient.post('/Auth/login-email', credentials);
    return response.data;
  },

  async register(data: RegisterRequest): Promise<{ userId: string; message: string }> {
    const response = await apiClient.post('/Auth/register', data);
    return response.data;
  },
  async updateProfile(data: UpdateProfileRequest): Promise<{ profile: ModeratorDto; message: string }> {
    const response: AxiosResponse<{ profile: ModeratorDto; message: string }> = await apiClient.put('/Auth/profile', data);
    return response.data;
  }
};

export const healthApiService = {
  async checkHealth(): Promise<{ status: string; timestamp: string; service: string }> {
    const response = await axios.get(`${API_BASE_URL.replace('/api', '')}/Health`);
    return response.data;
  }
};

export default apiClient;
