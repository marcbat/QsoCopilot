import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export interface QsoSignalREvents {
    onQsoUpdated: (data: { qsoId: string; qso: any }) => void;
    onQsoParticipantsChanged: (data: { qsoId: string; actionType: string; participantCallSign?: string; message?: string }) => void;
}

class QsoSignalRService {
    private connection: HubConnection | null = null;
    private currentQsoId: string | null = null;
    private events: Partial<QsoSignalREvents> = {};
    private reconnectAttempts = 0;
    private maxReconnectAttempts = 5;
    private reconnectInterval = 5000; // 5 secondes

    constructor() {
        this.setupConnection();
    }    private setupConnection() {
        // Construire la connexion SignalR
        const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5041/api';
        const apiBaseUrl = apiUrl.replace('/api', ''); // Retirer /api pour obtenir l'URL de base
        const hubUrl = `${apiBaseUrl}/qsohub`;
        
        this.connection = new HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: (retryContext: any) => {
                    if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
                        return this.reconnectInterval;
                    }
                    return null; // Arrêter les tentatives de reconnexion
                }
            })
            .configureLogging(LogLevel.Information)
            .build();

        // Configurer les gestionnaires d'événements
        this.setupEventHandlers();
    }    private setupEventHandlers() {
        if (!this.connection) return;        // Événement: QSO mis à jour
        this.connection.on('QsoUpdated', (data: any) => {
            if (this.events.onQsoUpdated) {
                this.events.onQsoUpdated(data);
            }
        });

        // Événement: Participants du QSO changés (événement unifié)
        this.connection.on('QsoParticipantsChanged', (data: any) => {
            if (this.events.onQsoParticipantsChanged) {
                this.events.onQsoParticipantsChanged(data);
            }
        });// Gestionnaires de connexion
        this.connection.onreconnecting((error: any) => {
            console.warn('SignalR: Reconnexion en cours...', error?.message);
        });

        this.connection.onreconnected(() => {
            console.log('SignalR: Reconnecté');
            this.reconnectAttempts = 0;
            // Rejoindre le groupe QSO si nécessaire
            if (this.currentQsoId) {
                this.joinQsoGroup(this.currentQsoId);
            }
        });

        this.connection.onclose((error: any) => {
            console.warn('SignalR: Connexion fermée', error?.message);
            this.reconnectAttempts++;
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                setTimeout(() => this.start(), this.reconnectInterval);
            }
        });
    }    async start(): Promise<void> {
        if (!this.connection) {
            this.setupConnection();
        }

        if (this.connection?.state === 'Disconnected') {
            try {
                await this.connection.start();
                this.reconnectAttempts = 0;
            } catch (error) {
                console.error('SignalR: Erreur de connexion:', error);
                this.reconnectAttempts++;
                if (this.reconnectAttempts < this.maxReconnectAttempts) {
                    setTimeout(() => this.start(), this.reconnectInterval);
                }
            }
        }
    }    async stop(): Promise<void> {
        if (this.connection) {
            if (this.currentQsoId) {
                await this.leaveQsoGroup(this.currentQsoId);
            }
            await this.connection.stop();
        }
    }    async joinQsoGroup(qsoId: string): Promise<void> {
        if (!this.connection || this.connection.state !== 'Connected') {
            return;
        }

        try {
            // Quitter l'ancien groupe si nécessaire
            if (this.currentQsoId && this.currentQsoId !== qsoId) {
                await this.leaveQsoGroup(this.currentQsoId);
            }

            await this.connection.invoke('JoinQsoGroup', qsoId);
            this.currentQsoId = qsoId;
        } catch (error) {
            console.error('SignalR: Erreur lors de l\'ajout au groupe:', error);
        }
    }    async leaveQsoGroup(qsoId: string): Promise<void> {
        if (!this.connection || this.connection.state !== 'Connected') {
            return;
        }

        try {
            await this.connection.invoke('LeaveQsoGroup', qsoId);
            if (this.currentQsoId === qsoId) {
                this.currentQsoId = null;
            }
        } catch (error) {
            console.error('SignalR: Erreur lors de la sortie du groupe:', error);
        }
    }

    setEventHandlers(events: Partial<QsoSignalREvents>) {
        this.events = events;
    }

    getConnectionState(): string {
        return this.connection?.state || 'Disconnected';
    }

    getCurrentQsoId(): string | null {
        return this.currentQsoId;
    }
}

// Instance singleton
export const qsoSignalRService = new QsoSignalRService();
