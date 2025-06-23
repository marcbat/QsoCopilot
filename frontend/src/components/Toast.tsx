import React, { useEffect, useState } from 'react';

interface ToastProps {
  message: string;
  type: 'success' | 'error' | 'info';
  duration?: number;
  onClose: () => void;
}

const Toast: React.FC<ToastProps> = ({ message, type, duration = 4000, onClose }) => {
  const [isVisible, setIsVisible] = useState(true);
  const [isAnimatingOut, setIsAnimatingOut] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsAnimatingOut(true);
      setTimeout(() => {
        setIsVisible(false);
        onClose();
      }, 300); // Durée de l'animation de sortie
    }, duration);

    return () => clearTimeout(timer);
  }, [duration, onClose]);

  if (!isVisible) return null;

  const getToastStyles = () => {
    const baseStyles = {
      position: 'fixed' as const,
      bottom: '20px',
      right: '20px',
      padding: '12px 16px',
      borderRadius: '6px',
      color: 'white',
      fontWeight: '500',
      fontSize: '14px',
      maxWidth: '350px',
      zIndex: 1000,
      boxShadow: '0 4px 12px rgba(0, 0, 0, 0.2)',
      transform: isAnimatingOut ? 'translateX(100%)' : 'translateX(0)',
      opacity: isAnimatingOut ? 0 : 1,
      transition: 'all 0.3s ease-in-out',
    };

    const typeStyles = {
      success: {
        backgroundColor: '#4CAF50',
        border: '1px solid #45a049'
      },
      error: {
        backgroundColor: '#f44336',
        border: '1px solid #da190b'
      },
      info: {
        backgroundColor: '#2196F3',
        border: '1px solid #0b7dda'
      }
    };

    return { ...baseStyles, ...typeStyles[type] };
  };

  return (
    <div style={getToastStyles()}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <span>{message}</span>
        <button
          onClick={() => {
            setIsAnimatingOut(true);
            setTimeout(() => {
              setIsVisible(false);
              onClose();
            }, 300);
          }}
          style={{
            marginLeft: '12px',
            background: 'none',
            border: 'none',
            color: 'white',
            cursor: 'pointer',
            fontSize: '16px',
            padding: '0',
            lineHeight: '1'
          }}
        >
          ×
        </button>
      </div>
    </div>
  );
};

export default Toast;
