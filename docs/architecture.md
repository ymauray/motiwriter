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
| `Accounts/` | `UserId`, `UserAccount`, `ExternalIdentity`. `IUserAccountStore` : stockage des comptes et des connexions externes. `IAccountService` / `AccountService` : retrouve ou crée le compte à la connexion, rafraîchit le nom sans jamais l'effacer. |
| `Journal/` | `IWritingJournal` : contrat de **stockage** (lire les jours, fixer l'état d'un jour, lire l'historique). `IWritingDayService` / `WritingDayService` : **saisie** (cocher, décocher, inverser) et **historique** (liste, ou regroupé par mois avec `HistoryMonth`), avec les règles métier. `WritingDayChange` : une entrée d'historique. `InMemoryWritingJournal` : implémentation en mémoire pour les tests. |
| `Statistics/` | `WritingStatistics` (record) et `WritingStatisticsCalculator` : **calcul pur**, sans effet de bord, qui prend un ensemble de dates et « aujourd'hui ». |
| `Dashboard/` | `IDashboardService` / `DashboardService` : assemble tout ce que la page d'accueil affiche dans un seul objet `DashboardSummary` (aujourd'hui écrit ou non, statistiques, calendrier des N dernières semaines, huit derniers jours). |
| `ServiceCollectionExtensions.cs` | `AddMotiWriterCore()` : enregistre les services métier et `TimeProvider.System`. |

Règles métier :

- **Toutes les données appartiennent à un compte** : chaque méthode de `IWritingJournal`,
  `IWritingDayService` et `IDashboardService` prend un `UserId` explicite. C'est l'hôte (Web,
  future API) qui le tire de la session authentifiée.
- **Un compte = une connexion (fournisseur, identifiant)** ; pas de fusion par adresse e-mail.

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

- `SqliteDatabase` ouvre les connexions (clés étrangères activées) et applique les migrations
  au premier accès ; il est partagé par les deux stockages.
- `SqliteUserAccountStore` implémente `IUserAccountStore`. Le tout premier compte créé adopte
  les données « non réclamées » (`user_id = 0`) héritées du schéma version 1.
- `SqliteWritingJournal` implémente `IWritingJournal` avec `Microsoft.Data.Sqlite`
  (pas d'ORM : deux tables, requêtes SQL explicites). Le changement d'état et son entrée
  d'historique sont écrits dans **la même transaction**.
- `SqliteSchema` crée et fait évoluer le schéma au premier accès. La version est stockée dans
  `PRAGMA user_version` ; chaque migration est une entrée d'une liste, **qu'on ne modifie jamais
  une fois publiée** : on en ajoute une nouvelle.
- `AddMotiWriterSqlite(connectionString, baseDirectory)` enregistre le journal, résout un chemin
  relatif depuis `baseDirectory` et crée le dossier du fichier si besoin.

Schéma (version 2) :

```sql
CREATE TABLE user_account (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    display_name TEXT    NOT NULL,
    email        TEXT,                     -- jamais demandé aux fournisseurs pour l'instant
    created_at   TEXT    NOT NULL
);

CREATE TABLE external_login (
    provider TEXT    NOT NULL,             -- Discord, Twitch, Google, Apple
    subject  TEXT    NOT NULL,             -- identifiant stable chez le fournisseur
    user_id  INTEGER NOT NULL REFERENCES user_account (id) ON DELETE CASCADE,
    PRIMARY KEY (provider, subject)
) WITHOUT ROWID;

CREATE TABLE writing_day (
    user_id INTEGER NOT NULL,
    day     TEXT    NOT NULL,              -- AAAA-MM-JJ
    PRIMARY KEY (user_id, day)
) WITHOUT ROWID;

CREATE TABLE writing_day_change (
    id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    day        TEXT    NOT NULL,           -- AAAA-MM-JJ
    written    INTEGER NOT NULL,           -- 1 = écrit, 0 = non écrit
    changed_at TEXT    NOT NULL,           -- ISO 8601 avec décalage horaire
    user_id    INTEGER NOT NULL DEFAULT 0
);
```

La base est en mode WAL.

## MotiWriter.Web

- `Program.cs` : culture `fr-FR`, `AddMotiWriterCore()`, `AddMotiWriterSqlite(...)` avec la
  chaîne de connexion `ConnectionStrings:MotiWriter` (par défaut `Data Source=App_Data/motiwriter.db`,
  relative au dossier du projet Web ; `App_Data/` est ignoré par git).
- `Authentication/` : `AuthenticationSetup` (cookie de session, cookie externe temporaire, et
  chaque fournisseur OAuth activé seulement s'il est configuré), `AuthenticationEndpoints`
  (`POST /connexion/{fournisseur}`, `GET /connexion/retour`, `POST /deconnexion`), extensions de
  `ClaimsPrincipal` (`GetUserId()`). Détails : [Authentification](authentification.md).
- `Components/Pages/Connexion.razor` : page de connexion (un bouton par fournisseur configuré).
  Le tableau de bord et l'historique portent `[Authorize]` ; sans session, on est redirigé vers `/connexion`.
- `Components/Layout/MainLayout.razor` : bandeau (marque, navigation, date, nom et « Se déconnecter »), contenu, colophon.
- `Components/Pages/Home.razor` : tableau de bord, **interactif** (Blazor Server). Il porte l'état
  de la page : résumé et cinq dernières modifications, rechargés après chaque clic.
- `Components/Pages/Historique.razor` : historique complet, un bloc `<details>` repliable par mois
  (seul le mois en cours est ouvert), rendu statique, sans JavaScript.
- `Components/Dashboard/` : composants du tableau de bord, sans logique métier, qui signalent
  les clics par un `EventCallback<DateOnly>` :
  `WritingCalendar` (calendrier cliquable), `RecentDays` (« Les huit derniers jours »), `HistoryList`.
- Le tableau de bord charge 53 semaines ; `WritingCalendar` n'affiche que les plus récentes qui
  tiennent en largeur. Son module JavaScript isolé (`WritingCalendar.razor.js`) mesure le calendrier
  avec un `ResizeObserver` et renvoie le nombre de semaines au composant ; la grille CSS
  (colonnes `1fr`, cases en `aspect-ratio: 1`) étire ensuite les cases pour remplir exactement la largeur.
- `Formatting/FrenchNumbers.cs` : nombres en toutes lettres pour les titres.
- `Formatting/FrenchDates.cs` : toutes les formulations françaises (« samedi 1er novembre »,
  « 19 h 42 », phrases d'historique, libellés accessibles).
- `wwwroot/app.css` : variables de la [charte graphique](charte-graphique.md) et styles globaux.
  Chaque composant a son propre fichier `.razor.css` (styles isolés).

## Site de présentation

Le dossier `site/` contient une page HTML statique (avec sa capture `apercu.png`) qui présente
le projet. Elle reprend la charte graphique et n'a aucune dépendance au code .NET.
Le workflow `.github/workflows/pages.yml` la publie sur GitHub Pages à chaque modification
de `site/` sur `main` : <https://ymauray.github.io/motiwriter/>.

## Intégration continue

**`.github/workflows/ci.yml`** tourne à chaque push sur `main` touchant le code (les changements
limités à `docs/`, `site/` ou aux fichiers Markdown ne le déclenchent pas), à chaque pull request
et à la demande : compilation en Release avec les avertissements traités comme des erreurs, puis
tous les tests ; les résultats (`.trx`) sont joints à l'exécution.

**`.github/workflows/release.yml`** tourne sur un tag `vX.Y.Z` :

1. il rejoue la CI (workflow réutilisé) ;
2. il publie les binaires autonomes pour linux-x64, linux-arm64, osx-arm64, osx-x64 et win-x64,
   avec la version du tag (`-p:Version=X.Y.Z`) ; l'exécutable linux-x64 est démarré sur le runner
   pour vérifier qu'il sert la page de connexion et ses fichiers statiques ;
3. il crée la release GitHub avec une archive par plateforme (`motiwriter-X.Y.Z-<rid>.tar.gz`,
   `.zip` pour Windows), le fichier `SHA256SUMS.txt` et des notes générées depuis les commits.

**`.github/dependabot.yml`** : chaque lundi, Dependabot propose par pull request les mises à
jour des actions GitHub, des paquets NuGet (regroupés : authentification, tests, runtime) et du
SDK de `global.json`. Les versions majeures des paquets Microsoft et du SDK sont exclues : elles
suivent la version de .NET, dont le changement se fait à la main. Chaque pull request passe par la CI.

## Conventions

- Code, noms de types et de membres en **anglais** ; commentaires, documentation et
  interface en **français**.
- Les composants Razor ne contiennent pas de logique métier : ils appellent un service de Core.
- Toute règle ajoutée dans Core s'accompagne de tests dans `MotiWriter.Core.Tests` ;
  tout changement de stockage, de tests dans `MotiWriter.Infrastructure.Tests`.
