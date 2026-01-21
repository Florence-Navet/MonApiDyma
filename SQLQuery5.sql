SELECT * FROM Produits WHERE Id IN (5, 31);

INSERT INTO Produits (Id, Nom, IdCategorie, IdFournisseur, PU)
VALUES 
  (NEWID(), 'Produit Test 5', 1, 1, 21.35),
  (NEWID(), 'Produit Test 31', 1, 1, 12.50);

  SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Produits';

SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Produits' AND COLUMN_NAME = 'Id';

SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Produits' 
  AND COLUMN_NAME IN ('IdCategorie', 'IdFournisseur');

  -- D'abord, récupère un GUID de catégorie existant
SELECT TOP 1 Id FROM Categories;

INSERT INTO Categories (Id, Nom)
VALUES (NEWID(), 'Catégorie Test');

SELECT TOP 1 Id FROM Categories;


  INSERT INTO Produits (Nom, IdCategorie, IdFournisseur, PU, UnitesEnStock)
VALUES 
  ('Produit A', 'EA56EC73-72CA-4D0E-8BDD-AA6BBAEF75EA', 1, 21.35, 100),
  ('Produit B', 'EA56EC73-72CA-4D0E-8BDD-AA6BBAEF75EA', 1, 12.50, 50);

  INSERT INTO Produits (Nom, IdCategorie, IdFournisseur, PU, UnitesEnStock, NiveauReappro)
VALUES 
  ('Produit A', 'EA56EC73-72CA-4D0E-8BDD-AA6BBAEF75EA', 1, 21.35, 100, 10),
  ('Produit B', 'EA56EC73-72CA-4D0E-8BDD-AA6BBAEF75EA', 1, 12.50, 50, 5);

  SELECT Id FROM Fournisseurs;

  INSERT INTO Fournisseurs (NomSociete)
VALUES ('Fournisseur Caninos');


INSERT INTO Fournisseurs (NomSociete, IdAdresse)
VALUES ('Fournisseur Caninos', '0cac718e-bd21-4403-b155-4f994cea9ccc');

SELECT Id FROM Fournisseurs;

INSERT INTO Produits (Nom, IdCategorie, IdFournisseur, PU, UnitesEnStock, NiveauReappro)
VALUES 
  ('Produit A', 'EA56EC73-72CA-4D0E-8BDD-AA6BBAEF75EA', 3, 21.35, 100, 10),
  ('Produit B', 'EA56EC73-72CA-4D0E-8BDD-AA6BBAEF75EA', 3, 12.50, 50, 5);

  SELECT Id, Nom, PU FROM Produits;