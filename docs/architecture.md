# Architecture

## Principe directeur

Le **métier est séparé de l'interface**. Toute la logique (enregistrer un jour, calculer
les statistiques, préparer le tableau de bord) vit dans `MotiWriter.Core`, qui ne connaît
ni le web, ni Blazor, ni HTTP.

Ainsi, plusieurs « hôtes » pourront s'appuyer sur le même métier :

```
                    ┌────────────────────┐
                    │   MotiWriter.Core  │  métier pur
                    └─────────▲──────────┘
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

| Projet | Type | Rôle |
|---|---|---|
| `src/MotiWriter.Core` | Bibliothèque de classes | Modèle, contrats et services métier |
| `src/MotiWriter.Web` | Blazor Web App (.NET 10, rendu serveur) | Interface web ; dépend de Core |
| `tests/MotiWriter.Core.Tests` | xUnit | Tests du métier |

La solution utilise le format `MotiWriter.slnx`. Le SDK est fixé par `global.json`
(10.0.403, montée de version autorisée au sein de .NET 10).

## MotiWriter.Core

| Dossier | Contenu |
|---|---|
| `Journal/` | `IWritingJournal` : contrat de stockage des jours écrits (lire, marquer, démarquer). `InMemoryWritingJournal` : implémentation en mémoire pour la maquette et les tests. |
| `Statistics/` | `WritingStatistics` (record) et `WritingStatisticsCalculator` : **calcul pur**, sans effet de bord, qui prend un ensemble de dates et « aujourd'hui ». |
| `Dashboard/` | `IDashboardService` / `DashboardService` : assemble tout ce que la page d'accueil affiche dans un seul objet `DashboardSummary` (statistiques + calendrier des N dernières semaines). |
| `ServiceCollectionExtensions.cs` | `AddMotiWriterCore()` : enregistre les services métier dans l'injection de dépendances. |

Choix notables :

- **Un jour d'écriture est une `DateOnly`.** Pas d'heure, pas de fuseau dans le modèle :
  c'est l'hôte qui décide quel est « aujourd'hui ».
- **L'heure vient de `TimeProvider`** (injecté), ce qui rend les calculs testables.
- **Le stockage est fourni par l'hôte.** `AddMotiWriterCore()` n'enregistre pas
  d'`IWritingJournal` : chaque hôte choisit son implémentation.
- **Les objets renvoyés sont des `record` simples**, directement sérialisables en JSON
  pour la future API REST.
- Seule dépendance externe : `Microsoft.Extensions.DependencyInjection.Abstractions`.

## MotiWriter.Web

- `Program.cs` : culture `fr-FR`, appel à `AddMotiWriterCore()`, enregistrement d'un
  `InMemoryWritingJournal` pré-rempli par `Mock/SampleWritingDays` (données fictives,
  déterministes, ~5 mois).
- `Components/Layout/MainLayout.razor` : bandeau, contenu, colophon.
- `Components/Pages/Home.razor` : tableau de bord ; n'appelle que `IDashboardService`.
- `Components/Pages/StatCard.razor` : carte de statistique réutilisable.
- `wwwroot/app.css` : variables de la [charte graphique](charte-graphique.md) et styles globaux.
  Chaque composant a son propre fichier `.razor.css` (styles isolés).

La page d'accueil est actuellement en rendu statique côté serveur ; l'interactivité Blazor
Server est activée dans l'application et sera utilisée pour les actions (marquer un jour).

## Site de présentation

Le dossier `site/` contient une page HTML statique (avec sa capture `apercu.png`) qui présente
le projet. Elle reprend la charte graphique et n'a aucune dépendance au code .NET.
Le workflow `.github/workflows/pages.yml` la publie sur GitHub Pages à chaque modification
de `site/` sur `main` : <https://ymauray.github.io/motiwriter/>.

## Conventions

- Code, noms de types et de membres en **anglais** ; commentaires, documentation et
  interface en **français**.
- Les composants Razor ne contiennent pas de logique métier : ils appellent un service de Core.
- Toute règle de calcul ajoutée dans Core s'accompagne de tests dans `MotiWriter.Core.Tests`.
