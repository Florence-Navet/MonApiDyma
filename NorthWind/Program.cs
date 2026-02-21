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
                .AddNewtonsoftJson()
                .AddJsonOptions(opt =>
                opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
