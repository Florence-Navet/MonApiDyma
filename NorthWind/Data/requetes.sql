-- initialise les variables

declare @idEmploye int = 10
declare @idAdresse uniqueidentifier


select @idAdresse = IdAdresse from Employes where Id = @idEmploye

--suppression enregistrement employe avec adresse et ses affectation
delete from employes where id = @idEmploye
delete from Adresses where id = @idAdresse
delete from Affectations where IdEmploye = @idEmploye

-- reinitationaliser la valeur de l'id sur la table employe à la valeur max deja presente
select @idEmploye = max(Id) from Employes
DBCC CHECKIDENT('Employes', RESEED, @idEmploye)

select * from Employes

-- Supression d'une commande et de ses lignes
declare @idCom int = 831
delete from Commandes where id = @idCom
select @idCom = max(Id) from Commandes
DBCC CHECKIDENT ('Commandes', RESEED, @idCom)

select * from Commandes where id = @idCom