import React, { useState } from 'react';
import { CreateQsoRequest } from '../types';
import { qsoApiService } from '../api';
import { useToasts } from '../hooks/useToasts';
import ToastContainer from './ToastContainer';
import { extractErrorMessage } from '../utils/errorUtils';

interface CreateQsoFormProps {
  onQsoCreated: (qsoId: string) => void;
}

const CreateQsoForm: React.FC<CreateQsoFormProps> = ({ onQsoCreated }) => {  const [formData, setFormData] = useState<CreateQsoRequest>({
    name: '',
    frequency: 0
  });
  const [isLoading, setIsLoading] = useState(false);
  
  // Utiliser le hook de toasts pour toutes les notifications
  const { toasts, removeToast, showSuccess, showError } = useToasts();

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ 
      ...prev, 
      [name]: name === 'frequency' ? (value ? parseFloat(value) : 0) : value 
    }));
  };  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      const qsoData = await qsoApiService.createQsoAggregate(formData);
      showSuccess(`QSO créé avec succès: ${qsoData.name}`);
        // Réinitialiser le formulaire
      setFormData({
        name: '',
        frequency: 0
      });
      
      // Notifier le parent
      onQsoCreated(qsoData.id);
    } catch (err: any) {
      showError(extractErrorMessage(err, 'Erreur lors de la création du QSO'));
    } finally {
      setIsLoading(false);
    }
  };
  return (
    <div className="create-qso-form">
      <div className="card-header">
        <h2 className="card-title">🚀 Créer un nouveau QSO</h2>
      </div>      <form onSubmit={handleSubmit} className="qso-form-horizontal">
        <div className="form-group" style={{ flex: '2', marginRight: '1rem' }}>
          <label htmlFor="name" className="form-label">Nom du QSO *</label>
          <input
            type="text"
            id="name"
            name="name"
            className="form-input"
            value={formData.name}
            onChange={handleInputChange}
            placeholder="Ex: QSO Dimanche Matin"
            required
          />
        </div>

        <div className="form-group" style={{ flex: '1' }}>
          <label htmlFor="frequency" className="form-label">Fréquence (MHz) *</label>
          <input
            type="number"
            id="frequency"
            name="frequency"
            className="form-input"
            value={formData.frequency || ''}
            onChange={handleInputChange}
            placeholder="14.200"
            step="0.001"
            min="0.001"
            required
          />
        </div>

        <div className="form-group">
          <button 
            type="submit" 
            className="btn btn-primary" 
            disabled={isLoading || !formData.name || !formData.frequency}
          >
            {isLoading ? 'Création...' : 'Créer le QSO'}
          </button>        </div>
      </form>
      
      {/* Container pour les toasts */}
      <ToastContainer toasts={toasts} onRemoveToast={removeToast} />
    </div>
  );
};

export default CreateQsoForm;
