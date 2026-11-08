using System.Globalization;
using MotiWriter.Core;
using MotiWriter.Core.Journal;
using MotiWriter.Web.Components;
using MotiWriter.Web.Mock;

var builder = WebApplication.CreateBuilder(args);

var french = new CultureInfo("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = french;
CultureInfo.DefaultThreadCurrentUICulture = french;

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Métier
builder.Services.AddMotiWriterCore();

// Maquette : journal en mémoire pré-rempli. À remplacer par une vraie persistance.
builder.Services.AddSingleton<IWritingJournal>(sp =>
{
    var today = DateOnly.FromDateTime(TimeProvider.System.GetLocalNow().DateTime);
    return new InMemoryWritingJournal(SampleWritingDays.Generate(today));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
