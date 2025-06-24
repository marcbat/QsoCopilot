import React from 'react';
import { useToasts } from '../hooks/useToasts';
import ToastContainer from './ToastContainer';

const ToastTestPage: React.FC = () => {
  const { toasts, removeToast, showSuccess, showError, showInfo } = useToasts();

  return (
    <div className="main-content" style={{ padding: '20px' }}>
      <h1>Test des Toasts</h1>
      <p>Cliquez sur les boutons ci-dessous pour tester l'empilage des toasts :</p>
      
      <div style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
        <button 
          onClick={() => showSuccess('Participant ajouté avec succès!')}
          style={{ 
            padding: '10px 15px', 
            backgroundColor: '#4CAF50', 
            color: 'white',
            border: 'none',
            borderRadius: '5px',
            cursor: 'pointer'
          }}
        >
          Toast de Succès
        </button>
        
        <button 
          onClick={() => showError('Erreur lors de l\'ajout du participant')}
          style={{ 
            padding: '10px 15px', 
            backgroundColor: '#f44336', 
            color: 'white',
            border: 'none',
            borderRadius: '5px',
            cursor: 'pointer'
          }}
        >
          Toast d'Erreur
        </button>
        
        <button 
          onClick={() => showInfo('Information importante')}
          style={{ 
            padding: '10px 15px', 
            backgroundColor: '#2196F3', 
            color: 'white',
            border: 'none',
            borderRadius: '5px',
            cursor: 'pointer'
          }}
        >
          Toast d'Info
        </button>
        
        <button 
          onClick={() => {
            showSuccess('Premier toast');
            setTimeout(() => showSuccess('Deuxième toast'), 200);
            setTimeout(() => showError('Troisième toast (erreur)'), 400);
            setTimeout(() => showInfo('Quatrième toast (info)'), 600);
          }}
          style={{ 
            padding: '10px 15px', 
            backgroundColor: '#9C27B0', 
            color: 'white',
            border: 'none',
            borderRadius: '5px',
            cursor: 'pointer'
          }}
        >
          Test Empilage (4 toasts)
        </button>
      </div>
      
      <div>
        <h3>Toasts actifs : {toasts.length}</h3>
        <ul>
          {toasts.map(toast => (
            <li key={toast.id}>
              [{toast.type}] {toast.message}
            </li>
          ))}
        </ul>
      </div>
      
      <ToastContainer toasts={toasts} onRemoveToast={removeToast} />
    </div>
  );
};

export default ToastTestPage;
