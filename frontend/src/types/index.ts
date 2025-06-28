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
  participants?: ParticipantDto[];
  history?: { [key: string]: string };
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
}

export interface UpdateQsoRequest {
  name?: string;
  description?: string;
  frequency?: number;
  startDateTime?: string;
  endDateTime?: string;
  mode?: string;
}

export interface CreateParticipantRequest {
  callSign: string;
  name?: string;
  location?: string;
  signalReport?: string;
  notes?: string;
  qth?: string;
  rstSent?: string;
  rstReceived?: string;
}

export interface AddParticipantRequest {
  callSign: string;
  name?: string;
  qth?: string;
  rstSent?: string;
  rstReceived?: string;
}

export interface ReorderParticipantsRequest {
  newOrders: { [callSign: string]: number };
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
  email?: string;
  qrzUsername?: string;
}

export interface User {
  id: string;
  userName: string;
  email?: string;
  callSign?: string;
  qrzUsername?: string;
}

// Types pour le contexte d'authentification
export interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (credentials: LoginRequest) => Promise<void>;
  loginByEmail: (credentials: LoginByEmailRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  updateProfile: (data: UpdateProfileRequest) => Promise<string>;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
}

export interface UpdateProfileRequest {
  email?: string;
  qrzUsername?: string;
  qrzPassword?: string;
}

export interface ModeratorDto {
  id: string;
  callSign: string;
  email?: string;
}

// Types d'erreur API
export interface ApiError {
  message: string;
  errors?: string[];
}

// Types pour les informations QRZ
export interface QrzCallsignInfo {
  callSign: string;
  fName?: string;
  name?: string;
  nickname?: string;
  nameFmt?: string;
  addr1?: string;
  addr2?: string;
  state?: string;
  zip?: string;
  country?: string;
  land?: string;
  lat?: number;
  lon?: number;
  grid?: string;
  county?: string;
  class?: string;
  email?: string;
  url?: string;
  qslManager?: string;
  timeZone?: string;
  geoLoc?: string;
  cqZone?: number;
  ituZone?: number;
  dxcc?: number;
  image?: string;
  bio?: string;
  eqsl?: string;
  mqsl?: string;
  lotw?: string;
  iota?: string;
  fetchedAt?: string;
}

export interface QrzDxccInfo {
  country?: string;
  code?: string;
  continent?: string;
  prefix?: string;
}

export interface ParticipantQrzInfoDto {
  callSign: string;
  qrzCallsignInfo?: QrzCallsignInfo;
  qrzDxccInfo?: QrzDxccInfo;
}

// Types pour la pagination
export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationParameters {
  pageNumber: number;
  pageSize: number;
}
