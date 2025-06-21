import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { UpdateProfileRequest } from '../types';

const ProfilePage: React.FC = () => {
  const { user, updateProfile, isLoading } = useAuth();
  const [formData, setFormData] = useState<UpdateProfileRequest>({
    email: user?.email || '',
    qrzUsername: user?.qrzUsername || '',
    qrzPassword: ''
  });
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    // Effacer les messages d'erreur/succès lors de la modification
    if (error) setError('');
    if (success) setSuccess('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      // Ne pas envoyer les champs vides
      const updateData: UpdateProfileRequest = {};
      if (formData.email && formData.email !== user?.email) {
        updateData.email = formData.email;
      }
      if (formData.qrzUsername && formData.qrzUsername !== user?.qrzUsername) {
        updateData.qrzUsername = formData.qrzUsername;
      }
      if (formData.qrzPassword) {
        updateData.qrzPassword = formData.qrzPassword;
      }

      // Vérifier qu'il y a au moins un champ à mettre à jour
      if (Object.keys(updateData).length === 0) {
        setError('Aucune modification détectée');
        return;
      }

      await updateProfile(updateData);
      setSuccess('Profil mis à jour avec succès !');
      // Réinitialiser le mot de passe après mise à jour réussie
      setFormData(prev => ({ ...prev, qrzPassword: '' }));
    } catch (err) {
      console.error('Erreur lors de la mise à jour du profil:', err);
      setError('Erreur lors de la mise à jour du profil. Veuillez réessayer.');
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
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="Votre adresse email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="qrzUsername">Nom d'utilisateur QRZ.com</label>
            <input
              type="text"
              id="qrzUsername"
              name="qrzUsername"
              value={formData.qrzUsername}
              onChange={handleChange}
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
                value={formData.qrzPassword}
                onChange={handleChange}
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

          {error && <div className="error-message">{error}</div>}
          {success && <div className="success-message">{success}</div>}

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

export default ProfilePage;
