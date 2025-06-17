import { useState, useEffect } from 'react';

export const useTheme = () => {  const [theme, setTheme] = useState<'light' | 'dark'>(() => {
    // Vérifier d'abord s'il y a une préférence sauvegardée
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'light' || savedTheme === 'dark') {
      return savedTheme;
    }
    
    // Par défaut, utiliser le thème sombre
    return 'dark';
  });

  useEffect(() => {
    // Appliquer le thème au document
    document.documentElement.setAttribute('data-theme', theme);
    
    // Sauvegarder la préférence
    localStorage.setItem('theme', theme);
  }, [theme]);

  const toggleTheme = () => {
    setTheme(prevTheme => prevTheme === 'light' ? 'dark' : 'light');
  };

  return { theme, toggleTheme };
};
