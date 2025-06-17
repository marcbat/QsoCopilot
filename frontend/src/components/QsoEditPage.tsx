import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { QsoAggregateDto, ParticipantDto, UpdateQsoRequest } from '../types';
import { qsoApiService } from '../api/qsoApi';
import { useAuth } from '../contexts/AuthContext';
import { extractErrorMessage } from '../utils/errorUtils';

const QsoEditPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [qso, setQso] = useState<QsoAggregateDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  // États pour le formulaire QSO
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    startDateTime: '',
    endDateTime: '',
    frequency: '',
    mode: ''
  });

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    if (!id) {
      setError('ID du QSO manquant');
      setIsLoading(false);
      return;
    }

    loadQso();
  }, [id, isAuthenticated]);

  const loadQso = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const response = await qsoApiService.getQso(id!);
      setQso(response);
        // Remplir le formulaire avec les données existantes
      setFormData({
        name: response.name || '',
        description: response.description || '',
        startDateTime: response.startDateTime ? formatDateForInput(response.startDateTime) : '',
        endDateTime: response.endDateTime ? formatDateForInput(response.endDateTime) : '',
        frequency: response.frequency?.toString() || '',
        mode: response.mode || ''
      });    } catch (err: any) {
      console.error('Erreur lors du chargement du QSO:', err);
      setError(extractErrorMessage(err, 'Impossible de charger les détails du QSO'));
    } finally {
      setIsLoading(false);
    }
  };

  const formatDateForInput = (dateString: string) => {
    const date = new Date(dateString);
    return date.toISOString().slice(0, 16); // Format YYYY-MM-DDTHH:mm
  };
  const handleFormChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSaveQso = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!qso) return;

    try {
      setIsSaving(true);
      setError(null);      const updateData: UpdateQsoRequest = {
        name: formData.name,
        description: formData.description,
        startDateTime: formData.startDateTime ? new Date(formData.startDateTime).toISOString() : undefined,
        endDateTime: formData.endDateTime ? new Date(formData.endDateTime).toISOString() : undefined,
        frequency: formData.frequency ? parseFloat(formData.frequency) : undefined,
        mode: formData.mode || undefined
      };

      await qsoApiService.updateQso(qso.id, updateData);
      setSuccessMessage('QSO mis à jour avec succès');
      
      // Recharger les données
      await loadQso();    } catch (err: any) {
      console.error('Erreur lors de la mise à jour:', err);
      setError(extractErrorMessage(err, 'Impossible de mettre à jour le QSO'));
    } finally {
      setIsSaving(false);
    }
  };  const handleBack = () => {
    navigate(`/qso/${id}`);
  };

  const handleViewDetails = () => {
    navigate(`/qso/${id}`);
  };

  if (isLoading) {
    return (
      <div className="page-container">
        <div className="loading">
          <div className="spinner"></div>
          Chargement du QSO...
        </div>
      </div>
    );
  }

  if (error && !qso) {
    return (
      <div className="page-container">
        <div className="error-message">
          <h2>Erreur</h2>
          <p>{error}</p>
          <button onClick={() => navigate('/')} className="btn btn-secondary">
            Retour à la liste
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <div className="page-header">
        <button onClick={handleBack} className="btn btn-secondary">
          ← Retour aux détails
        </button>
        <h1>Éditer le QSO</h1>
        <button onClick={handleViewDetails} className="btn btn-outline">
          Voir les détails
        </button>
      </div>

      {error && (
        <div className="error-message" style={{ marginBottom: '1rem' }}>
          {error}
        </div>
      )}

      {successMessage && (
        <div className="success-message" style={{ marginBottom: '1rem' }}>
          {successMessage}
        </div>
      )}

      <div className="edit-content">
        {/* Formulaire d'édition QSO */}
        <div className="edit-section">
          <h2>Informations du QSO</h2>
          <form onSubmit={handleSaveQso} className="qso-form">
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="name">Nom du QSO *</label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleFormChange}
                  required
                />
              </div>              <div className="form-group">
                <label htmlFor="frequency">Fréquence (MHz) *</label>
                <input
                  type="number"
                  id="frequency"
                  name="frequency"
                  value={formData.frequency}
                  onChange={handleFormChange}
                  step="0.001"
                  min="0.001"
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="description">Description</label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleFormChange}
                rows={3}
              />
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="startDateTime">Date de début</label>
                <input
                  type="datetime-local"
                  id="startDateTime"
                  name="startDateTime"
                  value={formData.startDateTime}
                  onChange={handleFormChange}
                />
              </div>

              <div className="form-group">
                <label htmlFor="endDateTime">Date de fin</label>
                <input
                  type="datetime-local"
                  id="endDateTime"
                  name="endDateTime"
                  value={formData.endDateTime}
                  onChange={handleFormChange}
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="mode">Mode</label>
                <input
                  type="text"
                  id="mode"
                  name="mode"
                  value={formData.mode}
                  onChange={handleFormChange}
                  placeholder="ex: SSB, CW, FT8..."
                />              </div>
            </div>

            <button type="submit" className="btn btn-primary" disabled={isSaving}>
              {isSaving ? 'Sauvegarde...' : 'Sauvegarder les modifications'}
            </button>
          </form>
        </div>        {/* Section participants */}
        <div className="edit-section">
          <h2>Participants ({qso?.participants?.length || 0})</h2>
          
          {/* Liste des participants existants */}
          {qso?.participants && qso.participants.length > 0 && (
            <div className="existing-participants">
              <h3>Participants actuels</h3>
              <div className="participants-grid">
                {qso.participants.map((participant: ParticipantDto, index: number) => (                  <div key={index} className="participant-card">
                    <h4>{participant.callSign}</h4>
                    {participant.name && <p><strong>Nom :</strong> {participant.name}</p>}
                    {participant.location && <p><strong>Localisation :</strong> {participant.location}</p>}
                    {participant.signalReport && <p><strong>Rapport :</strong> {participant.signalReport}</p>}
                    {participant.notes && <p><strong>Notes :</strong> {participant.notes}</p>}
                  </div>
                ))}
              </div>
            </div>          )}
        </div>
      </div>
    </div>
  );
};

export default QsoEditPage;
