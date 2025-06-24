import { useEffect, useRef } from 'react';
import { qsoSignalRService, QsoSignalREvents } from '../services/qsoSignalRService';

export const useQsoSignalR = (qsoId: string | null, events: Partial<QsoSignalREvents>) => {
    const currentQsoIdRef = useRef<string | null>(null);
    const eventsRef = useRef(events);

    // Mettre à jour la référence des événements quand ils changent
    useEffect(() => {
        eventsRef.current = events;
        qsoSignalRService.setEventHandlers(events);
    }, [events]);

    useEffect(() => {
        // Démarrer la connexion SignalR (pas besoin d'être connecté)
        const startConnection = async () => {
            try {
                await qsoSignalRService.start();
            } catch (error) {
                console.error('Erreur lors du démarrage de SignalR:', error);
            }
        };

        startConnection();

        // Nettoyer lors du démontage
        return () => {
            if (currentQsoIdRef.current) {
                qsoSignalRService.leaveQsoGroup(currentQsoIdRef.current);
            }
        };
    }, []); // Retiré la dépendance isAuthenticated

    useEffect(() => {
        if (!qsoId) {
            return;
        }

        const joinGroup = async () => {
            if (currentQsoIdRef.current !== qsoId) {
                try {
                    await qsoSignalRService.joinQsoGroup(qsoId);
                    currentQsoIdRef.current = qsoId;
                } catch (error) {
                    console.error('Erreur lors de l\'ajout au groupe QSO:', error);
                }
            }
        };

        // Attendre un court délai pour s'assurer que la connexion est établie
        const timeoutId = setTimeout(joinGroup, 1000);

        return () => {
            clearTimeout(timeoutId);
            if (currentQsoIdRef.current === qsoId) {
                qsoSignalRService.leaveQsoGroup(qsoId);
                currentQsoIdRef.current = null;
            }
        };
    }, [qsoId]); // Retiré la dépendance isAuthenticated

    return {
        connectionState: qsoSignalRService.getConnectionState(),
        currentQsoId: qsoSignalRService.getCurrentQsoId()
    };
};
