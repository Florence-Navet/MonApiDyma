
using Microsoft.EntityFrameworkCore;
using NorthWind.Data;
using NorthWind.Services;

namespace NorthWind
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //recupere la chain de connexion à la base dans les paramètres
            string? connect = builder.Configuration.GetConnectionString("NorthWindConnect");

            // Add services to the container.
            //enregitre la classe de contexte de données comme service
            // en lui indiquant la connexion à utiliser
            builder.Services.AddDbContext<ContexteNorthwind>(opt => opt.UseSqlServer(connect));

            //enregistre le service métier
            builder.Services.AddScoped<IServiceEmployes, ServiceEmployes>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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


            app.MapControllers();

            app.Run();
        }
    }
}
