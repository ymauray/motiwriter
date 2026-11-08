# Feuille de route

## ✅ Fait (novembre 2026)

- Solution .NET 10 : `Core` (métier), `Web` (Blazor), `Core.Tests` (xUnit).
- Modèle « jour écrit / non écrit » et contrat `IWritingJournal`.
- Calcul des statistiques provisoires : jours écrits, série en cours, plus longue série,
  jours du mois — couvert par des tests.
- Tableau de bord : titre, phrase d'encouragement, quatre cartes, calendrier des 20 dernières semaines.
- Charte graphique crème et encre, typographie serif.
- **Saisie des jours** : bouton « J'ai écrit aujourd'hui », bande « Cette semaine »,
  calendrier cliquable, bandeau « Annuler » ; jours futurs refusés.
- **Historique** de chaque modification (tableau de bord et page dédiée).
- **Stockage SQLite** (`MotiWriter.Infrastructure`) avec schéma versionné.
- Dépôt public [ymauray/motiwriter](https://github.com/ymauray/motiwriter) et
  [site de présentation](https://ymauray.github.io/motiwriter/) publié par GitHub Pages.

## 🔜 Prochaines étapes

1. **Définir les statistiques** du tableau de bord (à préciser selon ce qui motive le plus).
2. **Remonter plus loin** que vingt semaines dans le calendrier (navigation par mois ou par année).
3. **Sauvegarde** : exporter et importer ses jours (fichier JSON ou CSV).

## 🧭 Plus tard

- **API REST** (`MotiWriter.Api`) exposant le même métier, pour des applications mobiles.
- **Applications iOS et Android** consommant l'API.
- **Serveur MCP** (`MotiWriter.Mcp`) pour consulter et noter ses jours d'écriture depuis un assistant.
- **Authentification** et données par utilisateur, nécessaires dès que l'API sera exposée.

## 💭 Idées à discuter

- Saisie facultative du nombre de mots.
- Notes ou projet associé à une journée.
- Thème sombre respectant l'esprit « papier et encre ».
- Rappels doux.
