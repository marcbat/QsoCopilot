import React, { useState } from 'react';
import { UpdateProfileRequest } from '../types';
import { authApiService } from '../api';

const ProfilePageDirect: React.FC = () => {
  // Récupérer les données utilisateur depuis localStorage directement
  const userString = localStorage.getItem('user');
  const user = userString ? JSON.parse(userString) : null;
  
  // États du formulaire
  const [email, setEmail] = useState('');
  const [qrzUsername, setQrzUsername] = useState('');
  const [qrzPassword, setQrzPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
    // États pour les messages
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Initialiser les champs une seule fois
  const initializeFields = () => {
    if (user && !email && !qrzUsername) {
      setEmail(user.email || '');
      setQrzUsername(user.qrzUsername || '');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);
    setIsLoading(true);

    try {
      // Construire les données à mettre à jour
      const updateData: UpdateProfileRequest = {};
      if (email && email !== user?.email) {
        updateData.email = email;
      }
      if (qrzUsername && qrzUsername !== user?.qrzUsername) {
        updateData.qrzUsername = qrzUsername;
      }
      if (qrzPassword) {
        updateData.qrzPassword = qrzPassword;
      }

      // Vérifier qu'il y a au moins un champ à mettre à jour
      if (Object.keys(updateData).length === 0) {
        setErrorMessage('Aucune modification détectée');
        return;
      }      // Appel API direct (sans passer par AuthContext)
      const response = await authApiService.updateProfile(updateData);
      
      const confirmationMessage = response.message || 'Profil mis à jour avec succès';
      setSuccessMessage(confirmationMessage);
      
      // Mettre à jour le localStorage directement
      if (user) {
        const updatedUser = {
          ...user,
          email: updateData.email || user.email,
          qrzUsername: updateData.qrzUsername || user.qrzUsername,
        };
        localStorage.setItem('user', JSON.stringify(updatedUser));
      }
      
      // Réinitialiser le mot de passe
      setQrzPassword('');
      
      // Auto-masquer le message après 10 secondes
      setTimeout(() => {
        setSuccessMessage(null);
      }, 10000);
      
    } catch (err) {
      console.error('Erreur lors de la mise à jour du profil:', err);
      setErrorMessage('Erreur lors de la mise à jour du profil. Veuillez réessayer.');
    } finally {
      setIsLoading(false);
    }
  };

  if (!user) {
    return (
      <div className="auth-container">
        <div className="auth-card">
          <h1>Erreur</h1>
          <p>Vous devez être connecté pour accéder à cette page.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1>Mon Profil</h1>
        
        <div className="profile-info">
          <p><strong>Nom d'utilisateur :</strong> {user.userName}</p>
          <p><strong>Indicatif :</strong> {user.callSign || 'Non défini'}</p>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          <button type="button" onClick={initializeFields} className="btn btn-secondary" style={{marginBottom: '1rem'}}>
            Remplir avec les valeurs actuelles
          </button>
          
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Votre adresse email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="qrzUsername">Nom d'utilisateur QRZ.com</label>
            <input
              type="text"
              id="qrzUsername"
              name="qrzUsername"
              value={qrzUsername}
              onChange={(e) => setQrzUsername(e.target.value)}
              placeholder="Votre nom d'utilisateur QRZ.com"
            />
          </div>

          <div className="form-group">
            <label htmlFor="qrzPassword">Mot de passe QRZ.com</label>
            <div className="password-input-container">
              <input
                type={showPassword ? 'text' : 'password'}
                id="qrzPassword"
                name="qrzPassword"
                value={qrzPassword}
                onChange={(e) => setQrzPassword(e.target.value)}
                placeholder="Votre mot de passe QRZ.com"
              />
              <button
                type="button"
                className="password-toggle"
                onClick={() => setShowPassword(!showPassword)}
                aria-label={showPassword ? 'Masquer le mot de passe' : 'Afficher le mot de passe'}
              >
                {showPassword ? '👁️' : '👁️‍🗨️'}
              </button>
            </div>
            <small className="form-help">
              Laissez vide pour ne pas modifier le mot de passe actuel
            </small>
          </div>

          {errorMessage && <div className="error-message">{errorMessage}</div>}
          {successMessage && <div className="success-message">{successMessage}</div>}

          <button 
            type="submit" 
            className="btn btn-primary"
            disabled={isLoading}
          >
            {isLoading ? 'Mise à jour...' : 'Mettre à jour le profil'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default ProfilePageDirect;
