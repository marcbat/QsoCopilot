import React, { useState } from 'react';
import { CreateQsoRequest, AddParticipantRequest } from '../types';
import { qsoApiService } from '../api';

interface QsoFormProps {
  onQsoCreated: (qsoId: string) => void;
}

const QsoForm: React.FC<QsoFormProps> = ({ onQsoCreated }) => {  const [formData, setFormData] = useState<CreateQsoRequest>({
    name: '',
    description: '',
    frequency: 0
  });
  
  const [participant, setParticipant] = useState<AddParticipantRequest>({
    callSign: '',
    name: '',
    qth: '',
    rstSent: '59',
    rstReceived: '59'
  });

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [qsoCreated, setQsoCreated] = useState(false);
  const [createdQsoId, setCreatedQsoId] = useState<string | null>(null);
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ 
      ...prev, 
      [name]: name === 'frequency' ? (value ? parseFloat(value) : 0) : value 
    }));
  };

  const handleParticipantChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setParticipant(prev => ({ ...prev, [name]: value }));
  };

  const handleCreateQso = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const qsoData = await qsoApiService.createQsoAggregate(formData);
      setSuccess(`QSO créé avec succès: ${qsoData.name}`);
      setQsoCreated(true);
      setCreatedQsoId(qsoData.id);
      onQsoCreated(qsoData.id);
      
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Erreur inconnue');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddParticipant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!createdQsoId || !participant.callSign) return;

    setIsLoading(true);
    setError(null);

    try {
      await qsoApiService.addParticipant(createdQsoId, participant);
      setSuccess(`Participant ${participant.callSign} ajouté avec succès!`);
      
      // Réinitialiser le formulaire participant
      setParticipant({
        callSign: '',
        name: '',
        qth: '',
        rstSent: '59',
        rstReceived: '59'
      });
      
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Erreur lors de l\'ajout du participant');
    } finally {
      setIsLoading(false);
    }
  };

  const resetForm = () => {    setFormData({
      name: '',
      description: '',
      frequency: 0
    });
    setParticipant({
      callSign: '',
      name: '',
      qth: '',
      rstSent: '59',
      rstReceived: '59'
    });
    setQsoCreated(false);
    setCreatedQsoId(null);
    setError(null);
    setSuccess(null);
  };

  return (
    <div className="qso-form">
      <div className="card-header">
        <h2 className="card-title">Créer un nouveau QSO</h2>
      </div>
      
      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {!qsoCreated ? (
        // Formulaire de création QSO - horizontal
        <form onSubmit={handleCreateQso} className="qso-form-horizontal">
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
          </div>          <div className="form-group">
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
          </div>          <div className="form-group">
            <button type="submit" className="btn btn-primary" disabled={isLoading || !formData.name || !formData.frequency}>
              {isLoading ? 'Création...' : 'Créer le QSO'}
            </button>
          </div>
        </form>
      ) : (
        // Formulaire d'ajout de participant
        <div>
          <h3>Ajouter des participants au QSO: {formData.name}</h3>
          <form onSubmit={handleAddParticipant} className="qso-form-horizontal">
            <div className="form-group">
              <label htmlFor="callSign" className="form-label">Indicatif *</label>
              <input
                type="text"
                id="callSign"
                name="callSign"
                className="form-input"
                value={participant.callSign}
                onChange={handleParticipantChange}
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
                onChange={handleParticipantChange}
                placeholder="Nom du participant"
              />
            </div>

            <div className="form-group">
              <label htmlFor="qth" className="form-label">QTH</label>
              <input
                type="text"
                id="qth"
                name="qth"
                className="form-input"
                value={participant.qth}
                onChange={handleParticipantChange}
                placeholder="Localisation"
              />
            </div>

            <div className="form-group">
              <label htmlFor="rstSent" className="form-label">RST Envoyé</label>
              <input
                type="text"
                id="rstSent"
                name="rstSent"
                className="form-input"
                value={participant.rstSent}
                onChange={handleParticipantChange}
                placeholder="59"
              />
            </div>

            <div className="form-group">
              <label htmlFor="rstReceived" className="form-label">RST Reçu</label>
              <input
                type="text"
                id="rstReceived"
                name="rstReceived"
                className="form-input"
                value={participant.rstReceived}
                onChange={handleParticipantChange}
                placeholder="59"
              />
            </div>

            <div className="form-group">
              <button type="submit" className="btn btn-primary" disabled={isLoading || !participant.callSign}>
                {isLoading ? 'Ajout...' : 'Ajouter participant'}
              </button>
            </div>

            <div className="form-group">
              <button type="button" className="btn btn-secondary" onClick={resetForm}>
                Nouveau QSO
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};

export default QsoForm;
