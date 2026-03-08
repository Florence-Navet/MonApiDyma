using Duende.IdentityServer.Models;
using HoteIdentiyServer.Data;
using HoteIdentiyServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// capture les exceptions de bdd resolvables par migrations
// et envoie une réponse HTTP invitant à migrer la base (à utiliser en mode dev uniquement)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Ajoute des svc d'identités communs :
// - une interface de jetons pour générer des jetons afin de réinitialiser les mdp,
//   modifier l'email et modifier le n° de tel, et pour l'auth à 2 facteurs
// - configure l'auth pour utiliser les cookies d'identité
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true) // exige confirmation par mail
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// ajoute et configure le svc IdentityServer
builder.Services.AddIdentityServer(options =>
{
    options.Authentication.CoordinateClientLifetimesWithUserSession = true;
    options.UserInteraction.ErrorUrl = "/Error";
})
    // crée des identités
    .AddInMemoryIdentityResources(new IdentityResource[]
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource("entreprise", new[] { "entreprise" })
    })
    // Configure une appli cliente
    .AddInMemoryClients(new Client[] {
        new Client
        {
            ClientId = "Client1",
            ClientSecrets = { new Secret("Secret1".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code, // granttype doit fournir un code secret pour obtenir un jeton d'accès
            // Urls auxquelles envoyer les jetons
            RedirectUris = { "https://localhost:7189/signin-oidc" },
            // Urls de redirection après déconnexion
            PostLogoutRedirectUris = { "https://localhost:7189/signout-callback-oidc" },
            // Url pour envoyer une demande de déconnexion au serveur d'identité
            FrontChannelLogoutUri = "https://localhost:7189/signout-oidc",
            // Etendue d'API autorisée
            AllowedScopes = { "openid", "profile", "entreprise" },
            // autorise le client à utiliser un jeton d'actualisation
            AllowOfflineAccess = true // pour pouvoir utiliser le flux de rafraichissement de jetons
        }
    })
    // Indique d'utiliser ASP.Net core Identity pour la gestion des profils et revendications
    .AddAspNetIdentity<ApplicationUser>();

// ajouter la journalisation au niveau debug des événements émis par Duende
builder.Services.AddLogging(options =>
{
    options.AddFilter("Duende", LogLevel.Debug);
});

// construction de l'application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // on migre la base au moment de l'exécution de l'application en mode dev
}
else
{
    app.UseExceptionHandler("/Error");
    // ajoute un en-tête de réponse qui informe les navigateurs que le site ne doit être accessible qu'en HTTPS,
    // et que toute tentative future d'y accéder via HTTP doit être automatiquement convertie en HTTPS
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ajoute le middleware d'authentification avec IdentityServer dans le pipeline
app.UseIdentityServer();
app.UseAuthorization();
app.MapRazorPages();
app.Run();