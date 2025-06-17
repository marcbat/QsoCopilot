import React, { useState } from 'react';
import { AddParticipantRequest } from '../types';
import { qsoApiService } from '../api';
import { extractErrorMessage } from '../utils/errorUtils';

interface AddParticipantFormProps {
  qsoId: string;
  qsoName: string;
  onParticipantAdded: () => void;
}

const AddParticipantForm: React.FC<AddParticipantFormProps> = ({ 
  qsoId, 
  qsoName, 
  onParticipantAdded 
}) => {
  const [participant, setParticipant] = useState<AddParticipantRequest>({
    callSign: '',
    name: ''
  });

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setParticipant(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!participant.callSign) return;

    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      await qsoApiService.addParticipant(qsoId, participant);
      setSuccess(`Participant ${participant.callSign} ajouté avec succès!`);
      
      // Réinitialiser le formulaire
      setParticipant({
        callSign: '',
        name: ''
      });
      
      // Notifier le parent pour rafraîchir les données
      onParticipantAdded();
      
      // Effacer le message de succès après 3 secondes
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(extractErrorMessage(err, 'Erreur lors de l\'ajout du participant'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="add-participant-form">
      <div className="card-header">
        <h3 className="card-title">Ajouter un participant au QSO: {qsoName}</h3>
      </div>
      
      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <form onSubmit={handleSubmit} className="qso-form-horizontal">
        <div className="form-group">
          <label htmlFor="callSign" className="form-label">Indicatif *</label>
          <input
            type="text"
            id="callSign"
            name="callSign"
            className="form-input"
            value={participant.callSign}
            onChange={handleInputChange}
            placeholder="Ex: F1ABC"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="name" className="form-label">Nom</label>
          <input
            type="text"
            id="name"
            name="name"
            className="form-input"
            value={participant.name}
            onChange={handleInputChange}
            placeholder="Nom du participant"
          />
        </div>

        <div className="form-group">
          <button 
            type="submit" 
            className="btn btn-primary" 
            disabled={isLoading || !participant.callSign}
          >
            {isLoading ? 'Ajout...' : 'Ajouter participant'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default AddParticipantForm;
