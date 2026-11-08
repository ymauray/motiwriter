# Charte graphique

## Intention

Une ambiance de **papier et d'encre** : élégante, calme, littéraire. L'interface doit
évoquer un carnet ou une belle édition plutôt qu'un logiciel. Peu d'éléments, beaucoup d'air,
une typographie soignée.

Trois règles :

1. **Fonds crème**, jamais de blanc pur.
2. **Texte très foncé, légèrement bleuté**, jamais de noir pur.
3. **Polices à empattements (serif)** partout.

## Couleurs

Toutes les couleurs sont définies comme variables CSS dans
[`src/MotiWriter.Web/wwwroot/app.css`](../src/MotiWriter.Web/wwwroot/app.css).
On utilise toujours la variable, jamais la valeur en dur.

### Papier

| Variable | Valeur | Usage |
|---|---|---|
| `--paper` | `#f8f3e7` | Fond de page |
| `--paper-raised` | `#fcf9f1` | Fond des cartes et panneaux (légèrement plus clair) |
| `--paper-deep` | `#ece4d1` | Surfaces en creux : case d'un jour non écrit |
| `--rule` | `#e0d6c0` | Filets, bordures, séparateurs |

### Encre

| Variable | Valeur | Usage |
|---|---|---|
| `--ink` | `#16202f` | Titres, chiffres, texte principal |
| `--ink-soft` | `#33405a` | Texte courant secondaire (chapeau, paragraphes) |
| `--ink-muted` | `#6a7285` | Légendes, dates, mentions discrètes |
| `--ink-blue` | `#24395a` | Donnée « écrit » (case du calendrier), sélection de texte |
| `--accent` | `#3d5a80` | Liens, surtitres, mise en avant d'aujourd'hui |

L'accent reste **rare** : il signale, il ne décore pas.

## Typographie

| Variable | Police | Usage |
|---|---|---|
| `--font-display` | **Cormorant Garamond** (500, 600, italique 500) | Titres, grands chiffres, nom de marque, citations |
| `--font-body` | **Source Serif 4** (400, 600, italique 400) | Texte courant, libellés |

Les deux polices sont chargées depuis Google Fonts (dans `Components/App.razor`).
Repli : *Iowan Old Style*, puis *Georgia*, puis la serif du système.

Repères :

- Titre principal : Cormorant 600, `clamp(3rem, 8vw, 5.5rem)`, interligne serré (0.95).
- Titres de section : Cormorant 500 **italique**, ~1.75rem.
- Grands chiffres des statistiques : Cormorant 500, ~3.25rem, chiffres tabulaires.
- Surtitre (« Tableau de bord ») : petites capitales simulées, 0.8rem, espacement 0.18em, couleur `--accent`.
- Texte courant : Source Serif 4, 17px, interligne 1.55.
- Légendes et dates : italique, couleur `--ink-muted`.

## Composants

- **Bandeau** : nom de marque à gauche, puis la navigation (« Tableau de bord », « Historique »)
  en italique `--ink-muted` (page active en `--ink`, soulignée en `--accent`), date du jour en
  italique à droite, filet `--rule` en dessous.
- **Bouton du jour** (`TodayToggle`) : non coché, contour `--ink-blue` sur `--paper-raised`
  avec « ✎ J'ai écrit aujourd'hui » ; coché, plein `--ink-blue` avec « ✓ Écrit aujourd'hui ».
  Une légende en italique dessous (« Quelques mots suffisent. » / « Un clic de plus pour annuler. »).
- **Bande « Cette semaine »** (`WeekStrip`) : sept pastilles égales (jour abrégé en italique,
  numéro en Cormorant, ✓ ou ·). Écrit : plein `--ink-blue` ; non écrit : `--paper-raised`
  bordé `--rule` ; futur : pointillés, non cliquable ; aujourd'hui : anneau `--accent`.
- **Carte de statistique** (`StatCard`) : fond `--paper-raised`, bordure `--rule`, coins à peine
  arrondis (2px), grand chiffre, libellé en gras, légende en italique.
- **Calendrier** (`WritingCalendar`) : une colonne par semaine (lundi → dimanche), une case
  par jour, chacune cliquable (léger grossissement au survol, bulle explicative).
  - écrit : `--ink-blue`
  - non écrit (« silence ») : `--paper-deep`
  - futur : transparent, contour pointillé `--rule`
  - aujourd'hui : anneau `--accent`
- **Liste d'historique** (`HistoryList`) : une ligne par modification, séparées par des filets
  `--rule`, précédée d'un petit carré (`--ink-blue` pour « écrit », creux pour « non écrit »).
- **Bandeau d'annulation** (`UndoToast`) : en bas au centre, fond `--ink`, texte `--paper`,
  « Annuler » souligné en gras. Seule ombre portée de l'interface, très diffuse.
- **Colophon** : courte devise centrée en italique (« Écrire un peu, souvent. »).

## Principes

- Pas d'ombres portées marquées, pas de dégradés, pas d'icônes décoratives
  (les seuls signes sont typographiques : ✎, ✓, ·).
- Tout élément cliquable est un vrai bouton : accessible au clavier, avec un anneau de focus
  `--accent` et un libellé lu par les lecteurs d'écran.
- Les animations sont brèves et désactivées si l'utilisateur préfère réduire les mouvements.
- Les coins restent presque droits (2px).
- La mise en page s'adapte au mobile : le bandeau passe sur deux lignes, les cartes se replient
  en grille automatique, le calendrier défile horizontalement si nécessaire.
- Langue de l'interface : **français**, dates au format long (« dimanche 8 novembre 2026 »).
- Thème sombre : non prévu pour l'instant.
