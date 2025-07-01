import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

const RegisterPage: React.FC = () => {
  const { register, isLoading } = useAuth();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    userName: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validation
    if (formData.password !== formData.confirmPassword) {
      setError('Les mots de passe ne correspondent pas');
      return;
    }

    if (formData.password.length < 6) {
      setError('Le mot de passe doit contenir au moins 6 caractères');
      return;
    }

    try {
      await register({
        userName: formData.userName,
        email: formData.email,
        password: formData.password
      });
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erreur lors de l\'inscription');
    }
  };

  return (
    <div className="main-content">
      <div style={{ maxWidth: '400px', margin: '0 auto' }}>
        <div className="card">
          <div className="card-header">
            <h1 className="card-title">Créer un compte</h1>
          </div>

          {error && <div className="alert alert-error">{error}</div>}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="userName" className="form-label">Nom d'utilisateur / Indicatif *</label>
              <input
                type="text"
                id="userName"
                name="userName"
                className="form-input"
                value={formData.userName}
                onChange={handleInputChange}
                placeholder="Ex: F1ABC"
                required
              />
              <small style={{ color: 'var(--text-secondary)', fontSize: '0.75rem' }}>
                Utilisez votre indicatif radioamateur si vous en avez un
              </small>
            </div>

            <div className="form-group">
              <label htmlFor="email" className="form-label">Email *</label>
              <input
                type="email"
                id="email"
                name="email"
                className="form-input"
                value={formData.email}
                onChange={handleInputChange}
                placeholder="votre@email.com"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="password" className="form-label">Mot de passe *</label>
              <input
                type="password"
                id="password"
                name="password"
                className="form-input"
                value={formData.password}
                onChange={handleInputChange}
                placeholder="Minimum 6 caractères"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword" className="form-label">Confirmer le mot de passe *</label>
              <input
                type="password"
                id="confirmPassword"
                name="confirmPassword"
                className="form-input"
                value={formData.confirmPassword}
                onChange={handleInputChange}
                placeholder="Répétez votre mot de passe"
                required
              />
            </div>

            <div className="form-group">
              <button 
                type="submit" 
                className="btn btn-primary" 
                disabled={isLoading}
                style={{ width: '100%' }}
              >
                {isLoading ? 'Création du compte...' : 'Créer le compte'}
              </button>
            </div>
          </form>

          <div style={{ textAlign: 'center', marginTop: '1.5rem', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
            <p>Déjà un compte ?</p>
            <Link to="/login" className="btn btn-secondary">
              Se connecter
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
