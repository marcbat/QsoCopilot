import React, { useState, useEffect } from 'react';
import { ParticipantDto, ParticipantQrzInfoDto } from '../types';
import { participantApiService } from '../api/qsoApi';

interface ParticipantTableProps {
  participants: ParticipantDto[];
  onRemove?: (callSign: string) => void;
  showRemoveButton?: boolean;
}

interface ParticipantWithQrz extends ParticipantDto {
  qrzInfo?: ParticipantQrzInfoDto;
  isLoadingQrz?: boolean;
}

const ParticipantTable: React.FC<ParticipantTableProps> = ({ 
  participants, 
  onRemove, 
  showRemoveButton = false 
}) => {
  const [participantsWithQrz, setParticipantsWithQrz] = useState<ParticipantWithQrz[]>(
    participants.map(p => ({ ...p, isLoadingQrz: true }))
  );

  useEffect(() => {
    const fetchQrzInfo = async () => {
      const updatedParticipants = await Promise.all(
        participants.map(async (participant) => {
          try {
            const qrzInfo = await participantApiService.getParticipantQrzInfo(participant.callSign);
            return { 
              ...participant, 
              qrzInfo, 
              isLoadingQrz: false 
            };
          } catch (error) {
            console.error(`Erreur QRZ pour ${participant.callSign}:`, error);
            return { 
              ...participant, 
              isLoadingQrz: false 
            };
          }
        })
      );
      setParticipantsWithQrz(updatedParticipants);
    };

    fetchQrzInfo();
  }, [participants]);

  const getDisplayName = (participant: ParticipantWithQrz) => {
    if (participant.qrzInfo?.qrzCallsignInfo?.fName) {
      return participant.qrzInfo.qrzCallsignInfo.fName;
    }
    if (participant.name) {
      return participant.name;
    }
    return '';
  };

  const getCountry = (participant: ParticipantWithQrz) => {
    return participant.qrzInfo?.qrzCallsignInfo?.country || '';
  };

  const getGrid = (participant: ParticipantWithQrz) => {
    return participant.qrzInfo?.qrzCallsignInfo?.grid || '';
  };
  const getState = (participant: ParticipantWithQrz) => {
    return participant.qrzInfo?.qrzCallsignInfo?.state || '';
  };
  const getEmail = (participant: ParticipantWithQrz) => {
    return participant.qrzInfo?.qrzCallsignInfo?.email || '';
  };

  const getAddr2 = (participant: ParticipantWithQrz) => {
    return participant.qrzInfo?.qrzCallsignInfo?.addr2 || '';
  };

  return (
    <div style={{ 
      overflowX: 'auto',
      width: '100%',
      marginTop: '1rem'
    }}>
      <table style={{ 
        width: '100%', 
        minWidth: '100%',
        borderCollapse: 'collapse',
        backgroundColor: 'var(--surface-color)',
        borderRadius: '8px',
        overflow: 'hidden',
        boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)'
      }}>
        <thead>
          <tr style={{ 
            backgroundColor: 'var(--primary-color)', 
            color: 'white'
          }}>
            <th style={{ 
              padding: '12px', 
              textAlign: 'left', 
              fontWeight: '600',
              fontSize: '0.875rem',
              width: '12%'
            }}>
              Indicatif
            </th>            <th style={{ 
              padding: '12px', 
              textAlign: 'left', 
              fontWeight: '600',
              fontSize: '0.875rem',
              width: '12%'
            }}>
              Prénom
            </th>
            <th style={{ 
              padding: '12px', 
              textAlign: 'left', 
              fontWeight: '600',
              fontSize: '0.875rem',
              width: '15%'
            }}>
              Email
            </th>
            <th style={{ 
              padding: '12px', 
              textAlign: 'left', 
              fontWeight: '600',
              fontSize: '0.875rem',
              width: '12%'
            }}>
              Pays
            </th>
            <th style={{ 
              padding: '12px', 
              textAlign: 'left', 
              fontWeight: '600',
              fontSize: '0.875rem',
              width: '10%'
            }}>
              Grille
            </th>
            <th style={{ 
              padding: '12px',              textAlign: 'left', 
              fontWeight: '600',
              fontSize: '0.875rem',
              width: '12%'
            }}>
              État/Région
            </th>            <th style={{ 
              padding: '12px', 
              textAlign: 'left', 
              fontWeight: '600',
              fontSize: '0.875rem',
              width: '17%'
            }}>
              Ville
            </th>
            {showRemoveButton && (
              <th style={{ 
                padding: '12px', 
                textAlign: 'center', 
                fontWeight: '600',
                fontSize: '0.875rem',
                width: '60px'
              }}>
                Action
              </th>
            )}
          </tr>
        </thead>
        <tbody>
          {participantsWithQrz.map((participant, index) => (
            <tr 
              key={index}
              style={{ 
                borderBottom: index < participantsWithQrz.length - 1 ? '1px solid var(--border-color)' : 'none',
                transition: 'background-color 0.2s ease'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.backgroundColor = 'var(--hover-color)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.backgroundColor = 'transparent';
              }}
            >              <td style={{ 
                padding: '12px', 
                fontWeight: '600',
                color: 'var(--primary-color)'
              }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <span>{participant.callSign}</span>
                  <a
                    href={`https://www.qrz.com/db/${participant.callSign}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    title={`Voir ${participant.callSign} sur QRZ.com`}
                    style={{
                      textDecoration: 'none',
                      display: 'inline-flex',
                      alignItems: 'center',
                      padding: '1px 4px',
                      borderRadius: '3px',
                      fontSize: '0.7rem',
                      fontWeight: '500',
                      backgroundColor: 'var(--primary-color)',
                      color: 'white',
                      transition: 'all 0.2s ease',
                      opacity: 0.8
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.opacity = '1';
                      e.currentTarget.style.transform = 'scale(1.05)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.opacity = '0.8';
                      e.currentTarget.style.transform = 'scale(1)';
                    }}
                  >
                    QRZ
                  </a>
                </div>
              </td>
              <td style={{ padding: '12px' }}>
                {participant.isLoadingQrz ? (
                  <span style={{ 
                    fontStyle: 'italic', 
                    color: 'var(--text-secondary)',
                    fontSize: '0.875rem'
                  }}>
                    Chargement...
                  </span>
                ) : (
                  getDisplayName(participant) || (
                    <span style={{ 
                      fontStyle: 'italic', 
                      color: 'var(--text-secondary)' 
                    }}>
                      -
                    </span>
                  )
                )}
              </td>              <td style={{ padding: '12px' }}>
                {participant.isLoadingQrz ? (
                  <span style={{ 
                    fontStyle: 'italic', 
                    color: 'var(--text-secondary)',
                    fontSize: '0.875rem'
                  }}>
                    ...
                  </span>
                ) : (
                  getEmail(participant) || (
                    <span style={{ 
                      fontStyle: 'italic', 
                      color: 'var(--text-secondary)' 
                    }}>
                      -
                    </span>
                  )
                )}
              </td>
              <td style={{ padding: '12px' }}>
                {participant.isLoadingQrz ? (
                  <span style={{ 
                    fontStyle: 'italic', 
                    color: 'var(--text-secondary)',
                    fontSize: '0.875rem'
                  }}>
                    ...
                  </span>
                ) : (
                  getCountry(participant) || (
                    <span style={{ 
                      fontStyle: 'italic', 
                      color: 'var(--text-secondary)' 
                    }}>
                      -
                    </span>
                  )
                )}
              </td>
              <td style={{ padding: '12px' }}>
                {participant.isLoadingQrz ? (
                  <span style={{ 
                    fontStyle: 'italic', 
                    color: 'var(--text-secondary)',
                    fontSize: '0.875rem'
                  }}>
                    ...
                  </span>
                ) : (
                  getGrid(participant) || (
                    <span style={{ 
                      fontStyle: 'italic', 
                      color: 'var(--text-secondary)' 
                    }}>
                      -
                    </span>
                  )
                )}
              </td>
              <td style={{ padding: '12px' }}>
                {participant.isLoadingQrz ? (
                  <span style={{ 
                    fontStyle: 'italic', 
                    color: 'var(--text-secondary)',
                    fontSize: '0.875rem'
                  }}>
                    ...
                  </span>
                ) : (
                  getState(participant) || (                    <span style={{ 
                      fontStyle: 'italic', 
                      color: 'var(--text-secondary)' 
                    }}>
                      -
                    </span>
                  )
                )}
              </td>
              <td style={{ padding: '12px' }}>
                {participant.isLoadingQrz ? (
                  <span style={{ 
                    fontStyle: 'italic', 
                    color: 'var(--text-secondary)',
                    fontSize: '0.875rem'
                  }}>
                    ...
                  </span>
                ) : (
                  getAddr2(participant) ? (
                    <span title={getAddr2(participant)} style={{ 
                      maxWidth: '150px',
                      display: 'inline-block',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap'
                    }}>
                      {getAddr2(participant)}
                    </span>
                  ) : (
                    <span style={{ 
                      fontStyle: 'italic', 
                      color: 'var(--text-secondary)' 
                    }}>
                      -
                    </span>
                  )
                )}
              </td>
              {showRemoveButton && onRemove && (
                <td style={{ padding: '12px', textAlign: 'center' }}>
                  <button
                    onClick={() => onRemove(participant.callSign)}
                    title={`Supprimer ${participant.callSign}`}
                    style={{
                      background: 'transparent',
                      color: 'var(--text-secondary)',
                      border: 'none',
                      width: '32px',
                      height: '32px',
                      borderRadius: '6px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      cursor: 'pointer',
                      fontSize: '16px',
                      transition: 'all 0.2s ease',
                      opacity: '0.7'
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.color = '#ef4444';
                      e.currentTarget.style.backgroundColor = 'rgba(239, 68, 68, 0.1)';
                      e.currentTarget.style.opacity = '1';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.color = 'var(--text-secondary)';
                      e.currentTarget.style.backgroundColor = 'transparent';
                      e.currentTarget.style.opacity = '0.7';
                    }}
                  >
                    🗑️
                  </button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ParticipantTable;
