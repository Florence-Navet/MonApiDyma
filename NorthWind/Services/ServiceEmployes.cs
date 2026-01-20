using Microsoft.EntityFrameworkCore;
using NorthWind.Data;
using NorthWind.Entities;

namespace NorthWind.Services
{

    public interface IServiceEmployes
    {
        Task<List<Employe>> ObtenirEmployes(string? rechercheNom, DateTime? dateEmbaucheMax);
        Task<Employe?> ObtenirEmploye(int id);
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
            return await req.ToListAsync();
        }

        public async Task<Employe?> ObtenirEmploye (int id)
        {
            return await _contexte.Employés.FindAsync(id);
        }
    }
}
