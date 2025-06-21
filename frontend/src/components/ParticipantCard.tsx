import React, { useState, useEffect } from 'react';
import { ParticipantDto, ParticipantQrzInfoDto } from '../types';
import { participantApiService } from '../api/qsoApi';

interface ParticipantCardProps {
  participant: ParticipantDto;
  onRemove?: (callSign: string) => void;
  showRemoveButton?: boolean;
}

const ParticipantCard: React.FC<ParticipantCardProps> = ({ 
  participant, 
  onRemove, 
  showRemoveButton = false 
}) => {
  const [qrzInfo, setQrzInfo] = useState<ParticipantQrzInfoDto | null>(null);
  const [isLoadingQrz, setIsLoadingQrz] = useState(false);
  const [qrzError, setQrzError] = useState<string | null>(null);

  useEffect(() => {
    const fetchQrzInfo = async () => {
      try {
        setIsLoadingQrz(true);
        setQrzError(null);
        const info = await participantApiService.getParticipantQrzInfo(participant.callSign);
        setQrzInfo(info);
      } catch (error) {
        console.error(`Erreur lors de la récupération des informations QRZ pour ${participant.callSign}:`, error);
        setQrzError('Impossible de récupérer les informations QRZ');
      } finally {
        setIsLoadingQrz(false);
      }
    };

    fetchQrzInfo();
  }, [participant.callSign]);

  const getDisplayName = () => {
    if (qrzInfo?.qrzCallsignInfo?.fName) {
      return qrzInfo.qrzCallsignInfo.fName;
    }
    if (participant.name) {
      return participant.name;
    }
    return null;
  };

  return (
    <div className="participant-card" style={{ position: 'relative' }}>
      {showRemoveButton && onRemove && (
        <button
          className="remove-participant-btn"
          onClick={() => onRemove(participant.callSign)}
          title={`Supprimer ${participant.callSign}`}
          style={{
            position: 'absolute',
            top: '8px',
            right: '8px',
            background: 'transparent',
            color: 'var(--text-secondary)',
            border: 'none',
            width: '20px',
            height: '20px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            cursor: 'pointer',
            fontSize: '14px',
            transition: 'all 0.2s ease',
            opacity: '0.7'
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.color = '#ef4444';
            e.currentTarget.style.opacity = '1';
            e.currentTarget.style.transform = 'scale(1.2)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.color = 'var(--text-secondary)';
            e.currentTarget.style.opacity = '0.7';
            e.currentTarget.style.transform = 'scale(1)';
          }}
        >
          🗑️
        </button>
      )}
      
      <div className="participant-info">
        <div style={{ marginBottom: '8px' }}>
          <h4 style={{ margin: 0, marginBottom: '4px' }}>{participant.callSign}</h4>
          {isLoadingQrz ? (
            <div style={{ 
              fontSize: '0.875rem', 
              color: 'var(--text-secondary)',
              fontStyle: 'italic'
            }}>
              Chargement des informations...
            </div>
          ) : qrzError ? (
            <div style={{ 
              fontSize: '0.875rem', 
              color: 'var(--text-secondary)',
              fontStyle: 'italic'
            }}>
              {getDisplayName() || 'Informations non disponibles'}
            </div>
          ) : (
            <div style={{ 
              fontSize: '0.875rem', 
              color: 'var(--text-secondary)',
              fontWeight: '500'
            }}>
              {getDisplayName() || 'Nom non disponible'}
            </div>
          )}
        </div>

        <div className="participant-details">
          {participant.location && (
            <p><strong>Localisation :</strong> {participant.location}</p>
          )}
          {participant.signalReport && (
            <p><strong>Rapport signal :</strong> {participant.signalReport}</p>
          )}
          {participant.notes && (
            <p><strong>Notes :</strong> {participant.notes}</p>
          )}
          
          {/* Informations QRZ supplémentaires */}
          {qrzInfo?.qrzCallsignInfo && (
            <>
              {qrzInfo.qrzCallsignInfo.country && (
                <p><strong>Pays :</strong> {qrzInfo.qrzCallsignInfo.country}</p>
              )}
              {qrzInfo.qrzCallsignInfo.grid && (
                <p><strong>Grille :</strong> {qrzInfo.qrzCallsignInfo.grid}</p>
              )}
              {qrzInfo.qrzCallsignInfo.state && (
                <p><strong>État/Région :</strong> {qrzInfo.qrzCallsignInfo.state}</p>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default ParticipantCard;
