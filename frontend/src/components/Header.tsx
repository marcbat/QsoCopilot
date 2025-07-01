import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import ThemeToggle from './ThemeToggle';

const Header: React.FC = () => {
  const { user, isAuthenticated, logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <header className="header">
      <div className="header-content">
        <Link to="/" className="logo">
          QSO Manager
        </Link>        <nav className="nav-buttons">
          {isAuthenticated ? (
            <>
              <div className="user-info">
                <span>Connecté en tant que: <strong>{user?.userName}</strong></span>
                {user?.callSign && user.callSign !== user.userName && (
                  <span>({user.callSign})</span>
                )}
              </div>
              <Link to="/profile" className="btn btn-secondary">
                Profil
              </Link>
              <button onClick={handleLogout} className="btn btn-secondary">
                Déconnexion
              </button>
              <ThemeToggle />
            </>
          ) : (
            <>
              <Link to="/register" className="btn btn-secondary">
                S'inscrire
              </Link>
              <Link to="/login" className="btn btn-primary">
                Se connecter
              </Link>
              <ThemeToggle />
            </>
          )}
        </nav>
      </div>
    </header>
  );
};

export default Header;
