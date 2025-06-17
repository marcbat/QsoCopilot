// Utility functions for handling API errors

export interface ApiError {
  response?: {
    data?: {
      errors?: string[];
      message?: string;
    };
  };
  message?: string;
}

/**
 * Extract error message from API response
 * @param error - The error object from API call
 * @param defaultMessage - Default message if no specific error found
 * @returns Formatted error message
 */
export function extractErrorMessage(error: ApiError, defaultMessage: string = 'Une erreur est survenue'): string {
  // API errors array (from backend validation)
  if (error.response?.data?.errors && Array.isArray(error.response.data.errors)) {
    return error.response.data.errors.join(', ');
  }
  
  // Simple API error message
  if (error.response?.data?.message) {
    return error.response.data.message;
  }
  
  // Network or other errors
  if (error.message) {
    return error.message;
  }
  
  return defaultMessage;
}

/**
 * Extract multiple error messages as an array
 * @param error - The error object from API call
 * @returns Array of error messages
 */
export function extractErrorMessages(error: ApiError): string[] {
  // API errors array (from backend validation)
  if (error.response?.data?.errors && Array.isArray(error.response.data.errors)) {
    return error.response.data.errors;
  }
  
  // Simple API error message
  if (error.response?.data?.message) {
    return [error.response.data.message];
  }
  
  // Network or other errors
  if (error.message) {
    return [error.message];
  }
  
  return ['Une erreur est survenue'];
}
