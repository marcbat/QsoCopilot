// Types pour l'API QSO Manager

export interface QsoAggregateDto {
  id: string;
  name: string;
  description?: string;
  moderatorId: string;
  frequency: number;
  startDateTime?: string;
  endDateTime?: string;
  createdDate?: string;
  mode?: string;
  location?: string;
  participants?: ParticipantDto[];
}

export interface ParticipantDto {
  callSign: string;
  name?: string;
  location?: string;
  signalReport?: string;
  notes?: string;
  qth?: string;
  rstSent?: string;
  rstReceived?: string;
  order: number;
}

export interface CreateQsoRequest {
  id?: string;
  name: string;
  description?: string;
  frequency: number;
  startDateTime?: string;
  mode?: string;
  location?: string;
}

export interface UpdateQsoRequest {
  name?: string;
  description?: string;
  frequency?: number;
  startDateTime?: string;
  endDateTime?: string;
  mode?: string;
  location?: string;
}

export interface CreateParticipantRequest {
  callSign: string;
  name?: string;
  location?: string;
  signalReport?: string;
  notes?: string;
}

export interface AddParticipantRequest {
  callSign: string;
  name?: string;
  qth?: string;
  rstSent?: string;
  rstReceived?: string;
}

// Types pour l'authentification
export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginByEmailRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  userName: string;
  password: string;
  email: string;
}

export interface TokenDto {
  token: string;
  expiration: string;
  userId: string;
  userName: string;
}

export interface User {
  id: string;
  userName: string;
  email?: string;
  callSign?: string;
}

// Types pour le contexte d'authentification
export interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (credentials: LoginRequest) => Promise<void>;
  loginByEmail: (credentials: LoginByEmailRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
}

// Types d'erreur API
export interface ApiError {
  message: string;
  errors?: string[];
}
