import React, { useState } from 'react';
import { CreateQsoRequest, CreateParticipantRequest } from '../types';
import { qsoApiService } from '../api/qsoApi';

interface QsoFormProps {
  onQsoCreated: (qsoId: string) => void;
}

const QsoForm: React.FC<QsoFormProps> = ({ onQsoCreated }) => {  const [formData, setFormData] = useState<CreateQsoRequest>({
    name: '',
    description: '',
    frequency: 0,
    startDateTime: '',
    mode: ''
  });  const [participant, setParticipant] = useState<CreateParticipantRequest>({
    callSign: '',
    name: ''
  });

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [qsoCreated, setQsoCreated] = useState(false);
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ 
      ...prev, 
      [name]: name === 'frequency' ? (value ? parseFloat(value) : 0) : value 
    }));
  };  const handleParticipantChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setParticipant(prev => ({ 
      ...prev, 
      [name]: value 
    }));
  };

  const handleCreateQso = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const qsoData = await qsoApiService.createQso(formData);
      setSuccess(`QSO créé avec succès: ${qsoData.name}`);
      setQsoCreated(true);
      onQsoCreated(qsoData.id);      // Réinitialiser le formulaire QSO
      setFormData({
        name: '',
        description: '',
        frequency: 0,
        startDateTime: '',
        mode: ''
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la création du QSO');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddParticipant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!qsoCreated || !participant.callSign) return;

    setIsLoading(true);
    setError(null);

    try {
      await qsoApiService.addParticipant(formData.id || '', participant);
      setSuccess(`Participant ${participant.callSign} ajouté avec succès`);        // Réinitialiser le formulaire participant
      setParticipant({
        callSign: '',
        name: ''
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de l\'ajout du participant');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="qso-form-container">
      <div className="qso-form">
        <h2>Créer un nouveau QSO</h2>
        
        {error && <div className="error-message">{error}</div>}
        {success && <div className="success-message">{success}</div>}

        <form onSubmit={handleCreateQso} className="form-horizontal">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="name">Nom du QSO *</label>
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                required
                placeholder="Ex: Contest DX, Expédition..."
              />
            </div>

            <div className="form-group">
              <label htmlFor="frequency">Fréquence (MHz)</label>
              <input
                type="number"
                id="frequency"
                name="frequency"
                value={formData.frequency || ''}
                onChange={handleInputChange}
                step="0.001"
                min="0"
                placeholder="14.200"
              />
            </div>

            <div className="form-group">
              <label htmlFor="mode">Mode</label>
              <input
                type="text"
                id="mode"
                name="mode"
                value={formData.mode}
                onChange={handleInputChange}
                placeholder="SSB, CW, FT8..."
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="description">Description</label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                placeholder="Description du QSO..."
                rows={2}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="startDateTime">Date/Heure de début</label>
              <input
                type="datetime-local"
                id="startDateTime"
                name="startDateTime"
                value={formData.startDateTime}
                onChange={handleInputChange}
              />            </div>

            <div className="form-group">
              <button
                type="submit" 
                className="btn btn-primary"
                disabled={isLoading || !formData.name}
              >
                {isLoading ? 'Création...' : 'Créer QSO'}
              </button>
            </div>
          </div>
        </form>

        {/* Formulaire d'ajout de participant (visible seulement après création du QSO) */}
        {qsoCreated && (
          <div className="participant-form-section">
            <h3>Ajouter un participant</h3>
            <form onSubmit={handleAddParticipant} className="form-horizontal">
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="participant-callSign">Indicatif *</label>
                  <input
                    type="text"
                    id="participant-callSign"
                    name="callSign"
                    value={participant.callSign}
                    onChange={handleParticipantChange}
                    required
                    placeholder="F1ABC"
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="participant-name">Nom</label>
                  <input
                    type="text"
                    id="participant-name"
                    name="name"
                    value={participant.name}
                    onChange={handleParticipantChange}
                    placeholder="Nom du radioamateur"
                  />                </div>

                <div className="form-group">
                  <button 
                    type="submit" 
                    className="btn btn-secondary"
                    disabled={isLoading || !participant.callSign}
                  >
                    {isLoading ? 'Ajout...' : 'Ajouter'}
                  </button>
                </div>
              </div>
            </form>
          </div>
        )}
      </div>
    </div>
  );
};

export default QsoForm;
