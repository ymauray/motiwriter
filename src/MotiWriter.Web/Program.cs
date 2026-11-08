using System.Globalization;
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

var app = builder.Build();

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
