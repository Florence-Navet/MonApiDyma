using Microsoft.EntityFrameworkCore;
using Northwind.Services;
using NorthWind.Data;
using NorthWind.Entities;

namespace NorthWind.Services
{
   public interface IServiceCommandes
   {
      Task<List<Commande>> ObtenirCommandes(int? idEmployé, string? idClient);
      Task<Commande?> ObtenirCommande(int id);
      Task<Commande?> AjouterCommande(Commande cde);
      Task<LigneCommande?> AjouterLigneCommande(int idCommande, LigneCommande ligne);

        Task SupprimerLigneCommande(int idCommande, int idProduit);
   }

   public class ServiceCommandes : IServiceCommandes
   {
      private readonly ContexteNorthwind _contexte;

      public ServiceCommandes(ContexteNorthwind context)
      {
         _contexte = context;
      }

      // Renvoie les commandes liées à un employé et un client s'ils sont renseignés
      // sinon toutes les commandes
      public async Task<List<Commande>> ObtenirCommandes(int? idEmployé = null, string? idClient = null)
      {
         var req = from c in _contexte.Commandes
                   where (idEmployé == null || c.IdEmploye == idEmployé) &&
                         (idClient == null || c.IdClient == idClient)
                   select c;

         return await req.ToListAsync();
      }

      // Renvoie la commande d'id spécifié, avec ses lignes
      public async Task<Commande?> ObtenirCommande(int id)
      {
         var req = from c in _contexte.Commandes
                   .Include(c => c.Lignes)
                   where c.Id == id
                   select c;

         return await req.FirstOrDefaultAsync();
      }

        // Crée une commande pour un employé, une adresse, un client et
        // un livreur déjà existants en base
        public async Task<Commande?> AjouterCommande(Commande cde)
        {
            // On remet les propriétés de navigation correspondantes à null
            cde.Employe = null!;
            cde.Adresse = null!;
            cde.Livreur = null!;
            foreach (var ligne in cde.Lignes) ligne.Produit = null!;

            // Contrôle les données reçues
            ValidationRulesException vre = new();
            if (cde.DateCommande < DateTime.Today.AddDays(-3))
                vre.Errors.Add("DateCommande", new string[] { "La date de commande doit être > date du jour - 3 jours." });

            if (cde.FraisLivraison < 0 || cde.FraisLivraison > 2000)
                vre.Errors.Add("Frais", new string[] { "Les frais de livraison doivent être compris entre 0 et 2000 €" });

            if (vre.Errors.Any()) throw vre;

            foreach (var ligne in cde.Lignes)
                await ControlerLigneCommande(ligne);

            _contexte.Commandes.Add(cde);
            await _contexte.SaveChangesAsync();

            return cde;
        }

        // Crée une ligne de commande pour une commande donnée
        public async Task<LigneCommande?> AjouterLigneCommande(int idCommande, LigneCommande ligne)
      {
         ligne.IdCommande = idCommande;
         ligne.Produit = null!;

         _contexte.LignesCommandes.Add(ligne);
         await _contexte.SaveChangesAsync();

         return ligne;
      }

        //suppprime une ligne de commande
        public async Task SupprimerLigneCommande(int idCommande, int idProduit)
        {
            LigneCommande ligne = new()
            {
                IdCommande = idCommande,
                IdProduit = idProduit,
                Produit = null!
            };

            _contexte.Remove(ligne);

            // a la place de _contexte.Remove(ligne);
            // on peut faire : _contexte.Entry(ligne).State = EntityState.Deleted;
            // du coup, enlever la ligne suivante : ligne.Produit = null!
            // car Ef Core n'a pas besoin de charger le produit pour supprimer la ligne
            await _contexte.SaveChangesAsync();


        }



        private async Task ControlerLigneCommande(LigneCommande ligne)
        {
            Produit? produit = await _contexte.Produits.FindAsync(ligne.IdProduit);

            ValidationRulesException vre = new();
            if (produit == null || produit.Arrete)
                vre.Errors.Add("Produit.Arrete", new string[] { $"Le produit {ligne.IdProduit} n'existe pas ou a été arrêté." });

            if (produit != null && produit.UnitesEnStock < ligne.Quantite)
                vre.Errors.Add("Produit.UnitesEnStock", new string[] { $"La quantité en stock ({produit.UnitesEnStock}) du produit {ligne.IdProduit} est insuffisante." });

            if (vre.Errors.Any()) throw vre;
        }
    }
}
