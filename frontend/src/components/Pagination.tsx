import React from 'react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  pageSize: number;
  onPageSizeChange: (size: number) => void;
  totalCount: number;
  isLoading?: boolean;
}

const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  onPageChange,
  pageSize,
  onPageSizeChange,
  totalCount,
  isLoading = false
}) => {
  const pageSizeOptions = [5, 10, 20, 50]; // Commencer par des valeurs plus petites pour les tests

  const getPageNumbers = () => {
    const delta = 2; // Nombre de pages à afficher de chaque côté de la page courante
    const pages: number[] = [];
    
    // Toujours afficher la première page
    if (totalPages > 0) {
      pages.push(1);
    }
    
    // Ajouter les pages autour de la page courante
    const start = Math.max(2, currentPage - delta);
    const end = Math.min(totalPages - 1, currentPage + delta);
    
    // Ajouter "..." si nécessaire
    if (start > 2) {
      pages.push(-1); // -1 représente "..."
    }
    
    // Ajouter les pages du milieu
    for (let i = start; i <= end; i++) {
      if (i > 1 && i < totalPages) {
        pages.push(i);
      }
    }
    
    // Ajouter "..." si nécessaire
    if (end < totalPages - 1) {
      pages.push(-2); // -2 représente "..."
    }
    
    // Toujours afficher la dernière page
    if (totalPages > 1) {
      pages.push(totalPages);
    }
    
    return pages;
  };

  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalCount);

  if (totalPages <= 1) {
    return null;
  }

  return (
    <div className="pagination-container" style={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginTop: '1.5rem',
      padding: '1rem',
      borderTop: '1px solid var(--border-color)',
      backgroundColor: 'var(--card-bg)',
      borderRadius: '0 0 var(--border-radius) var(--border-radius)'
    }}>
      {/* Informations sur les résultats */}
      <div className="pagination-info" style={{
        fontSize: '0.875rem',
        color: 'var(--text-secondary)'
      }}>
        Affichage {startItem}-{endItem} sur {totalCount} résultats
      </div>

      {/* Contrôles de pagination */}
      <div className="pagination-controls" style={{
        display: 'flex',
        alignItems: 'center',
        gap: '1rem'
      }}>
        {/* Sélecteur de taille de page */}
        <div className="page-size-selector" style={{
          display: 'flex',
          alignItems: 'center',
          gap: '0.5rem'
        }}>
          <label style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
            Par page:
          </label>
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            disabled={isLoading}
            style={{
              padding: '0.25rem 0.5rem',
              borderRadius: 'var(--border-radius)',
              border: '1px solid var(--border-color)',
              fontSize: '0.875rem'
            }}
          >
            {pageSizeOptions.map(size => (
              <option key={size} value={size}>{size}</option>
            ))}
          </select>
        </div>

        {/* Boutons de navigation */}
        <div className="pagination-buttons" style={{
          display: 'flex',
          alignItems: 'center',
          gap: '0.25rem'
        }}>
          {/* Bouton Précédent */}
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage <= 1 || isLoading}
            className="pagination-btn"
            style={{
              padding: '0.5rem 0.75rem',
              border: '1px solid var(--border-color)',
              borderRadius: 'var(--border-radius)',
              backgroundColor: 'var(--card-bg)',
              color: 'var(--text-primary)',
              cursor: currentPage <= 1 || isLoading ? 'not-allowed' : 'pointer',
              opacity: currentPage <= 1 || isLoading ? 0.5 : 1,
              fontSize: '0.875rem'
            }}
            title="Page précédente"
          >
            ← Précédent
          </button>

          {/* Numéros de page */}
          {getPageNumbers().map((page, index) => {
            if (page === -1 || page === -2) {
              return (
                <span key={`ellipsis-${index}`} style={{
                  padding: '0.5rem 0.25rem',
                  color: 'var(--text-secondary)',
                  fontSize: '0.875rem'
                }}>
                  ...
                </span>
              );
            }

            return (
              <button
                key={page}
                onClick={() => onPageChange(page)}
                disabled={isLoading}
                className={`pagination-btn ${page === currentPage ? 'active' : ''}`}
                style={{
                  padding: '0.5rem 0.75rem',
                  border: '1px solid var(--border-color)',
                  borderRadius: 'var(--border-radius)',
                  backgroundColor: page === currentPage ? 'var(--primary-color)' : 'var(--card-bg)',
                  color: page === currentPage ? 'white' : 'var(--text-primary)',
                  cursor: isLoading ? 'not-allowed' : 'pointer',
                  opacity: isLoading ? 0.5 : 1,
                  fontSize: '0.875rem',
                  fontWeight: page === currentPage ? '600' : '400'
                }}
              >
                {page}
              </button>
            );
          })}

          {/* Bouton Suivant */}
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage >= totalPages || isLoading}
            className="pagination-btn"
            style={{
              padding: '0.5rem 0.75rem',
              border: '1px solid var(--border-color)',
              borderRadius: 'var(--border-radius)',
              backgroundColor: 'var(--card-bg)',
              color: 'var(--text-primary)',
              cursor: currentPage >= totalPages || isLoading ? 'not-allowed' : 'pointer',
              opacity: currentPage >= totalPages || isLoading ? 0.5 : 1,
              fontSize: '0.875rem'
            }}
            title="Page suivante"
          >
            Suivant →
          </button>
        </div>
      </div>
    </div>
  );
};

export default Pagination;
