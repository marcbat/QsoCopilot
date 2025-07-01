import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { UpdateProfileRequest } from '../types';

const ProfilePageNew: React.FC = () => {
  const { user, updateProfile, isLoading } = useAuth();
    // États du formulaire - initialisés directement avec les valeurs utilisateur
  const [formData, setFormData] = useState<UpdateProfileRequest>({
    email: user?.email || '',
    qrzUsername: user?.qrzUsername || '',
    qrzPassword: ''
  });
  
  const [showPassword, setShowPassword] = useState(false);
  // États pour les messages
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  
  // État pour éviter la réinitialisation multiple du formulaire  // Debug: log l'état des messages à chaque rendu
  console.log('ProfilePageNew rendu - successMessage:', successMessage, 'errorMessage:', errorMessage);

  // Auto-masquer le message de succès après 10 secondes
  useEffect(() => {
    if (successMessage) {
      const timer = setTimeout(() => {
        setSuccessMessage(null);
      }, 10000);
      return () => clearTimeout(timer);
    }
  }, [successMessage]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    // Effacer seulement les messages d'erreur lors de la modification
    if (errorMessage) setErrorMessage(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

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
        setErrorMessage('Aucune modification détectée');
        return;
      }      const confirmationMessage = await updateProfile(updateData);
      console.log('Message de confirmation reçu:', confirmationMessage);
      setSuccessMessage(confirmationMessage);
      console.log('setSuccessMessage appelé avec:', confirmationMessage);
      
      // Vérifier l'état immédiatement après (ce sera encore l'ancien état)
      console.log('État successMessage actuel (ancien):', successMessage);
      
      // Réinitialiser le mot de passe après mise à jour réussie
      setFormData(prev => ({ ...prev, qrzPassword: '' }));
    } catch (err) {
      console.error('Erreur lors de la mise à jour du profil:', err);
      setErrorMessage('Erreur lors de la mise à jour du profil. Veuillez réessayer.');
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
          </div>          {errorMessage && <div className="error-message">{errorMessage}</div>}
          {successMessage && <div className="success-message">{successMessage}</div>}
          
          {/* Debug: div toujours visible pour tester */}
          <div style={{border: '1px solid red', padding: '5px', margin: '5px'}}>
            Debug - successMessage: "{successMessage}" | errorMessage: "{errorMessage}"
          </div>

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

export default ProfilePageNew;
