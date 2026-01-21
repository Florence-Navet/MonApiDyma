SELECT * FROM Clients 

INSERT INTO Clients (Id, IdAdresse, NomSociete)
VALUES ('BOLID', '0cac718e-bd21-4403-b155-4f994cea9ccc', 'Bolid SA');


-- Voir les clients disponibles
SELECT Id, NomSociete FROM Clients;

SELECT * FROM Employes WHERE Id = 5;

SELECT * FROM Livreurs WHERE Id = 1;


INSERT INTO Livreurs (Id, NomSociete, Telephone)
VALUES (1, 'Speedy Express', '01-02-03-04-05');

SELECT * FROM Livreurs;