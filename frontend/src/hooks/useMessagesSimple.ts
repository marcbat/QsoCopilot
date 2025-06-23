import { useState } from 'react';

interface UseMessagesResult {
  successMessage: string | null;
  errorMessage: string | null;
  setSuccessMessage: (message: string | null) => void;
  setErrorMessage: (message: string | null) => void;
  clearMessages: () => void;
}

export const useMessagesSimple = (): UseMessagesResult => {
  const [successMessage, setSuccessMessageState] = useState<string | null>(null);
  const [errorMessage, setErrorMessageState] = useState<string | null>(null);

  console.log('useMessagesSimple - État actuel:', { successMessage, errorMessage });

  const setSuccessMessage = (message: string | null) => {
    console.log('useMessagesSimple.setSuccessMessage appelé avec:', message);
    setSuccessMessageState(message);
    console.log('setSuccessMessageState appelé avec:', message);
  };

  const setErrorMessage = (message: string | null) => {
    console.log('useMessagesSimple.setErrorMessage appelé avec:', message);
    setErrorMessageState(message);
  };

  const clearMessages = () => {
    console.log('useMessagesSimple.clearMessages appelé');
    setSuccessMessage(null);
    setErrorMessage(null);
  };

  return {
    successMessage,
    errorMessage,
    setSuccessMessage,
    setErrorMessage,
    clearMessages,
  };
};
