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
//capture les exceptions de bdd resolvables par migrations
//et envoie une réponse HTTP invitant à migrer la base (à utilise en mode dev uniquement)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Ajoute des svc d 'identités communs :
// - une interface de jtons pour générer des jetons afin de reintinaliser les mdp,
// modifier l'email et modifier le n° de tel, et pour l'auth à 2 facteurs
// - configure l'auth pour utiliser les cookies d'identité
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true) // exige confirmation par mail
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddRazorPages();

// ajoute et configure le svc IdentityServer
builder.Services.AddIdentityServer(options =>
options.Authentication.CoordinateClientLifetimesWithUserSession = true)

    //cree des identites
    .AddInMemoryIdentityResources(new IdentityResource[]
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
    })

    //Configure une appi cliente
    .AddInMemoryClients(new Client[] {
    new Client
    {
        ClientId = "Client1",
        ClientSecrets = {new Secret("Secret1".Sha256())},
        AllowedGrantTypes = GrantTypes.Code, // granttype  doit fournir un code secret pour obtenir un jeton d'accès

        //Urls auxquelles envoyer les jetons
        RedirectUris = {"https://localhost:6001/signin-oidc"},//5001
        //Urls  de redirection après déconnexion
        PostLogoutRedirectUris = {"https://localhost:6001/signout-callback-oidc"},
        // Url pour envoyer une demande de deconnexion au serveur d'identité
        FrontChannelLogoutUri = "https://localhost:6001/signout-oidc",

        // Etendue d'API autorisée
        AllowedScopes = {"openid", "profile"},
    }
    })
    //Indique d'utiiser ASP. Net core Identity pour la gestion des profils et revendications
    .AddAspNetIdentity<ApplicationUser>();
//ajouter la journalisation au niveau debug des évènements émis par Duende
builder.Services.AddLogging(options =>
{
    options.AddFilter("Duende", LogLevel.Debug);
});

// construction de l'application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseMigrationsEndPoint(); // on migre la base au moment de l'éxécution de l'application en mode dev 
}
else
{
   app.UseExceptionHandler("/Error");
    // ajoute un en-tete de reponse qui informe les navigateurs que le site ne doit être accessible qu'en HTTPS,
    // et que toute tentative future d'y accéder via HTTO doit être automatiquement convertie en HTTPS
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//ajoute le middleware d'authentification avec IdentityServer dans le pipeline 
app.UseIdentityServer();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
