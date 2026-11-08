# Contribuer à MotiWriter

Merci de vous intéresser à MotiWriter ! Ce document résume comment proposer une contribution.

## Avant de commencer

Pour une nouvelle fonctionnalité ou un changement important, ouvrez d'abord une
[discussion](https://github.com/ymauray/motiwriter/discussions) ou une issue pour en parler avec
le mainteneur. Lisez aussi [`docs/objectif.md`](docs/objectif.md) : MotiWriter fait volontairement
peu de choses (pas de compteur de mots, pas de notification), et certaines idées n'y ont pas leur place.

## Environnement de développement

Il faut le [SDK .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) (version minimale dans
`global.json`).

```sh
dotnet build
dotnet test
dotnet run --project src/MotiWriter.Web --launch-profile http   # http://localhost:5006
```

Pour se connecter sans configurer de fournisseur OAuth, voir la section « Connexion » du
[README](README.md#connexion).

## Proposer une modification

1. Créez une branche dédiée depuis `main` (ex. `fix/...`, `feature/...`).
2. Faites des commits atomiques, avec des messages clairs, en français.
3. Vérifiez que `dotnet build -c Release` ne produit aucun avertissement et que `dotnet test` passe.
4. Ouvrez une pull request vers `main` en remplissant le modèle fourni. La CI doit être verte ;
   les PR sont fusionnées en squash.

## Conventions

- Code (types, membres) en anglais ; interface, documentation, commentaires et commits en français.
- Le métier vit dans `MotiWriter.Core` et s'accompagne de tests ; l'interface ne fait qu'appeler
  ses services. Détails dans [`docs/architecture.md`](docs/architecture.md).
- Toute modification visible met à jour la documentation concernée.
- Jamais de secret dans le dépôt : `dotnet user-secrets` en développement.

Les règles complètes (ordre de lecture, vérifications, pièges connus) sont dans
[`AGENTS.md`](AGENTS.md), valables pour les humains comme pour les agents IA.
