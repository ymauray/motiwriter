# MotiWriter

[![CI](https://github.com/ymauray/motiwriter/actions/workflows/ci.yml/badge.svg)](https://github.com/ymauray/motiwriter/actions/workflows/ci.yml)
[![repocheck](https://img.shields.io/badge/repocheck%201.3.0-100%2F100-brightgreen)](https://github.com/ymauray/repocheck)

**Écrire un peu, souvent.**

MotiWriter est une application qui aide à garder l'envie d'écrire en montrant ce qu'on a
déjà accompli. On note simplement les jours où l'on a écrit — quelques mots suffisent —
et un tableau de bord présente le chemin parcouru : jours écrits, séries, calendrier.

🌐 Site de présentation : <https://ymauray.github.io/motiwriter/>

> 🚧 Projet jeune : les statistiques du tableau de bord sont encore provisoires.

## Fonctionnalités

- Un clic pour noter qu'on a écrit aujourd'hui ; rattrapage des jours passés depuis les sept
  derniers jours ou le calendrier ; décocher un jour se fait d'un clic.
- Historique de chaque modification, mois par mois (« Le mardi 3 novembre a été marqué écrit le dimanche 8 novembre à 19 h 42. »).
- Tableau de bord : nombre de jours écrits, série en cours, plus longue série, jours du mois.
- Calendrier jour par jour, qui remplit toute la largeur de l'écran (jusqu'à un an).
- Connexion avec Discord, Twitch, Google ou Apple, sans mot de passe ; chaque personne ne voit que ses jours.
- Données enregistrées dans un fichier SQLite.
- Interface en français, sobre, dans un esprit « papier et encre ».

Le métier est séparé de l'interface pour pouvoir être exposé plus tard en API REST
(applications iOS et Android) et en serveur MCP.

## Prérequis

- [SDK .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.403 ou plus récent, voir `global.json`)
- Une connexion internet pour charger les polices depuis Google Fonts (sinon, repli sur Georgia)

## Démarrer en développement

```bash
git clone https://github.com/ymauray/motiwriter.git
cd motiwriter

dotnet build
dotnet run --project src/MotiWriter.Web --launch-profile http
```

L'application est disponible sur <http://localhost:5006>.
Avec le profil `https` : <https://localhost:7096> (exécuter `dotnet dev-certs https --trust` la première fois).

Rechargement à chaud pendant le développement :

```bash
dotnet watch --project src/MotiWriter.Web
```

## Données

Les jours sont enregistrés dans `src/MotiWriter.Web/App_Data/motiwriter.db`, créé au premier
lancement (le dossier est ignoré par git). Pour utiliser un autre fichier :

```bash
ConnectionStrings__MotiWriter="Data Source=/chemin/vers/motiwriter.db" \
  dotnet run --project src/MotiWriter.Web --launch-profile http
```

## Connexion

Sans configuration, la page de connexion ne propose aucun fournisseur. Deux possibilités :

- configurer un ou plusieurs fournisseurs OAuth avec `dotnet user-secrets`
  (marche à suivre : [docs/authentification.md](docs/authentification.md)) ;
- ou, pour développer, activer le compte de développement :

```bash
Authentication__Development__Enabled=true \
  dotnet run --project src/MotiWriter.Web --launch-profile https
```

## Tests

```bash
dotnet test
```

## Binaire autonome

L'application peut être publiée en **un seul exécutable** qui embarque le runtime .NET et
ASP.NET Core : rien à installer sur la machine cible.

```bash
dotnet publish src/MotiWriter.Web -c Release -r osx-arm64 -o publish/osx-arm64
# autres cibles : osx-x64, linux-x64, linux-arm64, win-x64…
```

Le dossier obtenu (environ 50 Mo) se copie tel quel :

```
MotiWriter.Web                                   exécutable (runtime, DLL et SQLite inclus)
appsettings.json                                 configuration
wwwroot/                                         feuilles de style et scripts
MotiWriter.Web.staticwebassets.endpoints.json    index des fichiers de wwwroot
```

```bash
./publish/osx-arm64/MotiWriter.Web --urls http://localhost:5000
```

- Les fichiers sont cherchés **à côté de l'exécutable**, quel que soit le dossier courant ; la base
  est créée dans `App_Data/` à côté de lui (modifiable avec `ConnectionStrings__MotiWriter`).
- L'exécutable tourne en environnement *Production* : `dotnet user-secrets` n'y est **pas** lu.
  Les identifiants OAuth se donnent par variables d'environnement, par exemple
  `Authentication__Discord__ClientId`, `Authentication__Apple__PrivateKey="$(cat AuthKey.p8)"`.
- Sous Linux, la bibliothèque ICU (`libicu`) doit être installée pour les dates en français.
- Pour une mise en ligne (HTTPS, proxy, clés Data Protection), voir
  [docs/authentification.md](docs/authentification.md#mise-en-production).

Les exécutables prêts à l'emploi (Linux, macOS, Windows) sont joints à chaque
[release](https://github.com/ymauray/motiwriter/releases). Pour en publier une :

```bash
git tag v1.0.0
git push origin v1.0.0
```

Sur macOS, le binaire n'est pas signé : après extraction, `xattr -d com.apple.quarantine MotiWriter.Web`.

## Installation sur un serveur Ubuntu avec systemd, derrière un reverse proxy nginx

Cette marche à suivre installe une release sur **Ubuntu 24.04** : l'application tourne comme
service systemd sous un utilisateur dédié, n'écoute que sur `127.0.0.1`, et nginx la publie en
HTTPS. Dans les exemples, remplacez `motiwriter.exemple.ch` par votre domaine ; il doit déjà
pointer vers le serveur, ports 80 et 443 ouverts.

| Emplacement | Contenu | Propriétaire |
|---|---|---|
| `/opt/motiwriter/current` | lien vers la version installée (exécutable, `wwwroot/`) | root, lecture seule |
| `/etc/motiwriter/` | configuration et secrets (`motiwriter.env`, clé Apple) | root, lisible par le service |
| `/var/lib/motiwriter/` | base SQLite et clés Data Protection (cookies de session) | `motiwriter` |

### 1. Paquets

```bash
sudo apt update
sudo apt install -y nginx certbot python3-certbot-nginx libicu74   # Ubuntu 22.04 : libicu70
```

`libicu` fournit les règles du français (dates, noms de mois) à .NET.

### 2. Utilisateur du service

```bash
sudo useradd --system --home-dir /var/lib/motiwriter --shell /usr/sbin/nologin motiwriter
sudo install -d -m 755 /opt/motiwriter
sudo install -d -m 750 -o root -g motiwriter /etc/motiwriter
```

Le dossier `/var/lib/motiwriter` sera créé par systemd (`StateDirectory`), avec les bons droits.

### 3. Exécutable

```bash
VERSION=0.1.1
case "$(dpkg --print-architecture)" in
  amd64) RID=linux-x64 ;;
  arm64) RID=linux-arm64 ;;
esac
BASE=https://github.com/ymauray/motiwriter/releases/download/v$VERSION

cd /tmp
curl -fsSLO "$BASE/motiwriter-$VERSION-$RID.tar.gz"
curl -fsSLO "$BASE/SHA256SUMS.txt"
sha256sum --check --ignore-missing SHA256SUMS.txt

sudo tar -xzf "motiwriter-$VERSION-$RID.tar.gz" -C /opt/motiwriter
sudo ln -sfn "/opt/motiwriter/motiwriter-$VERSION-$RID" /opt/motiwriter/current
```

### 4. Configuration et secrets

Créez `/etc/motiwriter/motiwriter.env` (`sudo nano /etc/motiwriter/motiwriter.env`) en ne
gardant que les fournisseurs configurés :

```ini
ASPNETCORE_URLS=http://127.0.0.1:5000
ConnectionStrings__MotiWriter="Data Source=/var/lib/motiwriter/motiwriter.db"

Authentication__Discord__ClientId=…
Authentication__Discord__ClientSecret=…
Authentication__Twitch__ClientId=…
Authentication__Twitch__ClientSecret=…
Authentication__Google__ClientId=…
Authentication__Google__ClientSecret=…
Authentication__Apple__ClientId=…
Authentication__Apple__TeamId=…
Authentication__Apple__KeyId=…
Authentication__Apple__PrivateKeyPath=/etc/motiwriter/AuthKey_XXXXXXXXXX.p8
```

```bash
sudo chmod 600 /etc/motiwriter/motiwriter.env            # lu par systemd (root) seulement
# Pour Apple, copiez la clé .p8 sur le serveur, puis :
sudo install -m 640 -o root -g motiwriter AuthKey_XXXXXXXXXX.p8 /etc/motiwriter/
```

Chez chaque fournisseur, ajoutez l'adresse de retour du serveur :
`https://motiwriter.exemple.ch/signin-discord` (et `-twitch`, `-google`, `-apple` ; pour Apple,
le domaine aussi, dans le Services ID). Détails : [docs/authentification.md](docs/authentification.md).

### 5. Service systemd

`/etc/systemd/system/motiwriter.service` :

```ini
[Unit]
Description=MotiWriter
After=network.target

[Service]
Type=simple
User=motiwriter
Group=motiwriter
ExecStart=/opt/motiwriter/current/MotiWriter.Web
EnvironmentFile=/etc/motiwriter/motiwriter.env
# /var/lib/motiwriter : base, clés Data Protection ($HOME/.aspnet) et bibliothèques extraites ($HOME/.net)
StateDirectory=motiwriter
StateDirectoryMode=0750
WorkingDirectory=/var/lib/motiwriter
Restart=on-failure
RestartSec=5
SyslogIdentifier=motiwriter

# Durcissement
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ProtectKernelTunables=true
ProtectControlGroups=true

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now motiwriter
systemctl status motiwriter
curl -s -o /dev/null -w '%{http_code}\n' http://127.0.0.1:5000/connexion   # 200
journalctl -u motiwriter -f                       # journal en direct
```

### 6. nginx et HTTPS

`/etc/nginx/sites-available/motiwriter` :

```nginx
# Blazor garde une connexion WebSocket ouverte : il faut relayer l'en-tête Upgrade.
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}

server {
    listen 80;
    listen [::]:80;
    server_name motiwriter.exemple.ch;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade           $http_upgrade;
        proxy_set_header   Connection        $connection_upgrade;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Forwarded-Host  $host;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_read_timeout 1h;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/motiwriter /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
sudo certbot --nginx -d motiwriter.exemple.ch             # certificat Let's Encrypt + redirection HTTPS
sudo ufw allow 'Nginx Full'                                # si le pare-feu ufw est actif
```

Certbot ajoute le bloc HTTPS et renouvelle le certificat tout seul. L'application lit les en-têtes
`X-Forwarded-*` envoyés par nginx (elle ne les accepte que d'un proxy local) : elle sait ainsi
qu'elle est servie en `https://motiwriter.exemple.ch` et construit les bonnes adresses de retour.

### 7. Mise à jour

```bash
VERSION=0.2.0   # nouvelle version ; RID et BASE comme à l'étape 3
# … téléchargement et vérification comme à l'étape 3, puis :
sudo tar -xzf "motiwriter-$VERSION-$RID.tar.gz" -C /opt/motiwriter
sudo ln -sfn "/opt/motiwriter/motiwriter-$VERSION-$RID" /opt/motiwriter/current
sudo systemctl restart motiwriter
```

La base est mise à niveau automatiquement au démarrage. Pour revenir en arrière, refaites pointer
`current` vers l'ancien dossier et redémarrez (tant que la nouvelle version n'a pas modifié le
schéma de la base).

### Sauvegarde

Tout l'état est dans `/var/lib/motiwriter` : la base et les clés qui chiffrent les cookies de
session (sans elles, chacun doit se reconnecter). Le plus simple :

```bash
sudo systemctl stop motiwriter
sudo tar -czf motiwriter-sauvegarde-$(date +%F).tar.gz -C /var/lib motiwriter
sudo systemctl start motiwriter
```

## Intégration continue et releases

Tout passe par GitHub Actions ; aucun outil externe n'intervient.

| Workflow | Déclenchement | Rôle |
|---|---|---|
| [`ci.yml`](.github/workflows/ci.yml) | push sur `main` (hors changements limités à `docs/`, `site/` et Markdown), pull request, à la demande | compilation Release sans avertissement, tests ; check `Compilation et tests`, requis pour fusionner |
| [`release.yml`](.github/workflows/release.yml) | tag `vX.Y.Z` | rejoue la CI, publie les exécutables autonomes (Linux, macOS, Windows), essaie celui de Linux, crée la [release](https://github.com/ymauray/motiwriter/releases) avec archives et `SHA256SUMS.txt` |
| [`pages.yml`](.github/workflows/pages.yml) | push sur `main` touchant `site/`, à la demande | publie le [site de présentation](https://ymauray.github.io/motiwriter/) sur GitHub Pages |

[Dependabot](.github/dependabot.yml) propose chaque semaine les mises à jour des actions, des paquets
NuGet et du SDK .NET, sous forme de pull requests vérifiées par la CI. Détails :
[docs/architecture.md](docs/architecture.md#intégration-continue).

## Structure

```
MotiWriter.slnx
src/
  MotiWriter.Core/            Métier : saisie des jours, historique, statistiques, tableau de bord
  MotiWriter.Infrastructure/  Stockage SQLite
  MotiWriter.Web/             Interface web Blazor (.NET 10)
tests/
  MotiWriter.Core.Tests/            Tests du métier (xUnit)
  MotiWriter.Infrastructure.Tests/  Tests du stockage SQLite (xUnit)
docs/                         Documentation du projet
site/                         Site de présentation (GitHub Pages)
```

## Documentation

- [Objectif](docs/objectif.md)
- [Charte graphique](docs/charte-graphique.md)
- [Architecture](docs/architecture.md)
- [Authentification](docs/authentification.md)
- [Feuille de route](docs/feuille-de-route.md)
- [Consignes de travail (humains et agents IA)](AGENTS.md) : ordre de lecture, règles, pièges connus
- [Contribuer](CONTRIBUTING.md) · [Code de conduite](CODE_OF_CONDUCT.md) · [Sécurité](SECURITY.md) · [Aide](.github/SUPPORT.md)

## Licence

[MIT](LICENSE) © 2026 Yannick Mauray
