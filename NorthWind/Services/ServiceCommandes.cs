using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Northwind.Data;
using Northwind.Entities;

namespace Northwind.Services
{
    public interface IServiceCommandes
    {
        Task<List<Commande>> ObtenirCommandes(int? idEmployé, string? idClient);
        Task<Commande?> ObtenirCommande(int id);
        Task<Commande?> AjouterCommande(Commande cde);
        Task<int> SupprimerCommande(int idCommande);
        Task<int> SupprimerCommande2(int idCommande);
        Task<int> SupprimerCommande3(int idCommande);
        Task<int> ModifierCommande(Commande cde);
        Task<int> ModifierCommande2(int idCommande, float tauxReduc);

        Task<LigneCommande?> AjouterLigneCommande(int idCommande, LigneCommande ligne);
        Task SupprimerLigneCommande(int idCommande, int idProduit);
        Task<int> SupprimerLigneCommande2(int idCommande, int idProduit);
        Task<int> ModifierLigneCommande(int idCommande, LigneCommande ligne);
        Task<int> ModifierLigneCommande2(int idCommande, LigneCommande ligne);
        Task<LigneCommande> ModifierLigneCommande3(int idCommande, int idProduit, short quantité);
    }

    public class ServiceCommandes : IServiceCommandes
    {
        private readonly ContexteNorthwind _contexte;

        public ServiceCommandes(ContexteNorthwind context)
        {
            _contexte = context;
        }

        #region Commandes
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
            await ControlerCommande(cde);

            _contexte.Commandes.Add(cde);
            await _contexte.SaveChangesAsync();
            return cde;
        }

        private async Task ControlerCommande(Commande cde)
        {
            ValidationRulesException vre = new();
            if (cde.DateCommande < DateTime.Today.AddDays(-3))
                vre.Errors.Add("DateCommande", new string[] { "La date de commande doit être > date du jour - 3 jours." });

            if (cde.FraisLivraison < 0 || cde.FraisLivraison > 2000)
                vre.Errors.Add("Frais", new string[] { "Les frais de livraison doivent être compris entre 0 et 2000 €" });

            if (vre.Errors.Any()) throw vre;

            foreach (var ligne in cde.Lignes)
                await ControlerLigneCommande(ligne);
        }

        // Supprime une commande et ses lignes grâce à la suppression en cascade
        public async Task<int> SupprimerCommande(int idCommande)
        {
            Commande commande = new() { Id = idCommande };
            _contexte.Entry(commande).State = EntityState.Deleted;
            return await _contexte.SaveChangesAsync();
        }

        // Supprime une commande et ses lignes via des requêtes SQL de masse
        public async Task<int> SupprimerCommande2(int idComm)
        {
            using (var transaction = _contexte.Database.BeginTransaction())
            {
                await _contexte.LignesCommandes.Where(c => c.IdCommande == idComm).ExecuteDeleteAsync();
                int nbSuppr = await _contexte.Commandes.Where(c => c.Id == idComm).ExecuteDeleteAsync();
                transaction.Commit();
                return nbSuppr;
            }
        }

        // Supprime une commande et ses lignes en chargeant tout d'abord les entités
        public async Task<int> SupprimerCommande3(int idCommande)
        {
            // Récupère la commande et ses lignes sans les rattacher au suivi
            var req = from c in _contexte.Commandes.Include(c => c.Lignes)
                      where c.Id == idCommande
                      select c;

            var cde = await req.FirstOrDefaultAsync();
            if (cde == null) return 0;

            // Passe la commande et ses lignes à l'état Deleted
            //_contexte.Remove(cde);
            _contexte.Entry(cde).State = EntityState.Deleted;
            foreach (var ligne in cde.Lignes)
            {
                _contexte.Entry(ligne).State = EntityState.Deleted;
            }

            return await _contexte.SaveChangesAsync();
        }

        // Modifie une commande avec ses lignes
        public async Task<int> ModifierCommande(Commande cde)
        {
            // Contrôle les données reçues
            await ControlerCommande(cde);

            // Passe la commande et ses lignes à l'état Modified
            _contexte.Entry(cde).State = EntityState.Modified; // avec entry on s'occupe pas des entités filles ni sur ses lignes
            foreach (var ligne in cde.Lignes)
            {
                _contexte.Entry(ligne).State = EntityState.Modified;
            }

            return await _contexte.SaveChangesAsync();
        }

        // Met à jour le taux de réduction sur toutes les lignes d'une commande
        //meme modif sur toutes les lignes de commande
        public async Task<int> ModifierCommande2(int idCommande, float tauxReduc)
        {
            int nbmodifs = await _contexte.LignesCommandes.Where(lc => lc.IdCommande == idCommande)
                .ExecuteUpdateAsync(setter => setter.SetProperty(l => l.TauxReduc, tauxReduc));

            return nbmodifs;
        }
        #endregion

        #region Lignes de commandes
        // Crée une ligne de commande pour une commande donnée
        public async Task<LigneCommande?> AjouterLigneCommande(int idCommande, LigneCommande ligne)
        {
            ligne.IdCommande = idCommande;
            ligne.Produit = null!;

            // Contrôle les données reçues
            await ControlerLigneCommande(ligne);

            _contexte.LignesCommandes.Add(ligne);
            await _contexte.SaveChangesAsync();

            return ligne;
        }

        // Supprime une ligne de commande
        public async Task SupprimerLigneCommande(int idCommande, int idProduit)
        {
            LigneCommande ligne = new()
            {
                IdCommande = idCommande,
                IdProduit = idProduit,
                //Produit = null!
            };

            //_contexte.Remove(ligne);
            //_contexte.Attach(ligne).State = EntityState.Deleted;
            _contexte.Entry(ligne).State = EntityState.Deleted;

            await _contexte.SaveChangesAsync();
        }

        // Supprime une ligne de commande en vérifiant tout d'abord qu'elle existe
        public async Task<int> SupprimerLigneCommande2(int idCommande, int idProduit)
        {
            LigneCommande? ligne = await _contexte.LignesCommandes.FindAsync(idCommande, idProduit);
            if (ligne == null) return 0;
            else ligne.Produit = null!;

            _contexte.Remove(ligne);
            return await _contexte.SaveChangesAsync();
        }

        public async Task<int> ModifierLigneCommande(int idCommande, LigneCommande ligne)
        {
            ligne.IdCommande = idCommande;
            // Remet les propriétés de navigation à null
            ligne.Produit = null!;

            // Contrôle les données reçues
            await ControlerLigneCommande(ligne);

            // Rattache l'entité au suivi en passant son état à Modified
            _contexte.Update(ligne);

            return await _contexte.SaveChangesAsync();
            // Lève une DbUpdateConcurrencyException si la ligne n'existe pas en base
        }

        public async Task<int> ModifierLigneCommande2(int idCommande, LigneCommande ligne)
        {
            ligne.IdCommande = idCommande;

            // Contrôle les données reçues
            await ControlerLigneCommande(ligne);

            // Rattache l'entité au suivi, sans ses filles, en passant son état à Modified
            EntityEntry<LigneCommande> ent = _contexte.Entry(ligne);
            ent.State = EntityState.Modified;

            // Empêche la modification du prix unitaire de la ligne
            ent.Property(l => l.PU).IsModified = false;

            return await _contexte.SaveChangesAsync();
        }

        public async Task<LigneCommande> ModifierLigneCommande3(int idCommande, int idProduit, short quantité)
        {
            // Récupère la ligne présente en base avec suivi des modifs
            var req = from l in _contexte.LignesCommandes.AsTracking()
                      where l.IdCommande == idCommande && l.IdProduit == idProduit
                      select l;

            LigneCommande? ligne = await req.SingleOrDefaultAsync();
            if (ligne == null) throw new DbUpdateConcurrencyException();

            // Modifie les propriétés souhaitées
            ligne.Quantite = quantité;
            ligne.Produit = null!;

            // Contrôle la ligne
            await ControlerLigneCommande(ligne);

            // Enregistre les modifications
            await _contexte.SaveChangesAsync();

            return ligne;
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
        #endregion
    }
}
