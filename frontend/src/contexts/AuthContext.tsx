import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { AuthContextType, User, LoginRequest, LoginByEmailRequest, RegisterRequest, UpdateProfileRequest } from '../types';
import { authApiService } from '../api';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Charger l'utilisateur depuis le localStorage au démarrage
  useEffect(() => {
    const savedToken = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');

    if (savedToken && savedUser) {
      try {
        setToken(savedToken);
        setUser(JSON.parse(savedUser));
      } catch (error) {
        console.error('Erreur lors du chargement des données utilisateur:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    setIsLoading(false);
  }, []);

  const login = async (credentials: LoginRequest): Promise<void> => {
    try {
      setIsLoading(true);
      const response = await authApiService.login(credentials);
      
      const userData: User = {
        id: response.userId,
        userName: response.userName,
        callSign: credentials.userName, // Supposer que userName = callSign
      };

      setToken(response.token);
      setUser(userData);
      
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(userData));
    } catch (error) {
      console.error('Erreur de connexion:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const loginByEmail = async (credentials: LoginByEmailRequest): Promise<void> => {
    try {
      setIsLoading(true);
      const response = await authApiService.loginByEmail(credentials);
      
      const userData: User = {
        id: response.userId,
        userName: response.userName,
        email: credentials.email,
      };

      setToken(response.token);
      setUser(userData);
      
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(userData));
    } catch (error) {
      console.error('Erreur de connexion par email:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (data: RegisterRequest): Promise<void> => {
    try {
      setIsLoading(true);
      await authApiService.register(data);
      
      // Après inscription, connecter automatiquement l'utilisateur
      await login({ userName: data.userName, password: data.password });
    } catch (error) {
      console.error('Erreur d\'inscription:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };  const updateProfile = async (data: UpdateProfileRequest): Promise<string> => {
    try {
      setIsLoading(true);
      const response = await authApiService.updateProfile(data);
      
      // Mettre à jour les données utilisateur localement
      if (user) {
        const updatedUser: User = {
          ...user,
          email: data.email || user.email,
          qrzUsername: data.qrzUsername || user.qrzUsername,
        };
        setUser(updatedUser);
        localStorage.setItem('user', JSON.stringify(updatedUser));
      }
      
      // Retourner le message de confirmation
      return response.message;
    } catch (error) {
      console.error('Erreur de mise à jour du profil:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = (): void => {
    setUser(null);
    setToken(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  const isAuthenticated = !!user && !!token;
  const value: AuthContextType = {
    user,
    token,
    login,
    loginByEmail,
    register,
    updateProfile,
    logout,
    isAuthenticated,
    isLoading
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
