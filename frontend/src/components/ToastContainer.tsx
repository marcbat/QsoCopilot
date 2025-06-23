import React from 'react';
import Toast from './Toast';
import { ToastData } from '../hooks/useToasts';

interface ToastContainerProps {
  toasts: ToastData[];
  onRemoveToast: (id: string) => void;
}

const ToastContainer: React.FC<ToastContainerProps> = ({ toasts, onRemoveToast }) => {
  return (
    <div
      style={{
        position: 'fixed',
        bottom: '20px',
        right: '20px',
        zIndex: 1000,
        display: 'flex',
        flexDirection: 'column-reverse', // Les nouveaux toasts apparaissent en bas
        gap: '10px', // Espacement entre les toasts
        pointerEvents: 'none' // Permet au clic de passer à travers le conteneur
      }}
    >
      {toasts.map((toast) => (        <div
          key={toast.id}
          style={{
            pointerEvents: 'auto', // Réactive les interactions sur les toasts individuels
            transition: 'all 0.3s ease-in-out' // Animation fluide pour le réarrangement
          }}
        >
          <Toast
            message={toast.message}
            type={toast.type}
            duration={toast.duration}
            onClose={() => onRemoveToast(toast.id)}
          />
        </div>
      ))}
    </div>
  );
};

export default ToastContainer;
