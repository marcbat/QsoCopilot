// Utilitaires pour la gestion des autorisations

import { User, QsoAggregateDto } from '../types';

/**
 * Vérifie si l'utilisateur actuel est le modérateur du QSO
 */
export const isUserModerator = (user: User | null, qso: QsoAggregateDto | null): boolean => {
  if (!user || !qso) {
    return false;
  }
  
  return user.id === qso.moderatorId;
};

/**
 * Vérifie si l'utilisateur actuel peut modifier le QSO (actuellement, seul le modérateur peut)
 */
export const canUserModifyQso = (user: User | null, qso: QsoAggregateDto | null): boolean => {
  return isUserModerator(user, qso);
};

/**
 * Vérifie si l'utilisateur actuel peut réordonner les participants du QSO
 */
export const canUserReorderParticipants = (user: User | null, qso: QsoAggregateDto | null): boolean => {
  return isUserModerator(user, qso);
};

/**
 * Vérifie si l'utilisateur peut récupérer les informations QRZ
 * (utilisateur connecté avec des identifiants QRZ configurés)
 */
export const canUserFetchQrzInfo = (user: User | null): boolean => {
  if (!user) {
    return false;
  }
  
  // Vérifier si l'utilisateur a configuré ses identifiants QRZ
  return !!(user.qrzUsername);
};
