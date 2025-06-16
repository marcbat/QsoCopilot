import React, { useState, useEffect } from 'react';
import { QsoAggregateDto } from '../types';
import { qsoApiService } from '../api';
import { useAuth } from '../contexts/AuthContext';
import NewQsoForm from './NewQsoForm';
import QsoList from './QsoList';

const QsoManagerPage: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const [qsos, setQsos] = useState<QsoAggregateDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  // Charger la liste des QSO
  const loadQsos = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await qsoApiService.getAllQsoAggregates();
      setQsos(data);
    } catch (err) {
      console.error('Erreur lors du chargement des QSO:', err);
      setError('Erreur lors du chargement de la liste des QSO');
    } finally {
      setIsLoading(false);
    }
  };

  // Rechercher les QSO par nom
  const searchQsos = async (term: string) => {
    if (!term.trim()) {
      loadQsos();
      return;
    }

    try {
      setIsLoading(true);
      setError(null);
      const data = await qsoApiService.searchQsoByName(term);
      setQsos(data);
    } catch (err) {
      console.error('Erreur lors de la recherche:', err);
      setError('Erreur lors de la recherche');
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
    searchQsos(searchTerm);
  };
  // Gestionnaire pour la création d'un nouveau QSO
  const handleQsoCreated = (_qsoId: string) => {
    // Recharger la liste après création
    loadQsos();
  };

  return (
    <div className="qso-manager-page">      {/* Formulaire de création QSO pour utilisateurs connectés */}
      {isAuthenticated && (
        <div className="qso-creation-section">
          <NewQsoForm onQsoCreated={handleQsoCreated} />
        </div>
      )}

      {/* Barre de recherche */}
      <div className="card">
        <div className="card-header">
          <h2 className="card-title">Liste des QSO</h2>
        </div>
        
        <form onSubmit={handleSearch} className="form-row">
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
          <div className="form-group">
            <button type="submit" className="btn btn-primary" disabled={isLoading}>
              {isLoading ? 'Recherche...' : 'Rechercher'}
            </button>
            <button 
              type="button" 
              className="btn btn-secondary" 
              onClick={() => {
                setSearchTerm('');
                loadQsos();
              }}
              style={{ marginLeft: '0.5rem' }}
            >
              Effacer
            </button>
          </div>
        </form>

        {error && <div className="alert alert-error">{error}</div>}

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
