import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { QsoAggregateDto } from '../types';
import { useAuth } from '../contexts/AuthContext';
import { qsoApiService } from '../api/qsoApi';
import { extractErrorMessage } from '../utils/errorUtils';

interface QsoListProps {
  qsos: QsoAggregateDto[];
  isLoading: boolean;
  onRefresh: () => void;
}

const QsoList: React.FC<QsoListProps> = ({ qsos, isLoading, onRefresh }) => {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [deletingQsoId, setDeletingQsoId] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
        Chargement des QSO...
      </div>
    );
  }

  if (qsos.length === 0) {
    return (
      <div className="loading">
        <p>Aucun QSO trouvé.</p>
        <button onClick={onRefresh} className="btn btn-secondary" style={{ marginTop: '1rem' }}>
          Actualiser
        </button>
      </div>
    );
  }

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Non définie';
    return new Date(dateString).toLocaleDateString('fr-FR', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const handleViewDetails = (qsoId: string) => {
    navigate(`/qso/${qsoId}`);
  };

  const handleEditQso = (qsoId: string) => {
    navigate(`/qso/${qsoId}/edit`);
  };

  const handleDeleteQso = async (qso: QsoAggregateDto) => {
    // Confirmation avant suppression
    const confirmMessage = `Êtes-vous sûr de vouloir supprimer le QSO "${qso.name}" ?
    
Cette action est irréversible et supprimera également tous les participants associés.`;
    
    if (!confirm(confirmMessage)) {
      return;
    }

    setDeletingQsoId(qso.id);
    setDeleteError(null);

    try {
      await qsoApiService.deleteQso(qso.id);
      // Recharger la liste après suppression
      onRefresh();
    } catch (error: any) {
      console.error('Erreur lors de la suppression du QSO:', error);
      setDeleteError(extractErrorMessage(error, 'Impossible de supprimer le QSO'));
    } finally {
      setDeletingQsoId(null);
    }
  };

  return (
    <div className="qso-list">
      <div className="table-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <span>Total: {qsos.length} QSO(s)</span>
        <button onClick={onRefresh} className="btn btn-secondary btn-sm">
          Actualiser
        </button>
      </div>

      {deleteError && (
        <div className="alert alert-danger" role="alert" style={{ marginBottom: '1rem' }}>
          {deleteError}
        </div>
      )}

      <table className="table">
        <thead>          <tr>
            <th>Nom</th>
            <th>Description</th>
            <th>Fréquence</th>
            <th>Participants</th>
            <th>Date de début</th>
            {isAuthenticated && <th>Actions</th>}
          </tr>
        </thead>
        <tbody>
          {qsos.map((qso) => (
            <tr key={qso.id}>
              <td>
                <div className="qso-name" style={{ fontWeight: '600', color: 'var(--primary-color)' }}>
                  {qso.name}
                </div>
              </td>              <td>
                <div className="qso-description" style={{ maxWidth: '300px', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {qso.description || 'Aucune description'}
                </div>
              </td>
              <td>
                <div style={{ fontSize: '0.875rem', fontWeight: '500' }}>
                  {qso.frequency ? `${qso.frequency} MHz` : 'Non définie'}
                </div>
              </td>
              <td>
                <div className="participants-count">
                  {qso.participants ? (
                    <span>
                      {qso.participants.length} participant(s)
                      {qso.participants.length > 0 && (
                        <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                          {qso.participants.slice(0, 3).map(p => p.callSign).join(', ')}
                          {qso.participants.length > 3 && '...'}
                        </div>
                      )}
                    </span>
                  ) : (
                    <span style={{ color: 'var(--text-secondary)' }}>0 participant</span>
                  )}
                </div>
              </td>              <td>
                <div style={{ fontSize: '0.875rem' }}>
                  {formatDate(qso.startDateTime)}
                </div>
              </td>
              {isAuthenticated && (
                <td>
                  <div className="table-actions">
                    <button 
                      className="btn btn-sm btn-primary"
                      onClick={() => handleViewDetails(qso.id)}
                    >
                      Détails
                    </button>
                    <button 
                      className="btn btn-sm btn-secondary"
                      onClick={() => handleEditQso(qso.id)}
                    >
                      Éditer
                    </button>
                    <button 
                      className="btn btn-sm btn-danger"
                      onClick={() => handleDeleteQso(qso)}
                      disabled={deletingQsoId === qso.id}
                    >
                      {deletingQsoId === qso.id ? 'Suppression...' : 'Supprimer'}
                    </button>
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default QsoList;
