import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { QsoAggregateDto, ParticipantDto } from '../types';
import { qsoApiService } from '../api/qsoApi';
import { useAuth } from '../contexts/AuthContext';

const QsoDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const [qso, setQso] = useState<QsoAggregateDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      setError('ID du QSO manquant');
      setIsLoading(false);
      return;
    }

    loadQso();
  }, [id]);

  const loadQso = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const response = await qsoApiService.getQso(id!);
      setQso(response);
    } catch (err) {
      console.error('Erreur lors du chargement du QSO:', err);
      setError('Impossible de charger les détails du QSO');
    } finally {
      setIsLoading(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Non définie';
    return new Date(dateString).toLocaleDateString('fr-FR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const handleEdit = () => {
    navigate(`/qso/${id}/edit`);
  };

  const handleBack = () => {
    navigate('/');
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

  if (error || !qso) {
    return (
      <div className="page-container">
        <div className="error-message">
          <h2>Erreur</h2>
          <p>{error || 'QSO introuvable'}</p>
          <button onClick={handleBack} className="btn btn-secondary">
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
          ← Retour
        </button>
        <h1>Détails du QSO</h1>
        {isAuthenticated && (
          <button onClick={handleEdit} className="btn btn-primary">
            Éditer
          </button>
        )}
      </div>

      <div className="qso-detail-content">
        <div className="detail-section">
          <div className="detail-card">
            <h2>{qso.name}</h2>
            
            <div className="detail-grid">              <div className="detail-item">
                <label>Description :</label>
                <p>{qso.description || 'Aucune description'}</p>
              </div>

              <div className="detail-item">
                <label>Fréquence :</label>
                <p>{qso.frequency ? `${qso.frequency} MHz` : 'Non définie'}</p>
              </div>

              <div className="detail-item">
                <label>Date de début :</label>
                <p>{formatDate(qso.startDateTime)}</p>
              </div>              <div className="detail-item">
                <label>Date de fin :</label>
                <p>{formatDate(qso.endDateTime)}</p>
              </div>              <div className="detail-item">
                <label>Mode :</label>
                <p>{qso.mode || 'Non défini'}</p>
              </div>
            </div>
          </div>

          <div className="detail-card">
            <h3>Participants ({qso.participants?.length || 0})</h3>
            
            {qso.participants && qso.participants.length > 0 ? (
              <div className="participants-list">
                {qso.participants.map((participant: ParticipantDto, index: number) => (
                  <div key={index} className="participant-card">
                    <div className="participant-info">
                      <h4>{participant.callSign}</h4>                      <div className="participant-details">
                        {participant.name && <p><strong>Nom :</strong> {participant.name}</p>}
                        {participant.location && <p><strong>Localisation :</strong> {participant.location}</p>}
                        {participant.signalReport && <p><strong>Rapport signal :</strong> {participant.signalReport}</p>}
                        {participant.notes && <p><strong>Notes :</strong> {participant.notes}</p>}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="no-participants">
                <p>Aucun participant ajouté pour ce QSO</p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default QsoDetailPage;
