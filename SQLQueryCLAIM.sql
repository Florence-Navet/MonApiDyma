use Northwind;

select * from AspNetUserClaims

select * from AspNetUsers

insert AspNetUserClaims (UserId, ClaimType, ClaimValue)
values ('2', 'Fonction', 'Direction')


UPDATE AspNetUserClaims 
SET ClaimValue = 'Directeur' 
WHERE Id = 1