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

dotnet user-secrets set "Authentication:Apple:ClientId"       "…"   # Services ID, ex. ch.exemple.motiwriter
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
Apple refuse `localhost` comme adresse de retour. Pour tester en local, passez par un tunnel HTTPS
avec un domaine fixe, par exemple le domaine gratuit de ngrok :

```bash
dotnet run --project src/MotiWriter.Web --launch-profile https
ngrok http http://localhost:5006 --url=<domaine>.ngrok-free.dev
```

Ouvrez alors l'application **par l'adresse ngrok** (et non `localhost`), du début à la fin de
la connexion. L'application tient compte des en-têtes `X-Forwarded-*` envoyés par ngrok.

1. <https://developer.apple.com/account> → **Identifiers** → créez un *App ID* avec la capacité
   *Sign in with Apple*.
2. Créez un *Services ID* (c'est le `ClientId`), activez *Sign in with Apple*, configurez-le :
   domaine et adresse de retour `https://<domaine>/signin-apple`.
3. **Keys** → créez une clé avec *Sign in with Apple* ; téléchargez le fichier `.p8`
   (une seule fois possible) et notez son *Key ID*. Le *Team ID* figure en haut à droite du portail.
4. Apple ne transmet le nom de la personne **qu'à la toute première connexion** ; ensuite,
   MotiWriter garde le nom déjà connu (« Plume anonyme » si la personne a refusé de le partager).

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
