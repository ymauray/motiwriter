# Feuille de route

## ✅ Fait — maquette (novembre 2026)

- Solution .NET 10 : `Core` (métier), `Web` (Blazor), `Core.Tests` (xUnit).
- Modèle « jour écrit / non écrit » et contrat `IWritingJournal`.
- Journal en mémoire avec données fictives.
- Calcul des statistiques provisoires : jours écrits, série en cours, plus longue série,
  jours du mois — couvert par des tests.
- Tableau de bord : titre, phrase d'encouragement, quatre cartes, calendrier des 20 dernières semaines.
- Charte graphique crème et encre, typographie serif.

## 🔜 Prochaines étapes

1. **Persistance** : remplacer le journal en mémoire par un stockage réel
   (SQLite proposé : un simple fichier, sans serveur), dans un projet dédié
   (par exemple `MotiWriter.Infrastructure`) pour garder Core indépendant.
2. **Marquer un jour** : bouton « J'ai écrit aujourd'hui » ; clic sur une case du calendrier
   pour corriger un jour passé.
3. **Gestion de versions** : initialiser git et publier sur GitHub.

## 🧭 Plus tard

- **Définir les statistiques** du tableau de bord (à préciser selon ce qui motive le plus).
- **API REST** (`MotiWriter.Api`) exposant le même métier, pour des applications mobiles.
- **Applications iOS et Android** consommant l'API.
- **Serveur MCP** (`MotiWriter.Mcp`) pour consulter et noter ses jours d'écriture depuis un assistant.
- **Authentification** et données par utilisateur, nécessaires dès que l'API sera exposée.

## 💭 Idées à discuter

- Saisie facultative du nombre de mots.
- Notes ou projet associé à une journée.
- Thème sombre respectant l'esprit « papier et encre ».
- Rappels doux.
