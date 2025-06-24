import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { QsoAggregateDto, ParticipantDto, CreateParticipantRequest } from '../types';
import { qsoApiService } from '../api/qsoApi';
import { useAuth } from '../contexts/AuthContext';
import { useMessages } from '../hooks/useMessages';
import { useToasts } from '../hooks/useToasts';
import { useQsoSignalR } from '../hooks/useQsoSignalR';
import ToastContainer from './ToastContainer';
import ParticipantTable from './ParticipantTable';
import ParticipantMap from './ParticipantMap';
import ParticipantCard from './ParticipantCard';
import DraggableParticipantsList from './DraggableParticipantsList';
import { canUserReorderParticipants, canUserModifyQso, canUserFetchQrzInfo } from '../utils/authorizationUtils';
// @ts-ignore - Temporary ignore for build
import { extractErrorMessage } from '../utils/errorUtils';

const QsoDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();const [qso, setQso] = useState<QsoAggregateDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);  const [activeTab, setActiveTab] = useState<'details' | 'table' | 'map'>('details');
  const [newParticipant, setNewParticipant] = useState({
    callSign: ''
  });
  const [isAddingParticipant, setIsAddingParticipant] = useState(false);
  const [isReordering, setIsReordering] = useState(false);
    // Utiliser le hook de messages avec auto-hide
  const { errorMessage, setErrorMessage } = useMessages();
  // Utiliser le hook de toasts pour les notifications
  const { toasts, removeToast, showSuccess, showError } = useToasts();
  // Variable pour déterminer si les onglets Table et Carte doivent être désactivés
  const shouldDisableQrzTabs = !canUserFetchQrzInfo(user);

  // Callbacks pour les événements SignalR
  const handleParticipantAdded = useCallback((data: any) => {
    const receivedQsoId = data.qsoId;
    const participant = data.participant;
    
    if (receivedQsoId === id && qso && qso.participants) {
      // Vérifier si le participant n'existe pas déjà dans la liste
      const participantExists = qso.participants.some(p => 
        p.callSign?.toLowerCase() === participant.callSign?.toLowerCase()
      );
      
      if (!participantExists) {
        const updatedQso = {
          ...qso,
          participants: [...qso.participants, participant]
        };
        setQso(updatedQso);
        showSuccess(`${participant.callSign} a rejoint le QSO`);
      }
    }
  }, [id, qso, showSuccess]);  const handleParticipantRemoved = useCallback((data: any) => {
    const receivedQsoId = data.qsoId;
    const callSign = data.callSign;
    
    if (receivedQsoId === id && qso && qso.participants) {
      const updatedQso = {
        ...qso,
        participants: qso.participants.filter(p => 
          p.callSign?.toLowerCase() !== callSign?.toLowerCase()
        )
      };
      setQso(updatedQso);
      showSuccess(`${callSign} a quitté le QSO`);
    }
  }, [id, qso, showSuccess]);

  const handleParticipantsReordered = useCallback((data: any) => {
    const receivedQsoId = data.qsoId; // qsoId au lieu de QsoId
    const participants = data.participants; // participants au lieu de Participants
    
    if (receivedQsoId === id && qso) {
      const updatedQso = {
        ...qso,
        participants: participants
      };
      setQso(updatedQso);
    }
  }, [id, qso]);// Configurer SignalR
  const { connectionState } = useQsoSignalR(id || null, {
    onParticipantAdded: handleParticipantAdded,    onParticipantRemoved: handleParticipantRemoved,
    onParticipantsReordered: handleParticipantsReordered
  });

  useEffect(() => {
    if (!id) {
      setErrorMessage('ID du QSO manquant');
      setIsLoading(false);
      return;
    }

    loadQso();
  }, [id]);

  // Effet pour forcer le retour à l'onglet "détails" si les onglets QRZ sont désactivés
  useEffect(() => {
    if (shouldDisableQrzTabs && (activeTab === 'table' || activeTab === 'map')) {
      setActiveTab('details');
    }
  }, [shouldDisableQrzTabs, activeTab]);
  const loadQso = async () => {
    try {
      setIsLoading(true);      setErrorMessage(null);
      const response = await qsoApiService.getQso(id!);
      setQso(response);} catch (err: any) {
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
  };  const handleAddParticipant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!qso || !newParticipant.callSign.trim()) return;

    try {
      setIsAddingParticipant(true);
      setErrorMessage(null);

      const participantData: CreateParticipantRequest = {
        callSign: newParticipant.callSign
      };

      await qsoApiService.addParticipant(qso.id, participantData);
      
      // Réinitialiser le formulaire de participant
      setNewParticipant({ callSign: '' });

      // Recharger les données (fallback jusqu'à ce que SignalR soit complètement opérationnel)
      await loadQso();

    } catch (err: any) {
      console.error('Erreur lors de l\'ajout du participant:', err);
      const errorMessage = extractErrorMessage(err, 'Impossible d\'ajouter le participant');
      showError(errorMessage);
    } finally {
      setIsAddingParticipant(false);
    }
  };
  const handleRemoveParticipant = async (callSign: string) => {
    if (!qso) return;
    
    if (!confirm(`Êtes-vous sûr de vouloir supprimer ${callSign} du QSO ?`)) {
      return;
    }    try {
      setErrorMessage(null);

      await qsoApiService.removeParticipant(qso.id, callSign);
      showSuccess(`Participant ${callSign} supprimé avec succès`);

      // Recharger les données
      await loadQso();
    } catch (err: any) {
      console.error('Erreur lors de la suppression du participant:', err);
      setErrorMessage(extractErrorMessage(err, 'Impossible de supprimer le participant'));
    }
  };  const handleReorderParticipants = async (reorderedParticipants: ParticipantDto[]) => {
    if (!qso) return;

    // Vérification d'autorisation côté client
    if (!canUserReorderParticipants(user, qso)) {
      setErrorMessage('Seul le modérateur du QSO peut réordonner les participants.');
      return;
    }    try {
      setIsReordering(true);
      setErrorMessage(null);

      // Préparer la requête de réordonnancement
      const newOrders: { [callSign: string]: number } = {};
      reorderedParticipants.forEach(participant => {
        newOrders[participant.callSign] = participant.order;
      });

      const reorderRequest = { newOrders };

      await qsoApiService.reorderParticipants(qso.id, reorderRequest);
        // Mettre à jour l'état local immédiatement pour une meilleure expérience utilisateur
      setQso(prevQso => ({
        ...prevQso!,
        participants: reorderedParticipants
      }));

      showSuccess('Ordre des participants mis à jour avec succès');
      
      // Optionnel : Recharger les données pour s'assurer de la cohérence
      // await loadQso();
    } catch (err: any) {
      console.error('Erreur lors du réordonnancement des participants:', err);
      
      // Gérer spécifiquement l'erreur d'autorisation depuis le backend
      const errorMessage = extractErrorMessage(err, 'Impossible de réorganiser les participants');
      if (errorMessage.includes('modérateur')) {
        setErrorMessage('Seul le modérateur du QSO peut réordonner les participants.');
      } else {
        setErrorMessage(errorMessage);
      }
      
      // En cas d'erreur, recharger les données pour restaurer l'état précédent
      await loadQso();
    } finally {
      setIsReordering(false);
    }
  };

  const handleEdit = () => {
    navigate(`/qso/${id}/edit`);
  };

  const handleBack = () => {
    navigate('/');
  };  // Fonction pour générer le message informatif concernant les informations QRZ
  const getQrzInfoMessage = () => {
    if (!isAuthenticated) {
      return {
        type: 'info',
        message: 'ℹ️ Pour afficher les détails complets des participants (nom, localisation, etc.) et accéder aux onglets "Table" et "Carte", veuillez vous connecter et configurer vos identifiants QRZ.com dans votre profil.'
      };
    }
    
    if (!canUserFetchQrzInfo(user)) {
      return {
        type: 'warning', 
        message: '⚠️ Pour afficher les détails complets des participants et accéder aux onglets "Table" et "Carte", vous devez configurer vos identifiants QRZ.com dans votre profil.'
      };
    }
    
    return null;
  };

  // Fonction pour gérer le changement d'onglet avec validation
  const handleTabChange = (tab: 'details' | 'table' | 'map') => {
    if ((tab === 'table' || tab === 'map') && shouldDisableQrzTabs) {
      // Ne pas permettre de changer vers un onglet désactivé
      return;
    }
    setActiveTab(tab);
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
    <div className="page-container">      <div className="page-header">
        <button onClick={handleBack} className="btn btn-secondary">
          ← Retour
        </button>
        <div className="page-title-section">
          <h1>Détails du QSO</h1>          {/* Indicateur de connexion SignalR */}
          <div className={`signalr-status ${connectionState.toLowerCase()}`} title={`SignalR: ${connectionState}`}>
            <span className="status-dot"></span>
            <span className="status-text">Temps réel ({connectionState})</span>
          </div>
        </div>
        {isAuthenticated && (
          <button onClick={handleEdit} className="btn btn-primary">
            Éditer
          </button>
        )}
      </div>{errorMessage && (
        <div className="error-message" style={{ marginBottom: '1rem' }}>
          {errorMessage}
        </div>
      )}<div className="qso-detail-content">
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
              )}              {qso.mode && (
                <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.5rem' }}>
                  <label style={{ fontWeight: '600', fontSize: '0.875rem' }}>Mode :</label>
                  <span>{qso.mode}</span>
                </div>              )}
            </div>
          </div>
        </div>

        {/* Section participants */}
        <div className="detail-section">
          <div className="detail-card">            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center', 
              marginBottom: '1rem' 
            }}>
              <h3>Participants ({qso.participants?.length || 0})</h3>
            </div>

            {/* Message informatif concernant les informations QRZ */}
            {(() => {
              const qrzMessage = getQrzInfoMessage();
              if (!qrzMessage) return null;
              
              return (
                <div className="qrz-info-message" style={{ 
                  marginBottom: '1rem', 
                  padding: '0.75rem',
                  backgroundColor: qrzMessage.type === 'warning' ? 'var(--alert-warning-bg)' : 'var(--alert-success-bg)',
                  color: qrzMessage.type === 'warning' ? 'var(--alert-warning-color)' : 'var(--alert-success-color)',
                  border: `1px solid ${qrzMessage.type === 'warning' ? 'var(--alert-warning-border)' : 'var(--alert-success-border)'}`,
                  borderRadius: 'var(--border-radius)',
                  fontSize: '0.875rem'
                }}>
                  {qrzMessage.message}
                </div>
              );
            })()}

              {/* Formulaire rapide d'ajout de participant */}
            {canUserModifyQso(user, qso) && (
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

            {/* Message informatif pour les non-modérateurs */}
            {isAuthenticated && !canUserModifyQso(user, qso) && (
              <div className="info-message" style={{ 
                marginBottom: '1rem', 
                padding: '0.75rem',
                backgroundColor: 'var(--alert-warning-bg)',
                color: 'var(--alert-warning-color)',
                border: '1px solid var(--alert-warning-border)',
                borderRadius: 'var(--border-radius)',
                fontSize: '0.875rem'
              }}>
                ℹ️ Seul le modérateur du QSO peut ajouter, supprimer ou réordonner les participants.
              </div>
            )}

            {/* Onglets */}
            <div className="tabs-container" style={{ marginBottom: '1rem' }}>
              <div className="tabs-header" style={{ 
                display: 'flex', 
                borderBottom: '2px solid var(--border-color)',
                marginBottom: '1rem'
              }}>                <button
                  className={`tab-button ${activeTab === 'details' ? 'active' : ''}`}
                  onClick={() => handleTabChange('details')}
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
                </button>
                <button
                  className={`tab-button ${activeTab === 'table' ? 'active' : ''}`}
                  onClick={() => handleTabChange('table')}
                  disabled={shouldDisableQrzTabs}
                  title={shouldDisableQrzTabs ? 'Connectez-vous et configurez vos identifiants QRZ.com pour accéder à cet onglet' : ''}
                  style={{
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    background: 'transparent',
                    cursor: shouldDisableQrzTabs ? 'not-allowed' : 'pointer',
                    fontSize: '1rem',
                    fontWeight: activeTab === 'table' ? '600' : '400',
                    color: shouldDisableQrzTabs ? 'var(--text-disabled)' : (activeTab === 'table' ? 'var(--primary-color)' : 'var(--text-secondary)'),
                    borderBottom: activeTab === 'table' ? '2px solid var(--primary-color)' : '2px solid transparent',
                    marginBottom: '-2px',
                    transition: 'all 0.2s ease',
                    opacity: shouldDisableQrzTabs ? 0.5 : 1
                  }}
                >
                  Table
                </button>
                <button
                  className={`tab-button ${activeTab === 'map' ? 'active' : ''}`}
                  onClick={() => handleTabChange('map')}
                  disabled={shouldDisableQrzTabs}
                  title={shouldDisableQrzTabs ? 'Connectez-vous et configurez vos identifiants QRZ.com pour accéder à cet onglet' : ''}
                  style={{
                    padding: '0.75rem 1.5rem',
                    border: 'none',
                    background: 'transparent',
                    cursor: shouldDisableQrzTabs ? 'not-allowed' : 'pointer',
                    fontSize: '1rem',
                    fontWeight: activeTab === 'map' ? '600' : '400',
                    color: shouldDisableQrzTabs ? 'var(--text-disabled)' : (activeTab === 'map' ? 'var(--primary-color)' : 'var(--text-secondary)'),
                    borderBottom: activeTab === 'map' ? '2px solid var(--primary-color)' : '2px solid transparent',
                    marginBottom: '-2px',
                    transition: 'all 0.2s ease',
                    opacity: shouldDisableQrzTabs ? 0.5 : 1
                  }}
                >
                  Carte
                </button>
              </div>
            </div>            {/* Contenu des onglets */}
            {qso.participants && qso.participants.length > 0 ? (
              <div className="tab-content">
                {activeTab === 'details' ? (
                  canUserReorderParticipants(user, qso) ? (                    /* Affichage détaillé avec cartes et drag and drop pour le modérateur */
                    <DraggableParticipantsList
                      participants={qso.participants}
                      onReorder={handleReorderParticipants}
                      onRemove={canUserModifyQso(user, qso) ? handleRemoveParticipant : undefined}
                      showRemoveButton={canUserModifyQso(user, qso)}
                      isReordering={isReordering}
                      shouldFetchQrzInfo={canUserFetchQrzInfo(user)}
                    />) : (
                    /* Affichage des cartes sans drag and drop pour les non-modérateurs */
                    <div className="participants-grid" style={{
                      display: 'grid',
                      gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
                      gap: '1rem',
                      alignItems: 'stretch',
                      width: '100%'
                    }}>                      {qso.participants.map((participant) => (
                        <ParticipantCard
                          key={participant.callSign}
                          participant={participant}
                          onRemove={canUserModifyQso(user, qso) ? handleRemoveParticipant : undefined}
                          showRemoveButton={canUserModifyQso(user, qso)}
                          shouldFetchQrzInfo={canUserFetchQrzInfo(user)}
                        />
                      ))}
                    </div>
                  )
                ) : activeTab === 'table' ? (
                  /* Affichage en table */
                  <ParticipantTable
                    participants={qso.participants}
                    onRemove={canUserModifyQso(user, qso) ? handleRemoveParticipant : undefined}
                    showRemoveButton={canUserModifyQso(user, qso)}
                  />
                ) : (
                  /* Affichage sur carte */
                  <ParticipantMap
                    participants={qso.participants}
                  />
                )}
              </div>            ) : (
              <div className="no-participants">
                <p>Aucun participant ajouté pour ce QSO</p>
              </div>
            )}
          </div>        </div>
      </div>
      
      {/* Container pour les toasts */}
      <ToastContainer toasts={toasts} onRemoveToast={removeToast} />
    </div>
  );
};

export default QsoDetailPage;
