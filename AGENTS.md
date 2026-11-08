# AGENTS.md

Ce fichier s'adresse à toute personne, agent IA ou humain, qui reprend le travail sur MotiWriter.
Il rassemble ce qui n'a sa place ni dans le README (présenter et lancer le projet) ni dans `docs/`
(décrire le produit et le code) : **comment travailler ici sans rien casser**.

## 1. Lire avant d'agir, dans cet ordre

1. [`README.md`](README.md) : ce qu'est l'application, comment la lancer, la tester, la publier.
2. [`docs/objectif.md`](docs/objectif.md) : l'intention et le ton. Beaucoup de choix
   (pas de compteur de mots, pas de notification, pas de culpabilisation) en découlent.
3. [`docs/architecture.md`](docs/architecture.md) : découpage Core / Infrastructure / Web,
   schéma de la base, CI, conventions de code.
4. [`docs/charte-graphique.md`](docs/charte-graphique.md) : avant de toucher à l'interface.
5. [`docs/feuille-de-route.md`](docs/feuille-de-route.md) : ce qui est fait, ce qui est prévu.
6. Selon la tâche :
   - connexion, comptes, déploiement → [`docs/authentification.md`](docs/authentification.md)
     et la section serveur du README ;
   - site de présentation → `site/index.html` ;
   - CI, releases, dépendances → `.github/workflows/` et `.github/dependabot.yml`.

Puis, dans le code, partir des points d'entrée : `src/MotiWriter.Web/Program.cs`,
`src/MotiWriter.Core/ServiceCollectionExtensions.cs`, les services de `src/MotiWriter.Core/`,
et les tests correspondants dans `tests/`.

## 2. Règles absolues

### Secrets et informations personnelles

Le dépôt est **public**, le site aussi. Un secret poussé est compromis, même effacé ensuite :
c'est déjà arrivé (une clé Apple, révoquée puis retirée de l'historique).

- Les secrets vivent dans `dotnet user-secrets` (développement), dans des variables
  d'environnement (production) ou dans `private/` à la racine, **ignoré par git**. Jamais ailleurs.
- **Ajouter les fichiers un par un** (`git add <chemin>`), jamais `git add -A`, `git add .` ni
  `git commit -a`. Avant chaque commit, lire `git status --short` et examiner tout nouveau fichier.
- Ne jamais afficher la valeur d'un secret (sortie de `user-secrets list`, journal, capture) ;
  ne jamais demander à quelqu'un de coller un secret dans une conversation.
- Rien de propre à une machine dans le dépôt : chemins locaux, nom ou adresse d'un serveur,
  domaine réel, port personnalisé, pseudo d'un compte. La doc utilise des exemples
  (`motiwriter.exemple.ch`, `<domaine>`, `5000`).

### Actions à faire valider

Demander avant : `git push`, création de tag ou de release (un tag `vX.Y.Z` publie une release
publique), réécriture d'historique, fusion d'une pull request, modification des réglages du dépôt
GitHub, suppression de données. Un accord vaut pour l'action demandée, pas pour les suivantes.

### Ne pas déranger l'instance du développeur

Le développeur fait souvent tourner l'application lui-même sur `http://localhost:5006` /
`https://localhost:7096`, avec sa vraie base `src/MotiWriter.Web/App_Data/motiwriter.db`.

- Pour vérifier quelque chose, lancer sa propre instance sur un **autre port** avec une **base
  jetable** :
  ```bash
  ConnectionStrings__MotiWriter="Data Source=/tmp/motiwriter-essai.db" \
    dotnet run --project src/MotiWriter.Web --no-launch-profile --urls http://localhost:5199
  ```
- L'arrêter par son port (`lsof -ti tcp:5199 | xargs kill`), **jamais** `pkill -f MotiWriter`.
- Ne pas modifier la vraie base sans accord ; en faire une copie avant toute manipulation.

## 3. Façon de travailler

- **Langue** : on échange en français ; interface, documentation, commentaires et messages de
  commit en français ; code (types, membres, fichiers C#) en anglais.
- **Commits** : un commit par changement cohérent ; titre à l'indicatif présent
  (« Ajoute… », « Corrige… »), puis une liste à puces. Un agent IA ajoute une ligne
  `Co-Authored-By:` à son nom.
- **Interface** : directe et sans fioritures. Pas de toast, de notification ni de bouton
  « Annuler » : on corrige un jour en cliquant à nouveau dessus. Pas de boîte de confirmation
  pour un geste simple et réversible.
- **Métier** dans Core, testé ; l'interface ne fait qu'appeler les services (voir les
  conventions de `docs/architecture.md`).

### Vérifier avant d'annoncer

- `dotnet build -c Release` sans aucun avertissement (la CI les traite comme des erreurs)
  et `dotnet test` vert.
- Interface : regarder le résultat dans un navigateur, sur ordinateur **et** à largeur de
  téléphone (375 px). Chrome sans interface ne descend pas sous ~500 px de large : pour tester
  375 px, afficher la page dans une `<iframe width="375">`.
- Requêtes HTTP de contrôle : utiliser `GET`. Les pages Blazor répondent 405 à `HEAD`
  (`curl -I`), ce qui n'est pas une panne.
- Dire ce qui n'a pas pu être vérifié (pas de machine Linux, pas de vrai fournisseur OAuth…).

### Garder la documentation à jour

| Changement | À mettre à jour |
|---|---|
| Fonctionnalité visible | `README.md` (Fonctionnalités), `docs/objectif.md`, `docs/feuille-de-route.md`, `site/index.html` (et sa capture `site/apercu.png` si l'écran change) |
| Structure, stockage, CI | `docs/architecture.md` |
| Connexion, fournisseurs, déploiement | `docs/authentification.md`, section serveur du `README.md` |
| Couleur, police, composant | `docs/charte-graphique.md` |

## 4. Publier une version

1. Vérifier que `main` est vert en CI.
2. Mettre à jour les numéros de version écrits en dur dans `site/index.html` (liens de
   téléchargement de secours ; un script les met aussi à jour depuis l'API GitHub) et, si besoin,
   l'exemple `VERSION=` du README.
3. Après accord : `git tag -a vX.Y.Z -m "MotiWriter X.Y.Z"` puis `git push origin vX.Y.Z`.
   Le workflow `release.yml` rejoue la CI, fabrique les exécutables et publie la release.

## 5. Pièges connus

- **Migrations SQLite** (`SqliteSchema.cs`) : liste en ajout seul. Ne jamais modifier une
  migration publiée ; en ajouter une nouvelle.
- **Dates** : toujours passer par `TimeProvider` (« aujourd'hui » est la date *locale*). En test,
  `FakeTimeProvider` : construire avec un instant UTC et `SetLocalTimeZone`, et ne pas
  reculer l'horloge (`SetUtcNow` refuse).
- **Horodatages en base** : écrits au format `"O"`, relus avec `DateTimeOffset.Parse(...,
  RoundtripKind)` ; un `ParseExact` casse sur des données insérées à la main.
- **Razor** : ne pas mélanger texte et expressions dans un attribut (`class="a @x"` passe,
  `@x@(y)` non) ; passer par une propriété calculée.
- **Derrière un proxy** (ngrok, nginx) : l'application lit `X-Forwarded-*` mais ne les croit
  que d'un proxy local. Sans eux, elle redirige en boucle vers `https://<hôte>:7096`.
- **Apple** n'envoie le nom de la personne qu'à la toute première autorisation, dans un champ de
  formulaire à part (`AppleUserName.cs`).
- **Exécutable autonome** : la racine du contenu est le dossier de l'exécutable, pas le dossier
  courant (`Program.cs`).
- **systemd** : `ProtectHome=true` rend `/home` invisible au service ; l'application écrit dans
  `$HOME` (`.aspnet`, `.net`).
- **GitHub Pages** : un `push --force` ne déclenche pas forcément le workflow du site ;
  le relancer avec `gh workflow run pages.yml`.
- **Dependabot** : les versions majeures des paquets Microsoft et du SDK sont ignorées
  (elles suivent la version de .NET) ; une montée de .NET se fait à la main, d'un bloc.
