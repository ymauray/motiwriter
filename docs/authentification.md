# Authentification

## Principe

MotiWriter **ne stocke aucun mot de passe** et n'envoie **aucun code à usage unique** :
on se connecte uniquement avec un compte existant chez **Discord**, **Twitch**, **Google**
ou **Apple** (OAuth 2.0 / OpenID Connect).

- **Données minimales.** On ne demande que l'identifiant stable de la personne chez le
  fournisseur et son nom d'affichage. **Jamais l'adresse e-mail** (portées : Discord `identify`,
  Twitch aucune, Google `openid profile`, Apple `openid name`).
- **Un compte par fournisseur.** La clé d'un compte est le couple (fournisseur, identifiant).
  Se connecter avec Discord puis avec Google crée deux comptes distincts : on ne relie pas
  les comptes par l'adresse e-mail, ce qui serait une source de prise de contrôle de compte.
- **Session** : cookie `MotiWriter` (HttpOnly, SameSite=Lax, Secure hors développement),
  valable 30 jours glissants. La déconnexion se fait par un formulaire POST protégé
  contre la falsification de requête (CSRF).
- **Données d'avant les comptes.** Les jours saisis avant l'arrivée de l'authentification
  sont adoptés par le **tout premier compte créé**. Connectez-vous donc vous-même en premier,
  avant de partager l'application.

Seuls les fournisseurs dont les identifiants sont configurés apparaissent sur la page de connexion.

## Configuration

Les identifiants se placent dans la section `Authentication` de la configuration. **Ils ne doivent
jamais être commités** : en développement, utilisez les *user secrets* (le projet Web est déjà
initialisé), en production des variables d'environnement (`Authentication__Discord__ClientId`, etc.).

```bash
cd src/MotiWriter.Web

dotnet user-secrets set "Authentication:Discord:ClientId"     "…"
dotnet user-secrets set "Authentication:Discord:ClientSecret" "…"

dotnet user-secrets set "Authentication:Twitch:ClientId"      "…"
dotnet user-secrets set "Authentication:Twitch:ClientSecret"  "…"

dotnet user-secrets set "Authentication:Google:ClientId"      "…"
dotnet user-secrets set "Authentication:Google:ClientSecret"  "…"

dotnet user-secrets set "Authentication:Apple:ClientId"       "…"   # identifiant du Services ID (pas de l'App ID), ex. ch.exemple.motiwriter.web
dotnet user-secrets set "Authentication:Apple:TeamId"         "…"
dotnet user-secrets set "Authentication:Apple:KeyId"          "…"
dotnet user-secrets set "Authentication:Apple:PrivateKey"     "$(cat AuthKey_XXXXXXXXXX.p8)"

dotnet user-secrets list   # vérifier
```

### Adresses de retour à déclarer

Chaque fournisseur demande l'adresse exacte vers laquelle il renvoie après la connexion.
Lancez l'application avec le profil `https` (`https://localhost:7096`) et déclarez :

| Fournisseur | Adresse de retour en développement |
|---|---|
| Discord | `https://localhost:7096/signin-discord` |
| Twitch | `https://localhost:7096/signin-twitch` |
| Google | `https://localhost:7096/signin-google` |
| Apple | *pas de `localhost` possible, voir plus bas* — en production : `https://<domaine>/signin-apple` |

En production, ajoutez les mêmes chemins sur votre domaine.

### Discord

1. <https://discord.com/developers/applications> → **New Application**.
2. **OAuth2** : copiez le *Client ID*, générez un *Client Secret*.
3. **Redirects** : ajoutez l'adresse de retour.

### Twitch

1. <https://dev.twitch.tv/console/apps> → **Register Your Application** (catégorie *Website Integration*).
2. **OAuth Redirect URLs** : ajoutez l'adresse de retour.
3. Copiez le *Client ID*, générez un *Client Secret* (type de client : *Confidential*).

### Google

1. <https://console.cloud.google.com/> → créez un projet.
2. **Google Auth Platform** (écran de consentement) : type *Externe*, nom de l'application,
   portées `openid` et `profile` uniquement. Tant que l'application est en mode *Test*,
   ajoutez les adresses de vos amis comme *utilisateurs de test*.
3. **Clients** → **Créer un client** → *Application Web* ; *URI de redirection autorisés* :
   l'adresse de retour.

### Apple

Sign in with Apple demande un **compte Apple Developer payant** et un **vrai domaine en HTTPS** :
Apple refuse `localhost` comme adresse de retour. Pour tester en local, on passe par un tunnel
HTTPS à domaine fixe, par exemple le domaine statique gratuit de ngrok.

Apple distingue trois éléments, tous dans <https://developer.apple.com/account> →
*Certificates, Identifiers & Profiles* :

| Élément | Rôle | Secret correspondant |
|---|---|---|
| **App ID** | l'« application » à laquelle Sign in with Apple est rattaché | — |
| **Services ID** | l'identifiant utilisé par un **site web** ; porte le domaine et l'adresse de retour | `ClientId` |
| **Key** (`.p8`) | clé privée qui signe le secret client (généré par MotiWriter) | `KeyId`, `PrivateKey` |

Le *Team ID* (10 caractères) figure en haut à droite du portail → `TeamId`.

#### 1. App ID

1. *Identifiers* → **+** → **App IDs** → *Continue* → type **App** → *Continue*.
2. *Description* : `MotiWriter` ; *Bundle ID* explicite, par ex. `ch.exemple.motiwriter`.
3. Dans *Capabilities*, cochez **Sign In with Apple**, puis *Edit* :
   - laissez **Enable as a primary App ID** ;
   - laissez **vide** le champ *Server-to-Server Notification Endpoint* (inutile ici).
4. *Save*, *Continue*, *Register*.

> Cet écran de l'App ID ne contient **ni domaine ni adresse de retour** : c'est normal, ils se
> déclarent sur le Services ID (étape 2).

#### 2. Services ID (le `ClientId`)

1. *Identifiers* → **+** → **Services IDs** → *Continue*.
2. *Description* : `MotiWriter Web` ; *Identifier* : **différent de l'App ID**, par ex.
   `ch.exemple.motiwriter.web`. *Continue*, *Register*.
3. Dans la liste *Identifiers*, basculez le filtre en haut à droite sur **Services IDs**, ouvrez
   celui que vous venez de créer, cochez **Sign In with Apple** → **Configure** :
   - *Primary App ID* : l'App ID de l'étape 1 ;
   - *Domains and Subdomains* : le domaine public, **sans** `https://` ni chemin
     (ex. `mon-domaine.ngrok-free.dev`) ;
   - *Return URLs* : `https://<domaine>/signin-apple`.
4. *Next*, *Done*, *Continue*, **Save**.

#### 3. Clé privée

1. *Keys* → **+** → nom `MotiWriter`, cochez **Sign In with Apple** → *Configure* → choisissez
   l'App ID de l'étape 1 → *Save*, *Continue*, *Register*.
2. **Téléchargez** le fichier `AuthKey_XXXXXXXXXX.p8` : Apple ne le propose **qu'une seule fois**.
   Notez le *Key ID* (les 10 caractères du nom du fichier).

#### 4. Secrets

```bash
cd src/MotiWriter.Web
dotnet user-secrets set "Authentication:Apple:ClientId"   "ch.exemple.motiwriter.web"   # Services ID
dotnet user-secrets set "Authentication:Apple:TeamId"     "XXXXXXXXXX"
dotnet user-secrets set "Authentication:Apple:KeyId"      "XXXXXXXXXX"
dotnet user-secrets set "Authentication:Apple:PrivateKey" "$(cat ~/chemin/AuthKey_XXXXXXXXXX.p8)"
```

Rangez ensuite le `.p8` en lieu sûr, hors du dépôt. Le bouton « Continuer avec Apple »
n'apparaît que si les quatre valeurs sont présentes.

#### 5. Lancer avec ngrok

Réservez un domaine statique gratuit dans le tableau de bord ngrok (*Domains*), puis, dans deux
terminaux :

```bash
dotnet run --project src/MotiWriter.Web --launch-profile https
ngrok http http://localhost:5006 --url=<domaine>.ngrok-free.dev
```

Ouvrez l'application **par l'adresse ngrok** (`https://<domaine>.ngrok-free.dev`), et non par
`localhost`, du début à la fin de la connexion : les cookies de connexion sont liés au domaine.
ngrok transmet en HTTP à l'application ; celle-ci retrouve l'adresse publique HTTPS grâce aux
en-têtes `X-Forwarded-*` (sans eux, elle redirigerait vers `https://<domaine>:7096` et tournerait
en boucle). Si un ancien cookie gêne, utilisez une fenêtre de navigation privée.

#### Le nom de la personne

Apple n'inscrit pas le nom dans le jeton : il l'envoie à part (champ de formulaire `user`) et
**seulement lors de la toute première autorisation**. MotiWriter le lit à ce moment-là ; ensuite,
il garde le nom déjà connu (« Plume anonyme » si la personne a refusé de le partager).

Pour qu'Apple le renvoie (par ex. pour un compte créé sans nom) : sur <https://account.apple.com>
→ *Connexion et sécurité* → *Se connecter avec Apple* → MotiWriter → **Arrêter d'utiliser**,
puis reconnectez-vous. Le compte MotiWriter est retrouvé (même identifiant Apple) et renommé.

#### Erreurs fréquentes

| Symptôme | Cause probable |
|---|---|
| Pas de bouton Apple | une des quatre valeurs manque dans les secrets |
| `invalid_client` chez Apple | `ClientId` = App ID au lieu du Services ID, ou clé non rattachée au bon App ID |
| `invalid_request` / adresse de retour refusée | domaine ou *Return URL* du Services ID différent de l'adresse ngrok |
| Redirections en boucle | application ouverte par `localhost`, ou proxy non local (voir *Production*) |
| « Plume anonyme » | première autorisation déjà consommée : voir ci-dessus |

## Connexion de développement

Pour travailler sans configurer de fournisseur, une connexion « Compte de développement » existe.
Elle n'est active **qu'en environnement Development** et **seulement si on l'active explicitement** :

```bash
Authentication__Development__Enabled=true dotnet run --project src/MotiWriter.Web --launch-profile https
```

Attention : elle crée un compte comme un autre. Si c'est le premier compte de la base, c'est lui
qui adopte les données existantes.

## Mise en production

- Servir l'application **en HTTPS** (les cookies sont marqués *Secure*).
- **Persister les clés de Data Protection** (qui chiffrent les cookies) dans un dossier durable,
  sinon toutes les sessions sont perdues à chaque redémarrage ou déploiement.
- Derrière un reverse proxy : l'application lit déjà `X-Forwarded-Proto`, `X-Forwarded-Host` et
  `X-Forwarded-For`, mais ne les croit que venant d'un proxy local (127.0.0.1, ::1). Si le proxy
  est sur une autre machine, déclarez son adresse dans `ForwardedHeadersOptions.KnownProxies`
  (ou `KnownNetworks`), sinon les adresses de retour seront construites en `http://`.
