import React, { useState } from 'react';
import { CreateQsoRequest, AddParticipantRequest } from '../types';
import { qsoApiService } from '../api';

interface QsoFormProps {
  onQsoCreated: (qsoId: string) => void;
}

const QsoForm: React.FC<QsoFormProps> = ({ onQsoCreated }) => {
  const [formData, setFormData] = useState<CreateQsoRequest>({
    id: '',
    frequency: '',
    mode: 'SSB',
    band: '20m'
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

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleParticipantChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setParticipant(prev => ({ ...prev, [name]: value }));
  };

  const generateQsoId = () => {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    return `qso-${timestamp}`;
  };

  const handleCreateQso = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    setSuccess(null);

    try {
      const qsoData = {
        ...formData,
        id: formData.id || generateQsoId()
      };

      await qsoApiService.createQsoAggregate(qsoData);
      setSuccess(`QSO créé avec succès! ID: ${qsoData.id}`);
      setQsoCreated(true);
      onQsoCreated(qsoData.id);
      
      // Mettre à jour l'ID dans le formulaire pour l'ajout de participants
      setFormData(prev => ({ ...prev, id: qsoData.id }));
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur inconnue');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddParticipant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.id || !participant.callSign) return;

    setIsLoading(true);
    setError(null);

    try {
      await qsoApiService.addParticipant(formData.id, participant);
      setSuccess(`Participant ${participant.callSign} ajouté avec succès!`);
      
      // Réinitialiser le formulaire participant
      setParticipant({
        callSign: '',
        name: '',
        qth: '',
        rstSent: '59',
        rstReceived: '59'
      });
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de l\'ajout du participant');
    } finally {
      setIsLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      id: '',
      frequency: '',
      mode: 'SSB',
      band: '20m'
    });
    setParticipant({
      callSign: '',
      name: '',
      qth: '',
      rstSent: '59',
      rstReceived: '59'
    });
    setQsoCreated(false);
    setError(null);
    setSuccess(null);
  };

  return (
    <div className="card">
      <h2>Créer un nouveau QSO</h2>
      
      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {!qsoCreated ? (
        <form onSubmit={handleCreateQso}>
          <div className="form-group">
            <label htmlFor="id">ID du QSO (optionnel)</label>
            <input
              type="text"
              id="id"
              name="id"
              value={formData.id}
              onChange={handleInputChange}
              placeholder="Laissez vide pour génération automatique"
            />
          </div>

          <div className="form-group">
            <label htmlFor="frequency">Fréquence *</label>
            <input
              type="text"
              id="frequency"
              name="frequency"
              value={formData.frequency}
              onChange={handleInputChange}
              placeholder="Ex: 14.205"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="mode">Mode *</label>
            <select
              id="mode"
              name="mode"
              value={formData.mode}
              onChange={handleInputChange}
              required
            >
              <option value="SSB">SSB</option>
              <option value="CW">CW</option>
              <option value="FM">FM</option>
              <option value="AM">AM</option>
              <option value="FT8">FT8</option>
              <option value="FT4">FT4</option>
              <option value="PSK31">PSK31</option>
              <option value="RTTY">RTTY</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="band">Bande *</label>
            <select
              id="band"
              name="band"
              value={formData.band}
              onChange={handleInputChange}
              required
            >
              <option value="160m">160m</option>
              <option value="80m">80m</option>
              <option value="40m">40m</option>
              <option value="20m">20m</option>
              <option value="17m">17m</option>
              <option value="15m">15m</option>
              <option value="12m">12m</option>
              <option value="10m">10m</option>
              <option value="6m">6m</option>
              <option value="2m">2m</option>
              <option value="70cm">70cm</option>
            </select>
          </div>

          <button type="submit" className="btn btn-primary" disabled={isLoading}>
            {isLoading ? 'Création...' : 'Créer le QSO'}
          </button>
        </form>
      ) : (
        <div>
          <h3>Ajouter un participant</h3>
          <form onSubmit={handleAddParticipant}>
            <div className="form-group">
              <label htmlFor="callSign">Indicatif *</label>
              <input
                type="text"
                id="callSign"
                name="callSign"
                value={participant.callSign}
                onChange={handleParticipantChange}
                placeholder="Ex: F1ABC"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="name">Nom</label>
              <input
                type="text"
                id="name"
                name="name"
                value={participant.name}
                onChange={handleParticipantChange}
                placeholder="Nom du participant"
              />
            </div>

            <div className="form-group">
              <label htmlFor="qth">QTH</label>
              <input
                type="text"
                id="qth"
                name="qth"
                value={participant.qth}
                onChange={handleParticipantChange}
                placeholder="Localisation"
              />
            </div>

            <div className="form-group">
              <label htmlFor="rstSent">RST Envoyé</label>
              <input
                type="text"
                id="rstSent"
                name="rstSent"
                value={participant.rstSent}
                onChange={handleParticipantChange}
                placeholder="59"
              />
            </div>

            <div className="form-group">
              <label htmlFor="rstReceived">RST Reçu</label>
              <input
                type="text"
                id="rstReceived"
                name="rstReceived"
                value={participant.rstReceived}
                onChange={handleParticipantChange}
                placeholder="59"
              />
            </div>

            <div style={{ display: 'flex', gap: '10px' }}>
              <button type="submit" className="btn btn-primary" disabled={isLoading || !participant.callSign}>
                {isLoading ? 'Ajout...' : 'Ajouter le participant'}
              </button>
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
