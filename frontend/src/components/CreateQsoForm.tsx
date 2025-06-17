import React, { useState } from 'react';
import { CreateQsoRequest } from '../types';
import { qsoApiService } from '../api';
import { extractErrorMessage } from '../utils/errorUtils';

interface CreateQsoFormProps {
  onQsoCreated: (qsoId: string) => void;
}

const CreateQsoForm: React.FC<CreateQsoFormProps> = ({ onQsoCreated }) => {
  const [formData, setFormData] = useState<CreateQsoRequest>({
    name: '',
    description: '',
    frequency: 0
  });

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ 
      ...prev, 
      [name]: name === 'frequency' ? (value ? parseFloat(value) : 0) : value 
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const qsoData = await qsoApiService.createQsoAggregate(formData);
      setSuccess(`QSO créé avec succès: ${qsoData.name}`);
      
      // Réinitialiser le formulaire
      setFormData({
        name: '',
        description: '',
        frequency: 0
      });
      
      // Notifier le parent
      onQsoCreated(qsoData.id);
      
      // Effacer le message de succès après 3 secondes
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(extractErrorMessage(err, 'Erreur lors de la création du QSO'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="create-qso-form">      <div className="card-header">
        <h2 className="card-title">🚀 Créer un nouveau QSO</h2>
      </div>
      
      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <form onSubmit={handleSubmit} className="qso-form-horizontal">
        <div className="form-group">
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

        <div className="form-group">
          <label htmlFor="description" className="form-label">Description</label>
          <input
            type="text"
            id="description"
            name="description"
            className="form-input"
            value={formData.description}
            onChange={handleInputChange}
            placeholder="Description du QSO..."
          />
        </div>

        <div className="form-group">
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
          </button>
        </div>
      </form>
    </div>
  );
};

export default CreateQsoForm;
