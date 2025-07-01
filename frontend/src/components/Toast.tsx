import React, { useEffect, useState, useRef, useCallback } from 'react';

interface ToastProps {
  message: string;
  type: 'success' | 'error' | 'info';
  duration?: number;
  onClose: () => void;
}

const Toast: React.FC<ToastProps> = ({ message, type, duration = 4000, onClose }) => {
  const [isVisible, setIsVisible] = useState(true);
  const [isAnimatingOut, setIsAnimatingOut] = useState(false);
  const [isAnimatingIn, setIsAnimatingIn] = useState(true);
  
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  const handleClose = useCallback(() => {
    setIsAnimatingOut(true);
    setTimeout(() => {
      setIsVisible(false);
      onCloseRef.current();
    }, 300);
  }, []);

  useEffect(() => {
    // Animation d'entrée
    const enterTimer = setTimeout(() => {
      setIsAnimatingIn(false);
    }, 50);

    // Animation de sortie automatique
    const timer = setTimeout(() => {
      handleClose();
    }, duration);

    return () => {
      clearTimeout(enterTimer);
      clearTimeout(timer);
    };
  }, [duration, handleClose]);

  if (!isVisible) return null;  const getToastStyles = () => {
    const baseStyles = {
      position: 'relative' as const,
      padding: '12px 16px',
      borderRadius: '6px',
      color: 'white',
      fontWeight: '500',
      fontSize: '14px',
      maxWidth: '350px',
      boxShadow: '0 4px 12px rgba(0, 0, 0, 0.2)',
      transform: isAnimatingOut 
        ? 'translateX(100%)' 
        : isAnimatingIn 
          ? 'translateX(50px)' 
          : 'translateX(0)',
      opacity: isAnimatingOut ? 0 : isAnimatingIn ? 0.7 : 1,
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
        <span>{message}</span>        <button
          onClick={handleClose}
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
