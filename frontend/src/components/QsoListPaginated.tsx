import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { QsoAggregateDto, PagedResult } from '../types';
import { useAuth } from '../contexts/AuthContext';
import { qsoApiService } from '../api/qsoApi';
import { extractErrorMessage } from '../utils/errorUtils';
import Pagination from './Pagination';

interface QsoListPaginatedProps {
  pagedResult: PagedResult<QsoAggregateDto> | null;
  isLoading: boolean;
  onRefresh: () => void;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

const QsoListPaginated: React.FC<QsoListPaginatedProps> = ({ 
  pagedResult, 
  isLoading, 
  onRefresh, 
  onPageChange, 
  onPageSizeChange 
}) => {
  const { isAuthenticated, user } = useAuth();
  const navigate = useNavigate();
  const [deletingQsoId, setDeletingQsoId] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  // Fonction pour vérifier si l'utilisateur actuel peut supprimer un QSO
  const canDeleteQso = (qso: QsoAggregateDto): boolean => {
    return isAuthenticated && user !== null && user.id === qso.moderatorId;
  };

  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
        Chargement des QSO...
      </div>
    );
  }

  if (!pagedResult || pagedResult.items.length === 0) {
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

  const handleEdit = (qsoId: string) => {
    navigate(`/qso/${qsoId}/edit`);
  };

  const handleDeleteQso = async (qso: QsoAggregateDto) => {
    const confirmMessage = `Êtes-vous sûr de vouloir supprimer le QSO "${qso.name}" ?
Cette action est irréversible.`;
    
    if (!window.confirm(confirmMessage)) {
      return;
    }

    try {
      setDeletingQsoId(qso.id);
      setDeleteError(null);
      
      await qsoApiService.deleteQso(qso.id);
      onRefresh(); // Recharger les données après suppression
      
    } catch (error: any) {
      console.error('Erreur lors de la suppression du QSO:', error);
      setDeleteError(extractErrorMessage(error, 'Impossible de supprimer le QSO'));
    } finally {
      setDeletingQsoId(null);
    }
  };
  return (
    <div className="qso-list-paginated">
      <div className="qso-list-header">
        <span>
          {pagedResult.totalCount} QSO(s) trouvé(s) 
          {pagedResult.totalPages > 1 && (
            <span style={{ color: 'var(--text-secondary)', marginLeft: '0.5rem' }}>
              (Page {pagedResult.pageNumber} sur {pagedResult.totalPages})
            </span>
          )}
        </span>
        
        <div className="qso-list-controls">
          <div className="page-size-selector-header">
            <label>Par page :</label>
            <select
              value={pagedResult.pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
              disabled={isLoading}
            >
              <option value={5}>5</option>
              <option value={10}>10</option>
              <option value={20}>20</option>
            </select>
          </div>
          
          <button 
            onClick={onRefresh} 
            className="qso-list-refresh-btn" 
            disabled={isLoading}
          >
            {isLoading ? 'Actualisation...' : 'Actualiser'}
          </button>
        </div>
      </div>

      {deleteError && (
        <div className="alert alert-error" style={{ marginBottom: '1rem' }}>
          {deleteError}
        </div>
      )}

      <div className="card">
        <table className="table">          <thead>
            <tr>
              <th>Nom</th>
              <th>Fréquence</th>
              <th>Participants</th>
              <th>Date de début</th>
              {isAuthenticated && <th>Actions</th>}
            </tr>
          </thead>
          <tbody>
            {pagedResult.items.map((qso) => (
              <tr key={qso.id}>
                <td>
                  <div 
                    className="qso-name" 
                    style={{ 
                      fontWeight: '600', 
                      color: 'var(--primary-color)', 
                      cursor: 'pointer',
                      textDecoration: 'none'
                    }}
                    onClick={() => handleViewDetails(qso.id)}
                    title="Cliquez pour voir les détails"
                  >
                    {qso.name}
                  </div>                </td>
                <td>
                  <div style={{ fontSize: '0.875rem', fontWeight: '500' }}>
                    {qso.frequency ? `${qso.frequency.toFixed(3)} MHz` : 'Non définie'}
                  </div>
                </td>
                <td>
                  <div className="participants-count">
                    {qso.participants ? (
                      <span>
                        {qso.participants.length} participant(s)
                        {qso.participants.length > 0 && (
                          <div style={{ 
                            fontSize: '0.75rem', 
                            color: 'var(--text-secondary)', 
                            marginTop: '0.25rem' 
                          }}>
                            {qso.participants.slice(0, 3).map(p => p.callSign).join(', ')}
                            {qso.participants.length > 3 && '...'}
                          </div>
                        )}
                      </span>
                    ) : (
                      <span style={{ color: 'var(--text-secondary)' }}>0 participant</span>
                    )}
                  </div>
                </td>
                <td>
                  <div style={{ fontSize: '0.875rem' }}>
                    {formatDate(qso.startDateTime)}
                  </div>
                </td>
                {isAuthenticated && (
                  <td>
                    <div className="table-actions" style={{ 
                      display: 'flex', 
                      gap: '0.5rem', 
                      alignItems: 'center' 
                    }}>
                      <button 
                        className="btn btn-sm btn-primary"
                        onClick={() => handleViewDetails(qso.id)}
                        title="Voir les détails de ce QSO"
                      >
                        Détails
                      </button>
                      {canDeleteQso(qso) && (
                        <>
                          <button 
                            className="btn btn-sm btn-secondary"
                            onClick={() => handleEdit(qso.id)}
                            title="Éditer ce QSO"
                          >
                            Éditer
                          </button>
                          <button 
                            className="btn btn-sm btn-danger"
                            onClick={() => handleDeleteQso(qso)}
                            disabled={deletingQsoId === qso.id}
                            title="Supprimer ce QSO"
                          >
                            {deletingQsoId === qso.id ? 'Suppression...' : 'Supprimer'}
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>        {/* Composant de pagination */}
        <Pagination
          currentPage={pagedResult.pageNumber}
          totalPages={pagedResult.totalPages}
          onPageChange={onPageChange}
          totalCount={pagedResult.totalCount}
          pageSize={pagedResult.pageSize}
          isLoading={isLoading}
        />
      </div>
    </div>
  );
};

export default QsoListPaginated;
