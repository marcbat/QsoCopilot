import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { QsoAggregateDto, ParticipantDto, CreateParticipantRequest } from '../types';
import { qsoApiService } from '../api/qsoApi';
import { useAuth } from '../contexts/AuthContext';
import { useMessages } from '../hooks/useMessages';
import ParticipantCard from './ParticipantCard';
import ParticipantTable from './ParticipantTable';
import ParticipantMap from './ParticipantMap';
// @ts-ignore - Temporary ignore for build
import { extractErrorMessage } from '../utils/errorUtils';

const QsoDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();  const [qso, setQso] = useState<QsoAggregateDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'details' | 'table' | 'map'>('details');
  const [newParticipant, setNewParticipant] = useState({
    callSign: ''
  });
  const [isAddingParticipant, setIsAddingParticipant] = useState(false);
  
  // Utiliser le hook de messages avec auto-hide
  const { successMessage, errorMessage, setSuccessMessage, setErrorMessage } = useMessages();
  useEffect(() => {
    if (!id) {
      setErrorMessage('ID du QSO manquant');
      setIsLoading(false);
      return;
    }

    loadQso();
  }, [id]);
  const loadQso = async () => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
      const response = await qsoApiService.getQso(id!);
      setQso(response);    } catch (err: any) {
      console.error('Erreur lors du chargement du QSO:', err);
      setErrorMessage(extractErrorMessage(err, 'Impossible de charger les détails du QSO'));
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

  const handleParticipantChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { value } = e.target;
    setNewParticipant({ callSign: value });
  };

  const handleAddParticipant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!qso || !newParticipant.callSign.trim()) return;    try {
      setIsAddingParticipant(true);
      setErrorMessage(null);
      setSuccessMessage(null);

      const participantData: CreateParticipantRequest = {
        callSign: newParticipant.callSign
      };

      await qsoApiService.addParticipant(qso.id, participantData);
      setSuccessMessage('Participant ajouté avec succès');

      // Réinitialiser le formulaire de participant
      setNewParticipant({ callSign: '' });

      // Recharger les données
      await loadQso();    } catch (err: any) {
      console.error('Erreur lors de l\'ajout du participant:', err);
      setErrorMessage(extractErrorMessage(err, 'Impossible d\'ajouter le participant'));
    }finally {
      setIsAddingParticipant(false);
    }
  };

  const handleRemoveParticipant = async (callSign: string) => {
    if (!qso) return;
    
    if (!confirm(`Êtes-vous sûr de vouloir supprimer ${callSign} du QSO ?`)) {
      return;
    }

    try {
      setErrorMessage(null);
      setSuccessMessage(null);

      await qsoApiService.removeParticipant(qso.id, callSign);
      setSuccessMessage(`Participant ${callSign} supprimé avec succès`);

      // Recharger les données
      await loadQso();
    } catch (err: any) {
      console.error('Erreur lors de la suppression du participant:', err);
      setErrorMessage(extractErrorMessage(err, 'Impossible de supprimer le participant'));
    }
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
  if (errorMessage || !qso) {
    return (
      <div className="page-container">
        <div className="error-message">
          <h2>Erreur</h2>
          <p>{errorMessage || 'QSO introuvable'}</p>
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
          </button>        )}
      </div>      {errorMessage && (
        <div className="error-message" style={{ marginBottom: '1rem' }}>
          {errorMessage}
        </div>
      )}

      {successMessage && (
        <div className="success-message" style={{ marginBottom: '1rem' }}>
          {successMessage}
        </div>
      )}      <div className="qso-detail-content">
        <div className="detail-section">
          <div className="detail-card">            <h2>
              {qso.name}
              {qso.description && (
                <span style={{ fontWeight: 'normal', color: 'var(--text-secondary)', fontSize: '0.9em' }}>
                  {' '}({qso.description})
                </span>
              )}
            </h2>
              <div style={{ 
              display: 'flex', 
              gap: '2rem', 
              flexWrap: 'wrap',
              alignItems: 'baseline',
              marginTop: '1rem'
            }}>              <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.5rem' }}>
                <label style={{ fontWeight: '600', fontSize: '0.875rem' }}>Fréquence :</label>
                <span>{qso.frequency ? `${qso.frequency.toFixed(3)} MHz` : 'Non définie'}</span>
              </div>              <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.5rem' }}>
                <label style={{ fontWeight: '600', fontSize: '0.875rem' }}>Date de début :</label>
                <span>{formatDate(qso.startDateTime)}</span>
              </div>

              {qso.endDateTime && (
                <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.5rem' }}>
                  <label style={{ fontWeight: '600', fontSize: '0.875rem' }}>Date de fin :</label>
                  <span>{formatDate(qso.endDateTime)}</span>
                </div>
              )}

              {qso.mode && (
                <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.5rem' }}>
                  <label style={{ fontWeight: '600', fontSize: '0.875rem' }}>Mode :</label>
                  <span>{qso.mode}</span>
                </div>
              )}
            </div>
          </div>
        </div>        {/* Section participants */}
        <div className="detail-section">
          <div className="detail-card">
            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center', 
              marginBottom: '1rem' 
            }}>
              <h3>Participants ({qso.participants?.length || 0})</h3>
            </div>
            
            {/* Formulaire rapide d'ajout de participant */}
            {isAuthenticated && (
              <div className="quick-add-participant" style={{ marginBottom: '1rem' }}>
                <form onSubmit={handleAddParticipant} className="quick-participant-form">
                  <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                    <input
                      type="text"
                      name="callSign"
                      value={newParticipant.callSign}
                      onChange={handleParticipantChange}
                      placeholder="Indicatif (ex: F1ABC)"
                      required
                      className="form-input"
                      style={{ flex: 1 }}
                    />
                    <button 
                      type="submit" 
                      className="btn btn-primary" 
                      disabled={isAddingParticipant}
                      style={{ whiteSpace: 'nowrap' }}
                    >
                      {isAddingParticipant ? 'Ajout...' : 'Ajouter participant'}
                    </button>
                  </div>
                </form>
              </div>
            )}

            {/* Onglets */}
            <div className="tabs-container" style={{ marginBottom: '1rem' }}>
              <div className="tabs-header" style={{ 
                display: 'flex', 
                borderBottom: '2px solid var(--border-color)',
                marginBottom: '1rem'
              }}>
                <button
                  className={`tab-button ${activeTab === 'details' ? 'active' : ''}`}
                  onClick={() => setActiveTab('details')}
                  style={{
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    background: 'transparent',
                    cursor: 'pointer',
                    fontSize: '1rem',
                    fontWeight: activeTab === 'details' ? '600' : '400',
                    color: activeTab === 'details' ? 'var(--primary-color)' : 'var(--text-secondary)',
                    borderBottom: activeTab === 'details' ? '2px solid var(--primary-color)' : '2px solid transparent',
                    marginBottom: '-2px',
                    transition: 'all 0.2s ease'
                  }}
                >
                  Détails
                </button>                <button
                  className={`tab-button ${activeTab === 'table' ? 'active' : ''}`}
                  onClick={() => setActiveTab('table')}
                  style={{
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    background: 'transparent',
                    cursor: 'pointer',
                    fontSize: '1rem',
                    fontWeight: activeTab === 'table' ? '600' : '400',
                    color: activeTab === 'table' ? 'var(--primary-color)' : 'var(--text-secondary)',
                    borderBottom: activeTab === 'table' ? '2px solid var(--primary-color)' : '2px solid transparent',
                    marginBottom: '-2px',
                    transition: 'all 0.2s ease'
                  }}
                >
                  Table
                </button>
                <button
                  className={`tab-button ${activeTab === 'map' ? 'active' : ''}`}
                  onClick={() => setActiveTab('map')}
                  style={{
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    background: 'transparent',
                    cursor: 'pointer',
                    fontSize: '1rem',
                    fontWeight: activeTab === 'map' ? '600' : '400',
                    color: activeTab === 'map' ? 'var(--primary-color)' : 'var(--text-secondary)',
                    borderBottom: activeTab === 'map' ? '2px solid var(--primary-color)' : '2px solid transparent',
                    marginBottom: '-2px',
                    transition: 'all 0.2s ease'
                  }}
                >
                  Carte
                </button>
              </div>
            </div>            {/* Contenu des onglets */}
            {qso.participants && qso.participants.length > 0 ? (
              <div className="tab-content">
                {activeTab === 'details' ? (
                  <div className="participants-list">
                    {/* Affichage détaillé avec cartes */}
                    {qso.participants.map((participant: ParticipantDto, index: number) => (
                      <ParticipantCard
                        key={index}
                        participant={participant}
                        onRemove={isAuthenticated ? handleRemoveParticipant : undefined}
                        showRemoveButton={isAuthenticated}
                      />
                    ))}
                  </div>
                ) : activeTab === 'table' ? (
                  /* Affichage en table */
                  <ParticipantTable
                    participants={qso.participants}
                    onRemove={isAuthenticated ? handleRemoveParticipant : undefined}
                    showRemoveButton={isAuthenticated}
                  />
                ) : (
                  /* Affichage sur carte */
                  <ParticipantMap
                    participants={qso.participants}
                  />
                )}
              </div>
            ) : (
              <div className="no-participants">
                <p>Aucun participant ajouté pour ce QSO</p>
              </div>
            )}</div>
        </div>
      </div>
    </div>
  );
};

export default QsoDetailPage;
