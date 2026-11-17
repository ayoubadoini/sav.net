# SAV Manager

Application de gestion de service après-vente (SAV) pour suivre les clients, demandes, interventions, garanties et facturations.

## Présentation

Ce projet est une solution complète construite autour d'une architecture modulaire :
- backend ASP.NET Core pour l'API et la logique métier,
- frontend React pour l'interface utilisateur,
- base de données SQL Server,
- authentification sécurisée via JWT,
- notifications temps réel avec SignalR,
- génération de PDF et documents métier.

L'objectif est de centraliser la gestion du SAV d'une entreprise : dossier client, réclamations, interventions techniques, suivi d'état, facturation et reporting.

## Structure du projet

```text
SAVSolution/
├── SAV.API/                 # API backend ASP.NET Core
├── SAV.Application/         # Cas d'usage, DTOs, services applicatifs
├── SAV.Domain/              # Modèles métier et entités
├── SAV.Infrastructure/      # DbContext, repositories, Identity, services
├── SAV.Shared/              # Constantes et interfaces partagées
├── SAV.WebUI/               # Application web ASP.NET
├── sav-webui/               # Frontend React / TypeScript
├── docker-compose.yml       # Déploiement local avec conteneurs
├── Dockerfile               # Image du backend
├── .gitignore               # Fichiers ignorés par Git
├── README.md                # Documentation principale
├── QUICK_START_TESTING.md   # Guide rapide de test
├── GETTING_STARTED.md       # Démarrage du projet
├── API_DOCUMENTATION.md     # Documentation API
├── BACKEND_DOCUMENTATION.md  # Documentation backend
├── TESTING_GUIDE.md         # Scénarios de tests
├── PROJECT_COMPLETE.md      # Résumé du projet
└── ...
```

## Fonctionnalités principales

- gestion des clients
- gestion des réclamations / tickets SAV
- suivi des interventions techniques
- gestion des garanties
- génération de factures et PDF
- authentification avec rôles utilisateur
- tableau de bord et suivi d'activité
- API REST documentée avec Swagger
- notifications en temps réel
- support Docker pour exécution locale

## Stack technique

### Backend
- ASP.NET Core
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- JWT
- MediatR
- SignalR
- Swagger / OpenAPI

### Frontend
- React
- TypeScript
- Axios
- Bootstrap / styles UI
- React Router

## Prérequis

Avant de démarrer, vérifie que tu as :
- .NET SDK compatible
- Node.js 18+
- SQL Server local ou accessible
- Docker / Docker Compose (optionnel)

## Démarrage rapide

### 1. Cloner le dépôt

```bash
git clone <url-du-repo>
cd SAVSolution
```

### 2. Restaurer les dépendances

#### Backend
```bash
cd SAV.API
dotnet restore
```

#### Frontend
```bash
cd ../sav-webui
npm install
```

### 3. Lancer le backend

```bash
cd ../SAV.API
dotnet run
```

L'API est généralement disponible sur :
- https://localhost:7001
- Swagger : https://localhost:7001/swagger

### 4. Lancer le frontend

```bash
cd ../sav-webui
npm start
```

Le frontend est généralement disponible sur :
- http://localhost:3000

### 5. Lancer avec Docker

```bash
docker-compose up -d
```

## Comptes de test

Le backend crée automatiquement un compte administrateur au démarrage si nécessaire.

Exemple :
- Email : admin@savmanager.com
- Mot de passe : Admin@123456

> À utiliser uniquement dans un environnement local de test.

## Rôles applicatifs

- Admin
- Responsable SAV
- Technicien
- Client

## Documents de référence

Ce dépôt contient plusieurs guides utiles :
- [GETTING_STARTED.md](GETTING_STARTED.md)
- [QUICK_START_TESTING.md](QUICK_START_TESTING.md)
- [API_DOCUMENTATION.md](API_DOCUMENTATION.md)
- [BACKEND_DOCUMENTATION.md](BACKEND_DOCUMENTATION.md)
- [TESTING_GUIDE.md](TESTING_GUIDE.md)
- [PROJECT_COMPLETE.md](PROJECT_COMPLETE.md)

## Variables d'environnement et configuration

Vérifie les fichiers de configuration suivants :
- `SAV.API/appsettings.json`
- `SAV.API/appsettings.Development.json`
- éventuellement un fichier `.env` si ton environnement l'utilise

Les secrets sensibles ne doivent pas être commités dans le dépôt.

## Bonnes pratiques

- ne pas versionner les fichiers `.env` ni les secrets
- garder les configurations locales dans l'environnement du PC
- vérifier la chaîne de connexion SQL avant démarrage
- tester les changements avant de les pousser

## Contribution

1. créer une branche
2. développer la fonctionnalité ou correction
3. tester localement
4. commit avec un message clair
5. ouvrir une PR

## Résumé

SAV Manager est une application de gestion complète pour les services après-vente, pensée pour centraliser les démarches client, la maintenance, les interventions, la facturation et le suivi des demandes.

---

Projet : SAV Manager
Statut : solution fonctionnelle, documentation présente dans le dépôt
