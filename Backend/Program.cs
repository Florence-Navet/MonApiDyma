// Copyright (c) Duende Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Duende.Bff;
using Duende.Bff.Blazor;
using Duende.Bff.Yarp;
using Microsoft.IdentityModel.Tokens;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.Bff.AccessTokenManagement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddBff()
    .AddRemoteApis()
    .AddServerSideSessions() // Add in-memory implementation of server side sessions
    .AddBlazorServer();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc"; // auth par jeton ou oidc connect
        options.DefaultSignOutScheme = "oidc"; // page protege alors qu'il est pas authentifie, on redirige vers la page de login du serveur d'identite
    })
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Host-blazor"; // valeur par defaut 2 semaines
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://demo.duendesoftware.com"; // url de demo du serveur d'identite url de appsettings.json

        // id et code  secret de l'appli cliente
        options.ClientId = "Client1"; 
        options.ClientSecret = "Secret1";

        //type de flux de communiccation avec le serveur
        options.ResponseType = "code";
        options.ResponseMode = "query";


        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        //options.Scope.Add("api");
        options.Scope.Add("offline_access"); // pour utiliser le jeton d'actualisation


        // Active la recuperation des reventication sur le point de terminaison
        // des infos utilisateurs après obtention d'un jeton d'identité
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;

        //enrgistre les jetons d'accès et d'actualisation dans le cookie d'authentification
        options.SaveTokens = true;

        //options.DisableTelemetry = true;

        //paratmètre nécessaires  à OIDC pour valider les jeton
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

// Enregistre le service de gesion de jetons pour appli cliente avec utilisateurs
builder.Services.AddOpenIdConnectAccessTokenManagement();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//ajout des middlewares de gestion des requetes http
app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

//ajout des middlewares de gestion de l'authentification et de l'autorisation
app.UseAuthentication();
app.UseBff();
app.UseAuthorization();
app.UseAntiforgery();

// ajout des endpoints de gestion pour la connexion et la deconnexion
app.MapBffManagementEndpoints();


app.MapRazorPages();


app.MapControllers() // mapper les controllers de l'api
    .RequireAuthorization() // ajouter la politique d'authorisation par defaut 
    .AsBffApiEndpoint(); // indique qu'il s'agit de pt de terminaison locaux du backend

//mapp les endpoints e l'api externe sur l'API locale
app.MapRemoteBffApiEndpoint("/Regions", new Uri(app.Configuration["ApiUrl"] + "Regions"))
    .WithAccessToken(RequiredTokenType.User);



app.MapFallbackToFile("index.html");

app.Run();
