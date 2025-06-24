import React, { useState, useEffect } from 'react';
import { QsoAggregateDto, PagedResult } from '../types';
import { qsoApiService } from '../api/qsoApi';
import { useAuth } from '../contexts/AuthContext';
import { useToasts } from '../hooks/useToasts';
import CreateQsoForm from './CreateQsoForm';
import QsoListPaginated from './QsoListPaginated';
import ToastContainer from './ToastContainer';
import { extractErrorMessage } from '../utils/errorUtils';

const QsoManagerPagePaginated: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const [pagedResult, setPagedResult] = useState<PagedResult<QsoAggregateDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');  const [showMyModeratedOnly, setShowMyModeratedOnly] = useState(false);  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5); // Taille de page plus petite pour tester
  
  // Utiliser le hook de toasts pour toutes les notifications
  const { toasts, removeToast, showError } = useToasts();

  // Charger la liste des QSO avec pagination
  const loadQsos = async (
    page: number = currentPage,
    size: number = pageSize,
    term: string = searchTerm,
    moderatedOnly: boolean = showMyModeratedOnly
  ) => {    try {
      setIsLoading(true);
      
      let data: PagedResult<QsoAggregateDto>;
      
      if (moderatedOnly && isAuthenticated) {
        // Récupérer seulement les QSO modérés par l'utilisateur courant avec pagination
        data = await qsoApiService.getMyModeratedQsosPaginated(page, size);
        
        // Filtrer côté client par nom si un terme de recherche est fourni
        if (term.trim()) {
          const filteredItems = data.items.filter((qso: QsoAggregateDto) => 
            qso.name.toLowerCase().includes(term.toLowerCase())
          );
          // Recalculer les métadonnées de pagination pour les résultats filtrés
          const filteredTotal = filteredItems.length;
          data = {
            ...data,
            items: filteredItems,
            totalCount: filteredTotal,
            totalPages: Math.ceil(filteredTotal / size)
          };
        }
      } else if (term.trim()) {
        // Recherche par nom avec pagination
        data = await qsoApiService.searchQsoByNamePaginated(term, page, size);
      } else {
        // Récupérer tous les QSO avec pagination
        data = await qsoApiService.getAllQsosPaginated(page, size);
      }
        setPagedResult(data);
    } catch (err: any) {
      console.error('Erreur lors du chargement des QSO:', err);
      showError(extractErrorMessage(err, 'Erreur lors du chargement des QSO'));
      setPagedResult(null);
    } finally {
      setIsLoading(false);
    }
  };

  // Gestionnaire pour le changement de page
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    loadQsos(page, pageSize, searchTerm, showMyModeratedOnly);
  };

  // Gestionnaire pour le changement de taille de page
  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1); // Retourner à la première page
    loadQsos(1, size, searchTerm, showMyModeratedOnly);
  };

  // Recharger les données (garde la page actuelle)
  const handleRefresh = () => {
    loadQsos();
  };

  // Gestionnaire pour la recherche
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setCurrentPage(1); // Retourner à la première page pour une nouvelle recherche
    loadQsos(1, pageSize, searchTerm, showMyModeratedOnly);
  };

  // Gestionnaire pour le changement du filtre "Mes QSO modérés"
  const handleModeratedFilterChange = (checked: boolean) => {
    setShowMyModeratedOnly(checked);
    setCurrentPage(1); // Retourner à la première page
    loadQsos(1, pageSize, searchTerm, checked);
  };

  // Gestionnaire pour la création d'un nouveau QSO
  const handleQsoCreated = (_qsoId: string) => {
    // Recharger la liste après création (garde la page actuelle ou retourne à la première)
    handleRefresh();
  };

  // Charger les QSO au montage du composant
  useEffect(() => {
    loadQsos();
  }, []); // Dépendances vides, les paramètres sont passés explicitement

  return (
    <div className="qso-manager-page">
      {/* Formulaire de création QSO pour utilisateurs connectés */}
      {isAuthenticated && (
        <div className="qso-creation-section" style={{ marginBottom: '2rem' }}>
          <CreateQsoForm onQsoCreated={handleQsoCreated} />
        </div>
      )}

      {/* Barre de recherche et filtres */}
      <div className="card">
        <div className="card-header">
          <h2 className="card-title">Liste des QSO</h2>
        </div>
        
        <form onSubmit={handleSearch} style={{ 
          display: 'flex', 
          flexDirection: 'column', 
          gap: '0.5rem',
          padding: '1rem'
        }}>
          <div className="form-group">
            <label htmlFor="search" className="form-label">
              Rechercher par nom
            </label>
            <input
              type="text"
              id="search"
              className="form-input"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Entrez le nom d'un QSO..."
            />
          </div>

          {/* Filtre pour les QSO modérés (uniquement pour les utilisateurs connectés) */}
          {isAuthenticated && (
            <div className="form-group">
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={showMyModeratedOnly}
                  onChange={(e) => handleModeratedFilterChange(e.target.checked)}
                  className="checkbox-input"
                />
                <span className="checkbox-text">Afficher uniquement mes QSO</span>
              </label>
            </div>
          )}

          <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem' }}>
            <button type="submit" className="btn btn-primary" disabled={isLoading}>
              {isLoading ? 'Recherche...' : 'Rechercher'}
            </button>
            
            <button 
              type="button" 
              className="btn btn-secondary" 
              onClick={() => {
                setSearchTerm('');
                setShowMyModeratedOnly(false);
                setCurrentPage(1);
                loadQsos(1, pageSize, '', false);
              }}
              disabled={isLoading}
            >
              Effacer
            </button>
          </div>        </form>

        {/* Liste des QSO avec pagination */}
        <QsoListPaginated 
          pagedResult={pagedResult}
          isLoading={isLoading} 
          onRefresh={handleRefresh}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />
      </div>
      
      {/* Container pour les toasts */}
      <ToastContainer toasts={toasts} onRemoveToast={removeToast} />
    </div>
  );
};

export default QsoManagerPagePaginated;
