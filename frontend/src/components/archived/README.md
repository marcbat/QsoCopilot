# Composants Archivés

Ce dossier contient les composants React qui ont été retirés du projet principal car ils ne sont plus utilisés.

## Composants archivés (Décembre 2024)

### Formulaires et listes obsolètes :
- **AddParticipantForm.tsx** - Ancien formulaire d'ajout de participant
- **NewQsoForm.tsx** - Ancien formulaire de création de QSO
- **QsoForm.tsx** - Ancien formulaire de QSO générique
- **QsoFormFixed.tsx** - Version fixe du formulaire de QSO
- **QsoList.tsx** - Ancienne liste de QSO (remplacée par QsoListPaginated)
- **QsoListFixed.tsx** - Version fixe de la liste de QSO
- **QsoListNew.tsx** - Nouvelle version de la liste de QSO (obsolète)
- **QsoManagerPage.tsx** - Ancienne page principale (remplacée par QsoManagerPagePaginated)

### Pages de profil obsolètes :
- **ProfilePageDirect.tsx** - Version directe de la page de profil
- **ProfilePageNew.tsx** - Nouvelle version de la page de profil (obsolète)
- **ProfilePageOld.tsx** - Ancienne version de la page de profil
- **ProfilePageSimple.tsx** - Version simple de la page de profil
- **ProfileTestPage.tsx** - Page de test pour le profil

### Pages de test :
- **ToastTestPage.tsx** - Page de test pour les notifications toast

## Raison de l'archivage

Ces composants ont été archivés car :
1. Ils ne sont plus référencés dans l'application principale
2. Ils ont été remplacés par des versions plus récentes et mieux optimisées
3. Ils étaient des composants de test qui ne sont plus nécessaires

## Restauration

Pour restaurer un composant, il suffit de le déplacer du dossier `archived` vers le dossier `components` parent et de l'importer dans les fichiers appropriés.
