import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { QsoAggregateDto, UpdateQsoRequest } from '../types';
import { qsoApiService } from '../api/qsoApi';
import { useAuth } from '../contexts/AuthContext';
import { useMessages } from '../hooks/useMessages';
import { useToasts } from '../hooks/useToasts';
import ToastContainer from './ToastContainer';
import { extractErrorMessage } from '../utils/errorUtils';

const QsoEditPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();  const [qso, setQso] = useState<QsoAggregateDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  
  // Utiliser le hook de messages avec auto-hide
  const { errorMessage, setErrorMessage } = useMessages();
  
  // Utiliser le hook de toasts pour les notifications de succès
  const { toasts, removeToast, showSuccess } = useToasts();
  
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
    }    if (!id) {
      setErrorMessage('ID du QSO manquant');
      setIsLoading(false);
      return;
    }

    loadQso();
  }, [id, isAuthenticated]);
  const loadQso = async () => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
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
      setErrorMessage(extractErrorMessage(err, 'Impossible de charger les détails du QSO'));
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
    if (!qso) return;    try {
      setIsSaving(true);
      setErrorMessage(null);      const updateData: UpdateQsoRequest = {
        name: formData.name,
        description: formData.description,
        startDateTime: formData.startDateTime ? new Date(formData.startDateTime).toISOString() : undefined,
        endDateTime: formData.endDateTime ? new Date(formData.endDateTime).toISOString() : undefined,
        frequency: formData.frequency ? parseFloat(formData.frequency) : undefined,
        mode: formData.mode || undefined
      };      await qsoApiService.updateQso(qso.id, updateData);
      showSuccess('QSO mis à jour avec succès');
      
      // Recharger les données
      await loadQso();    } catch (err: any) {
      console.error('Erreur lors de la mise à jour:', err);
      setErrorMessage(extractErrorMessage(err, 'Impossible de mettre à jour le QSO'));
    }finally {
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
  if (errorMessage && !qso) {
    return (
      <div className="page-container">
        <div className="error-message">
          <h2>Erreur</h2>
          <p>{errorMessage}</p>
          <button onClick={() => navigate('/')} className="btn btn-secondary">
            Retour à la liste
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container">      <div className="page-header">
        <button onClick={handleBack} className="btn btn-secondary">
          ← Retour aux détails
        </button>
        <h1>Éditer le QSO</h1>
        <button onClick={handleViewDetails} className="btn btn-secondary">
          Voir les détails
        </button>
      </div>{errorMessage && (
        <div className="error-message" style={{ marginBottom: '1rem' }}>
          {errorMessage}        </div>
      )}

      <div className="edit-content">
        {/* Formulaire d'édition QSO */}        <div className="edit-section">
          <h2>Informations du QSO</h2>
          <form onSubmit={handleSaveQso} className="qso-form-compact">
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
          </form>        </div>
      </div>{/* Container pour les toasts */}
      <ToastContainer toasts={toasts} onRemoveToast={removeToast} />
    </div>
  );
};

export default QsoEditPage;
