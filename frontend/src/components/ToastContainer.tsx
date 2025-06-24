import React, { memo } from 'react';
import Toast from './Toast';
import { ToastData } from '../hooks/useToasts';

interface ToastWrapperProps {
  toast: ToastData;
  onRemoveToast: (id: string) => void;
}

const ToastWrapper: React.FC<ToastWrapperProps> = memo(({ toast, onRemoveToast }) => {
  const handleClose = () => onRemoveToast(toast.id);
  
  return (
    <div
      style={{
        pointerEvents: 'auto', // Réactive les interactions sur les toasts individuels
        transition: 'all 0.3s ease-in-out' // Animation fluide pour le réarrangement
      }}
    >
      <Toast
        message={toast.message}
        type={toast.type}
        duration={toast.duration}
        onClose={handleClose}
      />
    </div>
  );
});

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
      {toasts.map((toast) => (
        <ToastWrapper
          key={toast.id}
          toast={toast}
          onRemoveToast={onRemoveToast}
        />
      ))}
    </div>
  );
};

export default ToastContainer;
