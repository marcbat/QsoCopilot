import { useState, useCallback, useRef } from 'react';

interface UseMessagesResult {
  successMessage: string | null;
  errorMessage: string | null;
  setSuccessMessage: (message: string | null) => void;
  setErrorMessage: (message: string | null) => void;
  clearMessages: () => void;
}

export const useMessages = (autoHideDelay: number = 5000): UseMessagesResult => {
  const [successMessage, setSuccessMessageState] = useState<string | null>(null);
  const [errorMessage, setErrorMessageState] = useState<string | null>(null);
  
  // Références pour pouvoir annuler les timeouts précédents
  const successTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const errorTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  const setSuccessMessage = useCallback((message: string | null) => {
    // Annuler le timeout précédent s'il existe
    if (successTimeoutRef.current) {
      clearTimeout(successTimeoutRef.current);
      successTimeoutRef.current = null;
    }

    setSuccessMessageState(message);

    // Si on définit un message, programmer sa disparition
    if (message) {
      successTimeoutRef.current = setTimeout(() => {
        setSuccessMessageState(null);
        successTimeoutRef.current = null;
      }, autoHideDelay);
    }
  }, [autoHideDelay]);

  const setErrorMessage = useCallback((message: string | null) => {
    // Annuler le timeout précédent s'il existe
    if (errorTimeoutRef.current) {
      clearTimeout(errorTimeoutRef.current);
      errorTimeoutRef.current = null;
    }

    setErrorMessageState(message);

    // Si on définit un message, programmer sa disparition
    if (message) {
      errorTimeoutRef.current = setTimeout(() => {
        setErrorMessageState(null);
        errorTimeoutRef.current = null;
      }, autoHideDelay);
    }
  }, [autoHideDelay]);

  const clearMessages = useCallback(() => {
    // Annuler tous les timeouts en cours
    if (successTimeoutRef.current) {
      clearTimeout(successTimeoutRef.current);
      successTimeoutRef.current = null;
    }
    if (errorTimeoutRef.current) {
      clearTimeout(errorTimeoutRef.current);
      errorTimeoutRef.current = null;
    }

    // Effacer les messages
    setSuccessMessageState(null);
    setErrorMessageState(null);
  }, []);

  return {
    successMessage,
    errorMessage,
    setSuccessMessage,
    setErrorMessage,
    clearMessages,
  };
};
