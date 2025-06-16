import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

const LoginPage: React.FC = () => {
  const { login, loginByEmail, isLoading } = useAuth();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    userName: '',
    email: '',
    password: ''
  });
  const [loginType, setLoginType] = useState<'username' | 'email'>('username');
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      if (loginType === 'username') {
        await login({ userName: formData.userName, password: formData.password });
      } else {
        await loginByEmail({ email: formData.email, password: formData.password });
      }
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erreur de connexion');
    }
  };

  return (
    <div className="main-content">
      <div style={{ maxWidth: '400px', margin: '0 auto' }}>
        <div className="card">
          <div className="card-header">
            <h1 className="card-title">Connexion</h1>
          </div>

          {error && <div className="alert alert-error">{error}</div>}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">Type de connexion</label>
              <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  <input
                    type="radio"
                    value="username"
                    checked={loginType === 'username'}
                    onChange={(e) => setLoginType(e.target.value as 'username')}
                  />
                  Nom d'utilisateur
                </label>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  <input
                    type="radio"
                    value="email"
                    checked={loginType === 'email'}
                    onChange={(e) => setLoginType(e.target.value as 'email')}
                  />
                  Email
                </label>
              </div>
            </div>

            {loginType === 'username' ? (
              <div className="form-group">
                <label htmlFor="userName" className="form-label">Nom d'utilisateur / Indicatif</label>
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
              </div>
            ) : (
              <div className="form-group">
                <label htmlFor="email" className="form-label">Email</label>
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
            )}

            <div className="form-group">
              <label htmlFor="password" className="form-label">Mot de passe</label>
              <input
                type="password"
                id="password"
                name="password"
                className="form-input"
                value={formData.password}
                onChange={handleInputChange}
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
                {isLoading ? 'Connexion...' : 'Se connecter'}
              </button>
            </div>
          </form>

          <div style={{ textAlign: 'center', marginTop: '1.5rem', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
            <p>Pas encore de compte ?</p>
            <Link to="/register" className="btn btn-secondary">
              Créer un compte
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
