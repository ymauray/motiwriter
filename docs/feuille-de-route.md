# Feuille de route

## ✅ Fait (novembre 2026)

- Solution .NET 10 : `Core` (métier), `Web` (Blazor), `Core.Tests` (xUnit).
- Modèle « jour écrit / non écrit » et contrat `IWritingJournal`.
- Calcul des statistiques provisoires : jours écrits, série en cours, plus longue série,
  jours du mois — couvert par des tests.
- Tableau de bord : titre, phrase d'encouragement, quatre cartes, calendrier des 20 dernières semaines.
- Charte graphique crème et encre, typographie serif.
- **Saisie des jours** : bande « Les huit derniers jours » (dont « Aujourd'hui ») et
  calendrier cliquable ; jours futurs refusés.
- **Historique** de chaque modification (tableau de bord et page dédiée, regroupée par mois).
- **Stockage SQLite** (`MotiWriter.Infrastructure`) avec schéma versionné.
- **Comptes et connexion OAuth** (Discord, Twitch, Google, Apple), sans mot de passe ;
  chaque personne ne voit que ses données.
- Dépôt public [ymauray/motiwriter](https://github.com/ymauray/motiwriter) et
  [site de présentation](https://ymauray.github.io/motiwriter/) publié par GitHub Pages.

## 🔜 Prochaines étapes

1. **Définir les statistiques** du tableau de bord (à préciser selon ce qui motive le plus).
2. **Remonter plus loin** qu'un an dans le calendrier (navigation par année).
3. **Sauvegarde** : exporter et importer ses jours (fichier JSON ou CSV).
4. **Relier plusieurs fournisseurs** à un même compte (depuis une page « Mon compte »).
5. **Supprimer son compte** et toutes ses données.

## 🧭 Plus tard

- **API REST** (`MotiWriter.Api`) exposant le même métier, pour des applications mobiles.
- **Applications iOS et Android** consommant l'API.
- **Serveur MCP** (`MotiWriter.Mcp`) pour consulter et noter ses jours d'écriture depuis un assistant.
- **Authentification de l'API** (jetons) pour les applications mobiles et le serveur MCP.
- **Déploiement** de l'application web pour la partager (HTTPS, domaine, clés Data Protection).

## 💭 Idées à discuter

- Saisie facultative du nombre de mots.
- Notes ou projet associé à une journée.
- Thème sombre respectant l'esprit « papier et encre ».
- Rappels doux.
