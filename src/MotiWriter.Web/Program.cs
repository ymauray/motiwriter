using System.Globalization;
using Microsoft.AspNetCore.HttpOverrides;
using MotiWriter.Core;
using MotiWriter.Infrastructure;
using MotiWriter.Web.Authentication;
using MotiWriter.Web.Components;

var builder = WebApplication.CreateBuilder(args);

var french = new CultureInfo("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = french;
CultureInfo.DefaultThreadCurrentUICulture = french;

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Métier
builder.Services.AddMotiWriterCore();

// Stockage : fichier SQLite (chemin relatif résolu depuis la racine du projet).
builder.Services.AddMotiWriterSqlite(
    builder.Configuration.GetConnectionString("MotiWriter")
        ?? throw new InvalidOperationException("Chaîne de connexion « MotiWriter » manquante."),
    builder.Environment.ContentRootPath);

// Connexion via Discord, Twitch, Google et Apple (OAuth) ; aucun mot de passe stocké.
builder.AddMotiWriterAuthentication();

// Derrière un proxy (ngrok, reverse proxy) : l'application doit voir l'adresse publique
// (https://domaine) et non celle du proxy (http://localhost:5006), sinon la redirection HTTPS
// boucle et les adresses de retour OAuth sont fausses. Par défaut, seuls les proxys locaux
// (127.0.0.1, ::1) sont crus ; en production, déclarer les autres dans KnownProxies/KnownNetworks.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                               | ForwardedHeaders.XForwardedProto
                               | ForwardedHeaders.XForwardedHost;
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapMotiWriterAuthentication();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
