using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Northwind.Data;
using Northwind.Services;
using System.Text.Json.Serialization;

namespace Northwind
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // R?cup?re la cha?ne de connexion ? la base dans les param?tres
            string? connect = builder.Configuration.GetConnectionString("NorthwindConnect");

            // Add services to the container.
            // Enregistre la classe de contexte de donn?es comme service
            // en lui indiquant la connexion ? utiliser, et d?sactive le suivi des modifications
            builder.Services.AddDbContext<ContexteNorthwind>(opt => opt
                .UseSqlServer(connect)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableSensitiveDataLogging());

            // Enregistre les services m?tier
            builder.Services.AddScoped<IServiceEmployes, ServiceEmployes>();
            builder.Services.AddScoped<IServiceCommandes, ServiceCommandes>();
            builder.Services.AddScoped<IServiceProduits, ServiceProduits>();
            builder.Services.AddScoped<IServiceClients, ServiceClients>();

            // Utilise Serilog comme unique fournisseur de journalisation
            /*var logger = new LoggerConfiguration()
				 .ReadFrom.Configuration(builder.Configuration)
				 .Enrich.FromLogContext()
				 .CreateLogger();
			builder.Logging.ClearProviders();
			builder.Logging.AddSerilog(logger);*/

            // Enregistre les contrôleurs et ajoute une option de sérialisation
            // pour interrompre les références circulaires infinies
            builder.Services.AddControllers()
                .AddNewtonsoftJson(opt => 
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddJsonOptions(opt =>
                opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //ajoute le service d'authentification par porteur de jetons JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    //url d'accès au serveur d'identite
                    options.Authority = builder.Configuration["IdentityServerUrl"];
                    options.TokenValidationParameters.ValidateAudience = false; //on ne valide pas l'audience du jeton

                    // Tolérance sur la durée de validité du jeton 
                    options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                });

            //ttes les requetes devront contenir un jeton d'authentification valide
            builder.Services.AddAuthorization(options =>
            {
                //specifie que tout utilisateur de l'api doit etre authentifié
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                    .Build();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Middleware personnalis? de gestion d'erreurs
            //app.UseMiddleware<CustomErrorResponseMiddleware>(app.Logger);

            app.MapControllers();

            app.Run();
        }
    }
}
