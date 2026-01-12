using Microsoft.EntityFrameworkCore;
using NorthWind.Entities;

namespace NorthWind.Data
{
    public class ContexteNorthwind : DbContext
    {
        public ContexteNorthwind(DbContextOptions<ContexteNorthwind> options) : base(options)
        {
        }

        public virtual DbSet<Adresse> Adresses { get; set; }
        public virtual DbSet<Employe> Employés { get; set; }
        public virtual DbSet<Region> Régions { get; set; }
        public virtual DbSet<Territoire> Territoires { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Adresse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();// ne pas generer de valeur automatiquement
                entity.Property(e => e.Ville).HasMaxLength(40);
                entity.Property(e => e.Pays).HasMaxLength(40);
                entity.Property(e => e.Tel).HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.CodePostal).HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.Region).HasMaxLength(40);
                entity.Property(e => e.Rue).HasMaxLength(100);
            });

            modelBuilder.Entity<Employe>(entity =>
            {
                entity.ToTable("Employes"); // nom de la table sans accent
                entity.HasKey(e => e.Id); // definition de la clé primaire

                entity.Property(e => e.Prenom).HasMaxLength(40);
                entity.Property(e => e.Nom).HasMaxLength(40);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.Photo).HasColumnType("image"); // definie le type sql
                entity.Property(e => e.Fonction).HasMaxLength(40);
                entity.Property(e => e.Civilite).HasMaxLength(40);

                // Relation de la table Employe sur elle-même 
                entity.HasOne<Employe>().WithMany().HasForeignKey(d => d.IdManager);

                // Relation Employe - Adresse de cardinalités 0,1 - 1,1
                entity.HasOne<Adresse>().WithOne().HasForeignKey<Employe>(d => d.IdAdresse)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Affectation>(entity =>
            {
                entity.ToTable("Affectations");
                entity.HasKey(e => new { e.IdEmploye, e.IdTerritoire }); // syntaxe anonyme 

                //entity.Property(e => e.IdTerritoire).HasMaxLength(20).IsUnicode(false); on peut mettre ça en commentaire car il deduit tout seul

                //entity.HasOne<Employe>().WithMany().HasForeignKey(a => a.IdEmploye); on peut le faire dans territoire c'es mieux
                //entity.HasOne<Territoire>().WithMany().HasForeignKey(a => a.IdTerritoire);
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("Regions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Nom).HasMaxLength(40);

                //entity.HasMany<Territoire>().WithOne().HasForeignKey(d => d.IdRegion)
                //    .OnDelete(DeleteBehavior.NoAction); moins conseille
            });


            modelBuilder.Entity<Territoire>(entity =>
            {
                entity.ToTable("Territoires");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.Nom).HasMaxLength(40);

                entity.HasOne<Region>().WithMany().HasForeignKey(d => d.IdRegion)
                                .OnDelete(DeleteBehavior.NoAction); //utiliser plutot l'entite fille

                // Crée la relation N-N avec Employe en utilisant l'entité Affectation comme entité d'association
                entity.HasMany<Employe>().WithMany().UsingEntity<Affectation>(
                    l => l.HasOne<Employe>().WithMany().HasForeignKey(a => a.IdEmploye),
                    r => r.HasOne<Territoire>().WithMany().HasForeignKey(a => a.IdTerritoire));
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                modelBuilder.Entity<Employe>().HasData(
                new Employe
                {
                    Id = 11,
                    Nom = "Prégent",
                    Prenom = "Eric",
                    IdManager = 2,
                    Fonction = "Sales Representative",
                    Civilite = "Mr.",
                    DateNaissance = new DateTime(2000, 5, 20),
                    DateEmbauche = new DateTime(2023, 10, 11),
                    IdAdresse = new Guid("01fcbc07-b6ba-4f3a-ac69-891e5a41b14e")
                },
                new Employe
                {
                    Id = 12,
                    Nom = "Rignaut",
                    Prenom = "Solène",
                    IdManager = 2,
                    Fonction = "Sales Representative",
                    Civilite = "Mrs.",
                    DateNaissance = new DateTime(2000, 5, 20),
                    DateEmbauche = new DateTime(2023, 10, 11),
                    IdAdresse = new Guid("01fcbc07-b6ba-4f3a-ac69-891e5a41b14e")
                });
            }






        }


    }
}
