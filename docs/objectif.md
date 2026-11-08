# Objectif

## L'idée

MotiWriter aide à **garder l'envie d'écrire** en montrant ce qu'on a déjà accompli.

Le principe repose sur une observation simple : la régularité compte plus que la quantité.
Écrire quelques mots chaque jour entretient l'habitude ; voir le chemin parcouru donne envie
de continuer. L'application ne juge pas, elle **constate et encourage**.

## Ce que fait la première version

- On note les **jours où l'on a écrit**, sans rien d'autre : un jour est « écrit » ou « non écrit ».
  Quelques mots suffisent pour que la journée compte.
- La page d'accueil est un **tableau de bord** qui présente le travail accompli :
  statistiques et calendrier des jours écrits.
- Chaque modification est conservée dans un **historique**.

## Noter ses jours

Une seule action — **cocher ou décocher un jour** — accessible à trois endroits,
chacun adapté à un usage :

| Où | Pour quoi |
|---|---|
| Le bouton **« J'ai écrit aujourd'hui »**, sous la phrase d'accueil | Le geste quotidien. Coché, il devient « ✓ Écrit aujourd'hui » ; un clic de plus l'annule. |
| La bande **« Cette semaine »** (lundi → dimanche) | Rattraper un oubli récent (« j'ai oublié de noter hier »). Grandes pastilles, faciles à toucher sur téléphone. |
| Le **calendrier** des vingt dernières semaines | Corriger un jour plus ancien : chaque case se coche ou se décoche d'un clic. |

Les jours futurs ne peuvent pas être cochés.

**Pas de confirmation, mais une annulation.** Après chaque clic, un bandeau
(« Mardi 3 novembre marqué non écrit. **Annuler** ») reste affiché huit secondes, et ne
disparaît pas tant que le pointeur ou le focus est dessus. Le geste reste instantané,
et une erreur se rattrape en un clic.

## L'historique

Chaque changement est noté, sous la forme :

> Le mardi 3 novembre a été marqué écrit le dimanche 8 novembre à 19 h 42.

Les cinq dernières modifications figurent sur le tableau de bord ; la page **Historique**
les présente toutes, de la plus récente à la plus ancienne. Une annulation est une
modification comme une autre : elle apparaît aussi dans l'historique.

## Ce que la première version ne fait pas (volontairement)

- **Pas de saisie du nombre de mots.** C'est un choix : on valorise le geste d'écrire,
  pas le volume. Cette option pourra être envisagée plus tard.
- **Pas de comptes utilisateurs** : l'application est pensée, pour l'instant, pour une seule personne.
- **Pas d'objectifs chiffrés ni de rappels.**

## Ton

L'application s'adresse à l'auteur avec bienveillance. Les messages encouragent
(« Quelques mots aujourd'hui suffisent à relancer la série ») et ne culpabilisent jamais
un jour sans écriture : dans le calendrier, ce jour s'appelle « silence », pas « échec ».

## Les statistiques

Les indicateurs du tableau de bord **restent à définir**. Ceux de la maquette actuelle
sont provisoires :

| Indicateur | Définition |
|---|---|
| Jours écrits | Nombre total de jours marqués, jusqu'à aujourd'hui inclus |
| Série en cours | Jours consécutifs écrits jusqu'à aujourd'hui. Si aujourd'hui n'est pas encore écrit, on compte à partir d'hier : la série n'est rompue qu'une fois la journée passée |
| Plus longue série | Le record de jours consécutifs |
| Jours ce mois-ci | Jours écrits depuis le 1er du mois courant |

Les jours futurs sont ignorés dans tous les calculs.
