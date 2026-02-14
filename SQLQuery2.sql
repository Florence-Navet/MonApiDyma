SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME;


USE Northwind;

INSERT INTO Employes (Prenom, Nom, IdAdresse, Fonction, Civilite)
VALUES ('Test', 'Employe', '20F5B2E0-687B-44E8-8EB9-0009876D8C22', 'Développeur', 'M.');
-- D'abord supprimer dans le bon ordre (tables enfants d'abord)

EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
DELETE FROM Affectations;
DELETE FROM LignesCommandes;
DELETE FROM Commandes;
DELETE FROM Clients;
DELETE FROM Employes;
DELETE FROM Fournisseurs;
DELETE FROM Produits;
DELETE FROM Categories;
DELETE FROM Territoires;
DELETE FROM Regions;
DELETE FROM Livreurs;
DELETE FROM Adresses;

-- Vérifier
SELECT COUNT(*) AS NbAdresses FROM Adresses;
SELECT COUNT(*) AS NbEmployes FROM Employes	;
SELECT COUNT(*) AS NbFournisseurs FROM Fournisseurs;

SELECT COUNT(*) FROM Employes;

SELECT COUNT(*) FROM Commandes;

SELECT Id, IdAdresse FROM Commandes WHERE Id = 1;
SELECT COUNT(*) FROM LignesCommandes;

SELECT TOP 5 Id FROM Commandes;

SELECT TOP 5 IdCommande, IdProduit FROM LignesCommandes;