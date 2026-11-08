# MotiWriter

**Écrire un peu, souvent.**

MotiWriter est une application qui aide à garder l'envie d'écrire en montrant ce qu'on a
déjà accompli. On note simplement les jours où l'on a écrit — quelques mots suffisent —
et un tableau de bord présente le chemin parcouru : jours écrits, séries, calendrier.

> 🚧 Projet en phase de maquette : les données affichées sont fictives et ne sont pas encore enregistrées.

## Fonctionnalités

- Tableau de bord : nombre de jours écrits, série en cours, plus longue série, jours du mois.
- Calendrier des dernières semaines, jour par jour.
- Interface en français, sobre, dans un esprit « papier et encre ».

Le métier est séparé de l'interface pour pouvoir être exposé plus tard en API REST
(applications iOS et Android) et en serveur MCP.

## Prérequis

- [SDK .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.403 ou plus récent, voir `global.json`)
- Une connexion internet pour charger les polices depuis Google Fonts (sinon, repli sur Georgia)

## Démarrer en développement

```bash
git clone <url-du-depot>
cd MotiWriter

dotnet build
dotnet run --project src/MotiWriter.Web --launch-profile http
```

L'application est disponible sur <http://localhost:5006>.
Avec le profil `https` : <https://localhost:7096> (exécuter `dotnet dev-certs https --trust` la première fois).

Rechargement à chaud pendant le développement :

```bash
dotnet watch --project src/MotiWriter.Web
```

## Tests

```bash
dotnet test
```

## Structure

```
MotiWriter.slnx
src/
  MotiWriter.Core/          Métier : journal, statistiques, tableau de bord
  MotiWriter.Web/           Interface web Blazor (.NET 10)
tests/
  MotiWriter.Core.Tests/    Tests unitaires du métier (xUnit)
docs/                       Documentation du projet
```

## Documentation

- [Objectif](docs/objectif.md)
- [Charte graphique](docs/charte-graphique.md)
- [Architecture](docs/architecture.md)
- [Feuille de route](docs/feuille-de-route.md)
