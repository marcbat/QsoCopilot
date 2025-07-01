import React, { useState } from 'react';
import { AddParticipantRequest } from '../types';
import { qsoApiService } from '../api';
import { useMessages } from '../hooks/useMessages';
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
}) => {  const [participant, setParticipant] = useState<AddParticipantRequest>({
    callSign: '',
    name: ''
  });

  const [isLoading, setIsLoading] = useState(false);
  
  // Utiliser le hook de messages avec auto-hide
  const { successMessage, errorMessage, setSuccessMessage, setErrorMessage } = useMessages();

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setParticipant(prev => ({ ...prev, [name]: value }));
  };
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!participant.callSign) return;

    setIsLoading(true);
    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      await qsoApiService.addParticipant(qsoId, participant);
      setSuccessMessage(`Participant ${participant.callSign} ajouté avec succès!`);
      
      // Réinitialiser le formulaire
      setParticipant({
        callSign: '',
        name: ''
      });
      
      // Notifier le parent pour rafraîchir les données
      onParticipantAdded();
    } catch (err: any) {
      setErrorMessage(extractErrorMessage(err, 'Erreur lors de l\'ajout du participant'));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="add-participant-form">
      <div className="card-header">
        <h3 className="card-title">Ajouter un participant au QSO: {qsoName}</h3>
      </div>      
      {errorMessage && <div className="alert alert-error">{errorMessage}</div>}
      {successMessage && <div className="alert alert-success">{successMessage}</div>}

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
