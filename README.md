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

## Binaire autonome

L'application peut être publiée en **un seul exécutable** qui embarque le runtime .NET et
ASP.NET Core : rien à installer sur la machine cible.

```bash
dotnet publish src/MotiWriter.Web -c Release -r osx-arm64 -o publish/osx-arm64
# autres cibles : osx-x64, linux-x64, linux-arm64, win-x64…
```

Le dossier obtenu (environ 50 Mo) se copie tel quel :

```
MotiWriter.Web                                   exécutable (runtime, DLL et SQLite inclus)
appsettings.json                                 configuration
wwwroot/                                         feuilles de style et scripts
MotiWriter.Web.staticwebassets.endpoints.json    index des fichiers de wwwroot
```

```bash
./publish/osx-arm64/MotiWriter.Web --urls http://localhost:5000
```

- Les fichiers sont cherchés **à côté de l'exécutable**, quel que soit le dossier courant ; la base
  est créée dans `App_Data/` à côté de lui (modifiable avec `ConnectionStrings__MotiWriter`).
- L'exécutable tourne en environnement *Production* : `dotnet user-secrets` n'y est **pas** lu.
  Les identifiants OAuth se donnent par variables d'environnement, par exemple
  `Authentication__Discord__ClientId`, `Authentication__Apple__PrivateKey="$(cat AuthKey.p8)"`.
- Sous Linux, la bibliothèque ICU (`libicu`) doit être installée pour les dates en français.
- Pour une mise en ligne (HTTPS, proxy, clés Data Protection), voir
  [docs/authentification.md](docs/authentification.md#mise-en-production).

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
