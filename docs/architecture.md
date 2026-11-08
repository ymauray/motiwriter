# Architecture

## Principe directeur

Le **métier est séparé de l'interface**. Toute la logique (cocher un jour, refuser un jour
futur, calculer les statistiques, préparer le tableau de bord) vit dans `MotiWriter.Core`,
qui ne connaît ni le web, ni Blazor, ni HTTP, ni SQLite.

Ainsi, plusieurs « hôtes » pourront s'appuyer sur le même métier :

```
                    ┌────────────────────┐
                    │   MotiWriter.Core  │  métier pur
                    └─────────▲──────────┘
                              │ IWritingJournal
                 ┌────────────┴─────────────┐
                 │ MotiWriter.Infrastructure│  stockage SQLite
                 └────────────▲─────────────┘
          ┌───────────────────┼───────────────────┐
┌─────────┴────────┐ ┌────────┴────────┐ ┌────────┴────────┐
│  MotiWriter.Web  │ │ MotiWriter.Api  │ │ MotiWriter.Mcp  │
│   (Blazor) ✅    │ │  (REST) à venir │ │  (MCP) envisagé │
└──────────────────┘ └────────▲────────┘ └─────────────────┘
                     ┌────────┴────────┐
                     │ Apps iOS/Android│
                     └─────────────────┘
```

## Solution actuelle

| Projet | Type | Rôle | Dépend de |
|---|---|---|---|
| `src/MotiWriter.Core` | Bibliothèque de classes | Modèle, contrats et services métier | — |
| `src/MotiWriter.Infrastructure` | Bibliothèque de classes | Implémentation SQLite du journal | Core |
| `src/MotiWriter.Web` | Blazor Web App (.NET 10) | Interface web | Core, Infrastructure |
| `tests/MotiWriter.Core.Tests` | xUnit | Tests du métier | Core |
| `tests/MotiWriter.Infrastructure.Tests` | xUnit | Tests du stockage SQLite (fichier temporaire) | Infrastructure |

La solution utilise le format `MotiWriter.slnx`. Le SDK est fixé par `global.json`
(10.0.403, montée de version autorisée au sein de .NET 10).

## MotiWriter.Core

| Dossier | Contenu |
|---|---|
| `Journal/` | `IWritingJournal` : contrat de **stockage** (lire les jours, fixer l'état d'un jour, lire l'historique). `IWritingDayService` / `WritingDayService` : **saisie** (cocher, décocher, inverser, historique) avec les règles métier. `WritingDayChange` : une entrée d'historique. `InMemoryWritingJournal` : implémentation en mémoire pour les tests. |
| `Statistics/` | `WritingStatistics` (record) et `WritingStatisticsCalculator` : **calcul pur**, sans effet de bord, qui prend un ensemble de dates et « aujourd'hui ». |
| `Dashboard/` | `IDashboardService` / `DashboardService` : assemble tout ce que la page d'accueil affiche dans un seul objet `DashboardSummary` (aujourd'hui écrit ou non, statistiques, calendrier des N dernières semaines, semaine en cours). |
| `ServiceCollectionExtensions.cs` | `AddMotiWriterCore()` : enregistre les services métier et `TimeProvider.System`. |

Règles métier :

- **Un jour d'écriture est une `DateOnly`.** « Aujourd'hui » est calculé dans le fuseau local,
  via `TimeProvider` (injecté, donc testable).
- **On peut cocher aujourd'hui et n'importe quel jour passé**, jamais un jour futur
  (`WritingDayInFutureException`).
- **Chaque changement d'état est historisé** avec sa date et son heure. Cocher un jour déjà
  coché (ou décocher un jour non coché) ne change rien et n'ajoute rien à l'historique.
- **Corriger, c'est cliquer de nouveau** : la correction est elle-même une modification,
  visible dans l'historique. L'historique n'est jamais réécrit.
- Les objets renvoyés sont des `record` simples, directement sérialisables en JSON pour la future API REST.

Seule dépendance externe : `Microsoft.Extensions.DependencyInjection.Abstractions`.

## MotiWriter.Infrastructure

- `SqliteWritingJournal` implémente `IWritingJournal` avec `Microsoft.Data.Sqlite`
  (pas d'ORM : deux tables, requêtes SQL explicites). Le changement d'état et son entrée
  d'historique sont écrits dans **la même transaction**.
- `SqliteSchema` crée et fait évoluer le schéma au premier accès. La version est stockée dans
  `PRAGMA user_version` ; chaque migration est une entrée d'une liste, **qu'on ne modifie jamais
  une fois publiée** : on en ajoute une nouvelle.
- `AddMotiWriterSqlite(connectionString, baseDirectory)` enregistre le journal, résout un chemin
  relatif depuis `baseDirectory` et crée le dossier du fichier si besoin.

Schéma (version 1) :

```sql
CREATE TABLE writing_day (
    day TEXT NOT NULL PRIMARY KEY          -- AAAA-MM-JJ
) WITHOUT ROWID;

CREATE TABLE writing_day_change (
    id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    day        TEXT    NOT NULL,           -- AAAA-MM-JJ
    written    INTEGER NOT NULL,           -- 1 = écrit, 0 = non écrit
    changed_at TEXT    NOT NULL            -- ISO 8601 avec décalage horaire
);
```

La base est en mode WAL.

## MotiWriter.Web

- `Program.cs` : culture `fr-FR`, `AddMotiWriterCore()`, `AddMotiWriterSqlite(...)` avec la
  chaîne de connexion `ConnectionStrings:MotiWriter` (par défaut `Data Source=App_Data/motiwriter.db`,
  relative au dossier du projet Web ; `App_Data/` est ignoré par git).
- `Components/Layout/MainLayout.razor` : bandeau (marque, navigation, date), contenu, colophon.
- `Components/Pages/Home.razor` : tableau de bord, **interactif** (Blazor Server). Il porte l'état
  de la page : résumé et cinq dernières modifications, rechargés après chaque clic.
- `Components/Pages/Historique.razor` : historique complet (500 dernières modifications), rendu statique.
- `Components/Dashboard/` : composants du tableau de bord, sans logique métier, qui signalent
  les clics par un `EventCallback<DateOnly>` :
  `TodayToggle` (bouton du jour), `WeekStrip` (« Cette semaine »), `WritingCalendar`
  (calendrier cliquable), `HistoryList`.
- `Formatting/FrenchDates.cs` : toutes les formulations françaises (« samedi 1er novembre »,
  « 19 h 42 », phrases d'historique, libellés accessibles).
- `wwwroot/app.css` : variables de la [charte graphique](charte-graphique.md) et styles globaux.
  Chaque composant a son propre fichier `.razor.css` (styles isolés).

## Site de présentation

Le dossier `site/` contient une page HTML statique (avec sa capture `apercu.png`) qui présente
le projet. Elle reprend la charte graphique et n'a aucune dépendance au code .NET.
Le workflow `.github/workflows/pages.yml` la publie sur GitHub Pages à chaque modification
de `site/` sur `main` : <https://ymauray.github.io/motiwriter/>.

## Conventions

- Code, noms de types et de membres en **anglais** ; commentaires, documentation et
  interface en **français**.
- Les composants Razor ne contiennent pas de logique métier : ils appellent un service de Core.
- Toute règle ajoutée dans Core s'accompagne de tests dans `MotiWriter.Core.Tests` ;
  tout changement de stockage, de tests dans `MotiWriter.Infrastructure.Tests`.
