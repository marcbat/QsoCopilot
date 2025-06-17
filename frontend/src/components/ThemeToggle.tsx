import React from 'react';
import { useTheme } from '../hooks/useTheme';

const ThemeToggle: React.FC = () => {
  const { theme, toggleTheme } = useTheme();

  return (
    <button
      onClick={toggleTheme}
      className="theme-toggle"
      title={`Basculer vers le thème ${theme === 'light' ? 'sombre' : 'clair'}`}
      aria-label={`Basculer vers le thème ${theme === 'light' ? 'sombre' : 'clair'}`}
    >
      <div className={`theme-toggle-slider ${theme}`}>
        {theme === 'light' ? '☀️' : '🌙'}
      </div>
    </button>
  );
};

export default ThemeToggle;
