# MotiWriter

**Écrire un peu, souvent.**

MotiWriter est une application qui aide à garder l'envie d'écrire en montrant ce qu'on a
déjà accompli. On note simplement les jours où l'on a écrit — quelques mots suffisent —
et un tableau de bord présente le chemin parcouru : jours écrits, séries, calendrier.

🌐 Site de présentation : <https://ymauray.github.io/motiwriter/>

> 🚧 Projet jeune : les statistiques du tableau de bord sont encore provisoires.

## Fonctionnalités

- Un clic pour noter qu'on a écrit aujourd'hui ; rattrapage des jours passés depuis les sept
  derniers jours ou le calendrier ; décocher un jour se fait d'un clic.
- Historique de chaque modification, mois par mois (« Le mardi 3 novembre a été marqué écrit le dimanche 8 novembre à 19 h 42. »).
- Tableau de bord : nombre de jours écrits, série en cours, plus longue série, jours du mois.
- Calendrier jour par jour, qui remplit toute la largeur de l'écran (jusqu'à un an).
- Connexion avec Discord, Twitch, Google ou Apple, sans mot de passe ; chaque personne ne voit que ses jours.
- Données enregistrées dans un fichier SQLite.
- Interface en français, sobre, dans un esprit « papier et encre ».

Le métier est séparé de l'interface pour pouvoir être exposé plus tard en API REST
(applications iOS et Android) et en serveur MCP.

## Prérequis

- [SDK .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.403 ou plus récent, voir `global.json`)
- Une connexion internet pour charger les polices depuis Google Fonts (sinon, repli sur Georgia)

## Démarrer en développement

```bash
git clone https://github.com/ymauray/motiwriter.git
cd motiwriter

dotnet build
dotnet run --project src/MotiWriter.Web --launch-profile http
```

L'application est disponible sur <http://localhost:5006>.
Avec le profil `https` : <https://localhost:7096> (exécuter `dotnet dev-certs https --trust` la première fois).

Rechargement à chaud pendant le développement :

```bash
dotnet watch --project src/MotiWriter.Web
```

## Données

Les jours sont enregistrés dans `src/MotiWriter.Web/App_Data/motiwriter.db`, créé au premier
lancement (le dossier est ignoré par git). Pour utiliser un autre fichier :

```bash
ConnectionStrings__MotiWriter="Data Source=/chemin/vers/motiwriter.db" \
  dotnet run --project src/MotiWriter.Web --launch-profile http
```

## Connexion

Sans configuration, la page de connexion ne propose aucun fournisseur. Deux possibilités :

- configurer un ou plusieurs fournisseurs OAuth avec `dotnet user-secrets`
  (marche à suivre : [docs/authentification.md](docs/authentification.md)) ;
- ou, pour développer, activer le compte de développement :

```bash
Authentication__Development__Enabled=true \
  dotnet run --project src/MotiWriter.Web --launch-profile https
```

## Tests

```bash
dotnet test
```

## Structure

```
MotiWriter.slnx
src/
  MotiWriter.Core/            Métier : saisie des jours, historique, statistiques, tableau de bord
  MotiWriter.Infrastructure/  Stockage SQLite
  MotiWriter.Web/             Interface web Blazor (.NET 10)
tests/
  MotiWriter.Core.Tests/            Tests du métier (xUnit)
  MotiWriter.Infrastructure.Tests/  Tests du stockage SQLite (xUnit)
docs/                         Documentation du projet
site/                         Site de présentation (GitHub Pages)
```

## Documentation

- [Objectif](docs/objectif.md)
- [Charte graphique](docs/charte-graphique.md)
- [Architecture](docs/architecture.md)
- [Authentification](docs/authentification.md)
- [Feuille de route](docs/feuille-de-route.md)
