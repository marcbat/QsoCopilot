import React, { useState } from 'react';
import { extractErrorMessage } from '../utils/errorUtils';

// Composant de test pour démontrer la gestion d'erreurs améliorée
const ErrorTestComponent: React.FC = () => {
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Simulation d'une erreur API avec tableau d'erreurs
  const simulateApiErrorArray = () => {
    const mockError = {
      response: {
        data: {
          errors: [
            "Un QSO Aggregate avec le nom 'Test QSO Session' existe déjà",
            "La fréquence doit être supérieure à 0"
          ]
        }
      }
    };
    
    const errorMessage = extractErrorMessage(mockError, 'Erreur par défaut');
    setError(errorMessage);
    setSuccess(null);
  };

  // Simulation d'une erreur API avec message simple
  const simulateApiErrorMessage = () => {
    const mockError = {
      response: {
        data: {
          message: "Utilisateur non authentifié"
        }
      }
    };
    
    const errorMessage = extractErrorMessage(mockError, 'Erreur par défaut');
    setError(errorMessage);
    setSuccess(null);
  };

  // Simulation d'une erreur réseau
  const simulateNetworkError = () => {
    const mockError = {
      message: "Network Error: Unable to connect to server"
    };
    
    const errorMessage = extractErrorMessage(mockError, 'Erreur par défaut');
    setError(errorMessage);
    setSuccess(null);
  };

  const clearMessages = () => {
    setError(null);
    setSuccess(null);
  };

  return (
    <div style={{ padding: '20px', maxWidth: '600px', margin: '0 auto' }}>
      <h2>Test de Gestion d'Erreurs Améliorée</h2>
      
      {error && (
        <div style={{ 
          background: '#ffebee', 
          color: '#c62828', 
          padding: '12px', 
          borderRadius: '4px', 
          marginBottom: '16px',
          border: '1px solid #ef5350'
        }}>
          <strong>Erreur:</strong> {error}
        </div>
      )}
      
      {success && (
        <div style={{ 
          background: '#e8f5e8', 
          color: '#2e7d32', 
          padding: '12px', 
          borderRadius: '4px', 
          marginBottom: '16px',
          border: '1px solid #4caf50'
        }}>
          <strong>Succès:</strong> {success}
        </div>
      )}

      <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap', marginBottom: '20px' }}>
        <button 
          onClick={simulateApiErrorArray}
          style={{ 
            padding: '8px 16px', 
            background: '#f44336', 
            color: 'white', 
            border: 'none', 
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Tester Erreurs API (Array)
        </button>
        
        <button 
          onClick={simulateApiErrorMessage}
          style={{ 
            padding: '8px 16px', 
            background: '#ff9800', 
            color: 'white', 
            border: 'none', 
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Tester Erreur API (Message)
        </button>
        
        <button 
          onClick={simulateNetworkError}
          style={{ 
            padding: '8px 16px', 
            background: '#9c27b0', 
            color: 'white', 
            border: 'none', 
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Tester Erreur Réseau
        </button>
        
        <button 
          onClick={clearMessages}
          style={{ 
            padding: '8px 16px', 
            background: '#607d8b', 
            color: 'white', 
            border: 'none', 
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Effacer
        </button>
      </div>

      <div style={{ background: '#f5f5f5', padding: '16px', borderRadius: '4px' }}>
        <h3>Informations sur le Test</h3>
        <p><strong>Objectif:</strong> Démontrer que l'utilitaire <code>extractErrorMessage</code> fonctionne correctement</p>
        <ul>
          <li><strong>Erreurs API (Array):</strong> Extrait et joint les messages d'un tableau d'erreurs</li>
          <li><strong>Erreur API (Message):</strong> Extrait un message d'erreur simple</li>
          <li><strong>Erreur Réseau:</strong> Utilise le message d'erreur JavaScript natif</li>
        </ul>
        <p><strong>Résultat attendu:</strong> Les messages d'erreur détaillés de l'API sont affichés au lieu de messages génériques</p>
      </div>
    </div>
  );
};

export default ErrorTestComponent;
