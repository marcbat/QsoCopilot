import React, { useEffect, useState } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { ParticipantDto, ParticipantQrzInfoDto } from '../types';
import { participantApiService } from '../api/qsoApi';

// Fix pour les icônes Leaflet par défaut
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

interface ParticipantMapProps {
  participants: ParticipantDto[];
}

interface ParticipantWithLocation extends ParticipantDto {
  qrzInfo?: ParticipantQrzInfoDto;
  latitude?: number;
  longitude?: number;
  isLoadingLocation?: boolean;
}

// Composant pour ajuster automatiquement la vue de la carte
const MapBoundsAdjuster: React.FC<{ participants: ParticipantWithLocation[] }> = ({ participants }) => {
  const map = useMap();

  useEffect(() => {
    const validParticipants = participants.filter(
      p => p.latitude !== undefined && p.longitude !== undefined && 
           !isNaN(p.latitude!) && !isNaN(p.longitude!)
    );

    if (validParticipants.length === 0) return;

    if (validParticipants.length === 1) {
      const p = validParticipants[0];
      map.setView([p.latitude!, p.longitude!], 10);
    } else {
      // Créer des bounds pour englober tous les participants
      const bounds = L.latLngBounds(
        validParticipants.map(p => [p.latitude!, p.longitude!] as [number, number])
      );
      
      // Ajuster la vue avec un padding pour un meilleur affichage
      map.fitBounds(bounds, { padding: [20, 20] });
    }
  }, [map, participants]);

  return null;
};

const ParticipantMap: React.FC<ParticipantMapProps> = ({ participants }) => {
  const [participantsWithLocation, setParticipantsWithLocation] = useState<ParticipantWithLocation[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchLocations = async () => {
      setIsLoading(true);
      
      const participantsWithLocationData = await Promise.all(
        participants.map(async (participant) => {
          try {
            const qrzInfo = await participantApiService.getParticipantQrzInfo(participant.callSign);
            
            // Extraire les coordonnées si disponibles
            let latitude: number | undefined;
            let longitude: number | undefined;
              if (qrzInfo?.qrzCallsignInfo?.lat && qrzInfo?.qrzCallsignInfo?.lon) {
              latitude = typeof qrzInfo.qrzCallsignInfo.lat === 'string' 
                ? parseFloat(qrzInfo.qrzCallsignInfo.lat)
                : qrzInfo.qrzCallsignInfo.lat;
              longitude = typeof qrzInfo.qrzCallsignInfo.lon === 'string'
                ? parseFloat(qrzInfo.qrzCallsignInfo.lon)
                : qrzInfo.qrzCallsignInfo.lon;
            }
            
            return {
              ...participant,
              qrzInfo,
              latitude,
              longitude,
              isLoadingLocation: false
            };
          } catch (error) {
            console.error(`Erreur lors de la récupération des coordonnées pour ${participant.callSign}:`, error);
            return {
              ...participant,
              isLoadingLocation: false
            };
          }
        })
      );
      
      setParticipantsWithLocation(participantsWithLocationData);
      setIsLoading(false);
    };

    if (participants.length > 0) {
      fetchLocations();
    } else {
      setIsLoading(false);
    }
  }, [participants]);

  // Filtrer les participants qui ont des coordonnées valides
  const participantsWithValidCoords = participantsWithLocation.filter(
    p => p.latitude !== undefined && p.longitude !== undefined && 
         !isNaN(p.latitude!) && !isNaN(p.longitude!)
  );
  // Calculer le centre initial (sera ajusté par MapBoundsAdjuster)
  const getInitialCenter = (): [number, number] => {
    const validParticipants = participantsWithLocation.filter(
      p => p.latitude !== undefined && p.longitude !== undefined && 
           !isNaN(p.latitude!) && !isNaN(p.longitude!)
    );

    if (validParticipants.length === 0) {
      // Position par défaut (Paris)
      return [48.8566, 2.3522];
    }

    // Centre approximatif pour démarrage
    const lats = validParticipants.map(p => p.latitude!);
    const lons = validParticipants.map(p => p.longitude!);
    
    const centerLat = lats.reduce((a, b) => a + b, 0) / lats.length;
    const centerLon = lons.reduce((a, b) => a + b, 0) / lons.length;
    
    return [centerLat, centerLon];
  };

  const initialCenter = getInitialCenter();

  if (isLoading) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '400px',
        fontSize: '1rem',
        color: 'var(--text-secondary)'
      }}>
        <div>
          <div className="spinner" style={{ margin: '0 auto 1rem' }}></div>
          Chargement des positions des participants...
        </div>
      </div>
    );
  }

  if (participantsWithValidCoords.length === 0) {
    return (
      <div style={{ 
        display: 'flex', 
        flexDirection: 'column',
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '400px',
        fontSize: '1rem',
        color: 'var(--text-secondary)',
        textAlign: 'center'
      }}>
        <div>
          <h4 style={{ marginBottom: '0.5rem' }}>Aucune position disponible</h4>
          <p>Les participants n'ont pas de coordonnées géographiques dans leur profil QRZ.</p>
        </div>
      </div>
    );
  }
  return (
    <div className="map-container">
      <div style={{ 
        marginBottom: '1rem', 
        padding: '0.75rem',
        backgroundColor: 'var(--surface-color)',
        border: '1px solid var(--border-color)',
        borderRadius: '6px',
        fontSize: '0.875rem'
      }}>
        <strong>Participants localisés :</strong> {participantsWithValidCoords.length} sur {participants.length}
      </div>
      
      <div className="map-wrapper">
        <MapContainer
          center={initialCenter}
          zoom={6}
          style={{ height: '100%', width: '100%', borderRadius: '8px' }}
          scrollWheelZoom={true}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          
          {/* Composant pour ajuster automatiquement les bounds */}
          <MapBoundsAdjuster participants={participantsWithValidCoords} />
          
          {participantsWithValidCoords.map((participant, index) => (
            <Marker
              key={index}
              position={[participant.latitude!, participant.longitude!]}
            >
            <Popup>
              <div style={{ minWidth: '200px' }}>
                <h4 style={{ 
                  margin: '0 0 0.5rem 0', 
                  color: 'var(--primary-color)',
                  fontSize: '1.1rem'
                }}>
                  {participant.callSign}
                </h4>
                
                {participant.qrzInfo?.qrzCallsignInfo?.fName && (
                  <p style={{ margin: '0.25rem 0' }}>
                    <strong>Nom :</strong> {participant.qrzInfo.qrzCallsignInfo.fName}
                  </p>
                )}
                
                {participant.qrzInfo?.qrzCallsignInfo?.country && (
                  <p style={{ margin: '0.25rem 0' }}>
                    <strong>Pays :</strong> {participant.qrzInfo.qrzCallsignInfo.country}
                  </p>
                )}
                
                {participant.qrzInfo?.qrzCallsignInfo?.state && (
                  <p style={{ margin: '0.25rem 0' }}>
                    <strong>État/Région :</strong> {participant.qrzInfo.qrzCallsignInfo.state}
                  </p>
                )}
                
                {participant.qrzInfo?.qrzCallsignInfo?.grid && (
                  <p style={{ margin: '0.25rem 0' }}>
                    <strong>Grille :</strong> {participant.qrzInfo.qrzCallsignInfo.grid}
                  </p>                )}
                
                <p style={{ 
                  margin: '0.5rem 0 0 0', 
                  fontSize: '0.8rem', 
                  color: 'var(--text-secondary)' 
                }}>
                  📍 {participant.latitude!.toFixed(4)}, {participant.longitude!.toFixed(4)}
                </p>
              </div>
            </Popup>
          </Marker>
        ))}
        </MapContainer>
      </div>
    </div>
  );
};

export default ParticipantMap;
