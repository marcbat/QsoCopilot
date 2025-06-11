// Script d'initialisation MongoDB pour QSO Manager
db = db.getSiblingDB('QsoManager');

// Créer un utilisateur pour l'application
db.createUser({
  user: 'qso-user',
  pwd: 'qso-password',
  roles: [
    {
      role: 'readWrite',
      db: 'QsoManager'
    }
  ]
});

// Créer les collections de base
db.createCollection('Events');
db.createCollection('Projections');

// Créer des index pour optimiser les performances
db.Events.createIndex({ "AggregateId": 1, "Version": 1 });
db.Events.createIndex({ "Timestamp": 1 });
db.Events.createIndex({ "EventType": 1 });

print('MongoDB initialisé avec succès pour QSO Manager');
