import React, { useState, useEffect } from 'react';
import { QsoAggregateDto } from '../types';
import { qsoApiService } from '../api';
import { useAuth } from '../contexts/AuthContext';
import { useMessages } from '../hooks/useMessages';
import CreateQsoForm from './CreateQsoForm';
import QsoList from './QsoList';
import { extractErrorMessage } from '../utils/errorUtils';

const QsoManagerPage: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const [qsos, setQsos] = useState<QsoAggregateDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showMyModeratedOnly, setShowMyModeratedOnly] = useState(false);
  
  // Utiliser le hook de messages avec auto-hide
  const { errorMessage, setErrorMessage } = useMessages();  // Charger la liste des QSO
  const loadQsos = async () => {
    // Utiliser la fonction de recherche avec les paramètres actuels
    searchQsos(searchTerm, showMyModeratedOnly);
  };// Rechercher les QSO par nom et/ou par modérateur
  const searchQsos = async (term: string, moderatedOnly: boolean = showMyModeratedOnly) => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
      
      let data: QsoAggregateDto[];
      
      if (moderatedOnly && isAuthenticated) {
        // Récupérer seulement les QSO modérés par l'utilisateur courant
        const moderatedQsos = await qsoApiService.getMyModeratedQsos();
          if (term.trim()) {
          // Filtrer côté client par nom si un terme de recherche est fourni
          data = moderatedQsos.filter((qso: QsoAggregateDto) => 
            qso.name.toLowerCase().includes(term.toLowerCase())
          );
        } else {
          data = moderatedQsos;
        }
      } else if (term.trim()) {
        // Recherche par nom uniquement
        data = await qsoApiService.searchQsoByName(term);
      } else {
        // Récupérer tous les QSO
        data = await qsoApiService.getAllQsoAggregates();
      }
      
      // Trier les résultats par date (plus récents en premier)
      const sortedQsos = data.sort((a, b) => {
        const dateA = new Date(a.startDateTime || a.createdDate || '');
        const dateB = new Date(b.startDateTime || b.createdDate || '');
        return dateB.getTime() - dateA.getTime(); // Tri décroissant (plus récents en premier)
      });
      setQsos(sortedQsos);
    } catch (err: any) {
      console.error('Erreur lors de la recherche:', err);
      setErrorMessage(extractErrorMessage(err, 'Erreur lors de la recherche'));
    } finally {
      setIsLoading(false);
    }
  };

  // Charger les QSO au montage du composant
  useEffect(() => {
    loadQsos();
  }, []);
  // Gestionnaire pour la recherche
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    searchQsos(searchTerm, showMyModeratedOnly);
  };

  // Gestionnaire pour le changement du filtre "Mes QSO modérés"
  const handleModeratedFilterChange = (checked: boolean) => {
    setShowMyModeratedOnly(checked);
    // Relancer la recherche immédiatement avec le nouveau filtre
    searchQsos(searchTerm, checked);
  };
  // Gestionnaire pour la création d'un nouveau QSO
  const handleQsoCreated = (_qsoId: string) => {
    // Recharger la liste après création
    loadQsos();
  };

  return (
    <div className="qso-manager-page">      {/* Formulaire de création QSO pour utilisateurs connectés */}
      {isAuthenticated && (
        <div className="qso-creation-section" style={{ marginBottom: '2rem' }}>
          <CreateQsoForm onQsoCreated={handleQsoCreated} />
        </div>
      )}

      {/* Barre de recherche */}
      <div className="card">
        <div className="card-header">
          <h2 className="card-title">Liste des QSO</h2>
        </div>        <form onSubmit={handleSearch} style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
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
          </div>          {/* Filtre pour les QSO modérés (uniquement pour les utilisateurs connectés) */}
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
          )}<div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem' }}>
            <button type="submit" className="btn btn-primary" disabled={isLoading}>
              {isLoading ? 'Recherche...' : 'Rechercher'}
            </button>            <button 
              type="button" 
              className="btn btn-secondary" 
              onClick={() => {
                setSearchTerm('');
                setShowMyModeratedOnly(false);
                searchQsos('', false);
              }}
            >
              Effacer
            </button>
          </div>
        </form>

        {errorMessage && <div className="alert alert-error">{errorMessage}</div>}

        {/* Liste des QSO */}
        <QsoList 
          qsos={qsos} 
          isLoading={isLoading} 
          onRefresh={loadQsos}
        />
      </div>
    </div>
  );
};

export default QsoManagerPage;
