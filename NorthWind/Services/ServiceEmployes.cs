using Microsoft.EntityFrameworkCore;
using NorthWind.Data;
using NorthWind.Entities;

namespace NorthWind.Services
{

    public interface IServiceEmployes
    {
        Task<List<Employe>> ObtenirEmployes(string? rechercheNom, DateTime? dateEmbaucheMax);
        Task<Employe?> ObtenirEmploye(int id);

         Task<Region?> ObtenirRégion(int id);
      Task<Employe> AjouterEmployé(Employe empl);
      Task<Affectation> AjouterAffectation(Affectation affect);

    }
    public class ServiceEmployes : IServiceEmployes
    {
        private readonly ContexteNorthwind _contexte;

        public ServiceEmployes(ContexteNorthwind contexte)
        {
            _contexte = contexte;
        }

        public async Task<List<Employe>> ObtenirEmployes(string? rechercheNom, DateTime? dateEmbaucheMax)
        {
         //pour optimiser la requete
         var req = from e in _contexte.Employés
                   where (rechercheNom == null || e.Nom.Contains(rechercheNom))
                   && (dateEmbaucheMax == null || e.DateEmbauche <= dateEmbaucheMax)
                   select new Employe
                   {
                      Id = e.Id,
                      Civilite = e.Civilite,
                      Nom = e.Nom,
                      Prenom = e.Prenom,
                      Fonction = e.Fonction,
                      DateEmbauche = e.DateEmbauche,
                   };

         // tri par date d'embauche décroissante
         if (dateEmbaucheMax != null)

            req = req.OrderByDescending(e => e.DateEmbauche);
         else
            req = req.OrderBy(e => e.Nom).ThenBy(e => e.Prenom);


            return await req.ToListAsync();
        }

        public async Task<Employe?> ObtenirEmploye (int id)
        {
         var req = from e in _contexte.Employés
                   .Include(e => e.Adresse)
                   .Include(e => e.Territoires)
                   .ThenInclude (t => t.Région)
                   where (e.Id == id) select e;
         return await req.FirstOrDefaultAsync();
            //return await _contexte.Employés.FindAsync(id);
        }



      //recupère une region et ses territoires

      public async Task<Region?> ObtenirRégion(int id)
      {
         var req = from r in _contexte.Régions.Include(r => r.Territoires)
                   where r.Id == id
                   select r;

         return await req.FirstOrDefaultAsync();
      }

      // Ajoute un employé
      public async Task<Employe> AjouterEmployé(Employe empl)
      {
         // Ajoute l'employé dans le DbSet
         _contexte.Employés.Add(empl);

         // Enregistre l'employé dans la base et affecte son Id
         await _contexte.SaveChangesAsync();

         return empl; // Renvoie l'employé avec son Id renseigné
      }

      public async Task<Affectation> AjouterAffectation(Affectation affect)
      {
         _contexte.Affectations.Add(affect);
         await _contexte.SaveChangesAsync();
         return affect;
      }

   }
}
